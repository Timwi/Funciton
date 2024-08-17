using System.Linq;
using System.Text;

namespace Funciton
{
    class FuncitonFunction(Node[] outputNodes, string name)
    {
        public Node[] OutputNodes { get; private set; } = outputNodes;
        public string Name { get; private set; } = name;

        public Node[] CloneOutputNodes(Node[] functionInputs)
        {
            FuncitonLanguage.CloneCounter++;
            return OutputNodes.Select(node => node?.CloneForFunctionCall(FuncitonLanguage.CloneCounter, functionInputs)).ToArray();
        }

        public void Analyze(StringBuilder sb)
        {
            LambdaExpressionParameterNode.LambdaParameterCounter = 0;

            // Pass one: determine which nodes are single-use and which are multi-use
            var nodes = FindNodes();

            // Pass two: generate expressions
            sb.AppendLine($"Analysis of {(Name == "" ? "main program" : $"{Name}({string.Join(", ", nodes.AllNodes.OfType<InputNode>().OrderByDescending(i => i.InputPosition).Select(i => "↑→↓←"[i.InputPosition]))})")}:");

            // Find functions or lambda invocations that return more than one value
            var letNodes = nodes.LetNodes.ToArray();
            var done = new bool[letNodes.Length];
            for (int i = 0; i < letNodes.Length; i++)
            {
                if (done[i])
                    continue;
                done[i] = true;

                var lion = letNodes[i] as LambdaInvocationOutputNode;
                var con = letNodes[i] as CallOutputNode;
                if (lion != null || (con != null && con.Call.Function.OutputNodes.Count(n => n != null) > 1))
                {
                    var belongingNodes = lion != null
                        ? letNodes
                            .Skip(i + 1)
                            .Select((ln, ix) => new { Node = ln as LambdaInvocationOutputNode, Index = ix + i + 1 })
                            .Where(x => x.Node != null && x.Node.Invocation == lion.Invocation)
                            .Select(x => new { x.Node.OutputPosition, x.Index })
                            .Concat([new { lion.OutputPosition, Index = i }])
                            .ToArray()
                        : letNodes
                            .Skip(i + 1)
                            .Select((ln, ix) => new { Node = ln as CallOutputNode, Index = ix + i + 1 })
                            .Where(x => x.Node != null && x.Node.Call == con.Call)
                            .Select(x => new { x.Node.OutputPosition, x.Index })
                            .Concat([new { con.OutputPosition, Index = i }])
                            .ToArray();

                    var outPosses = lion != null
                        ? [2, 1]
                        : con.Call.Function.OutputNodes
                            .Select((nd, outPos) => new { Node = nd, OutputPosition = outPos })
                            .Where(inf => inf.Node != null)
                            .Select(inf => inf.OutputPosition)
                            .ToArray();
                    var outputs = outPosses.Select(outPos =>
                    {
                        var nd = belongingNodes.FirstOrDefault(c => c.OutputPosition == outPos);
                        if (nd == null)
                            return null;
                        done[nd.Index] = true;
                        return new { Dir = "↑→↓←"[(nd.OutputPosition + 2) % 4], Letter = (char) ('a' + nd.Index) };
                    }).Where(inf => inf != null).ToArray();
                    var outputsStr = string.Join(", ", outputs.Select(op => op.Letter));
                    sb.AppendLine($"    let {(outputs.Length == 1 ? outputsStr : $"[{outputsStr}]")} := {letNodes[i].GetExpression(letNodes, true, false, false)}[{string.Join(", ", outputs.Select(op => op.Dir))}]");
                    continue;
                }

                sb.AppendLine($"    let {(char) ('a' + i)} := {letNodes[i].GetExpression(letNodes, true, false, false)}");
            }

            for (int i = 0; i < OutputNodes.Length; i++)
            {
                if (OutputNodes[i] == null)
                    continue;
                sb.Append("    output ");
                sb.Append("↓←↑→"[i]);
                sb.Append(" := ");
                sb.AppendLine(OutputNodes[i].GetExpression(letNodes, false, false, false));
            }
        }

        public FindNodesResult FindNodes()
        {
            var fnr = new FindNodesResult();
            foreach (var output in OutputNodes.Where(on => on != null))
                output.FindNodes(fnr);
            foreach (var node in fnr.SingleUseNodes.OfType<LambdaInvocationOutputNode>())
                if (fnr.LetNodes.OfType<LambdaInvocationOutputNode>().Any(lion => lion.Invocation == node.Invocation))
                    fnr.LetNodes.Add(node);
            return fnr;
        }

        public int? GetInputForOutputIfNop(int outputPosition) => OutputNodes[outputPosition] is InputNode input ? input.InputPosition : (int?) null;

        public override string ToString() => Name == "" ? "main program" : "function " + Name;
    }
}
