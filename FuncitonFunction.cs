using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace FuncitonInterpreter
{
    class FuncitonFunction
    {
        public abstract class Node
        {
            protected FuncitonFunction _thisFunction;
            public Node(FuncitonFunction thisFunction) { _thisFunction = thisFunction; }

            public Node Clone(Dictionary<object, object> cloned, Node[] functionInputs)
            {
                object node;
                if (cloned.TryGetValue(this, out node))
                    return (Node) node;
                return cloneImpl(cloned, functionInputs);
            }
            protected abstract Node cloneImpl(Dictionary<object, object> cloned, Node[] functionInputs);

            /// <summary>This function is designed to evaluate an entire Funciton program without using .NET’s own call stack (so that we are not limited to its size). See remarks for details.</summary>
            /// <param name="traceFunctions">A list of function names for which to output debug trace information.</param>
            /// <returns>A node to evaluate next, or null to indicate evaluation is complete. See remarks for details.</returns>
            /// <remarks>
            /// <para>The code contract is this:</para>
            /// <list type="bullet">
            /// <item><description>The caller calls <see cref="NextToEvaluate"/>.</description></item>
            /// <item><description>If <see cref="NextToEvaluate"/> returns null, node is fully evaluated and the result can be read from <see cref="Result"/>.</description></item>
            /// <item><description>If <see cref="NextToEvaluate"/> returns a node, the caller is expected to fully evaluate that node,
            /// read its result, store that in <see cref="PreviousSubresult"/> and then call <see cref="NextToEvaluate"/> again.</description></item>
            /// </list>
            /// </remarks>
            public Node NextToEvaluate(List<string> traceFunctions)
            {
                var res = nextToEvaluate();
                if (res == null && !(this is LiteralNode) && traceFunctions.Contains(_thisFunction.Name))
                {
                    if (_alreadyTraced == null)
                        _alreadyTraced = new HashSet<Node>();
                    if (_alreadyTraced.Add(this))
                        Console.WriteLine("{0}: {1} = {2} (\"{3}\")", _thisFunction.Name, getExpression(null, false), _result, Helpers.CLiteralEscape(FuncitonLanguage.IntegerToString(_result)));
                }
                return res;
            }
            private static HashSet<Node> _alreadyTraced;

            protected abstract Node nextToEvaluate();

            protected BigInteger _result;
            protected BigInteger _previousSubresult;

            /// <summary>See the remarks on <see cref="NextToEvaluate"/> for details. Until <see cref="NextToEvaluate"/> has returned null, this value is meaningless. Afterwards, it contains the result of evaluating this code.</summary>
            public BigInteger Result { get { return _result; } }

            /// <summary>See the remarks on <see cref="NextToEvaluate"/> for details. Write the result of a previous evaluation here. The previous subresult must be written before the next call to <see cref="NextToEvaluate"/> is made.</summary>
            public BigInteger PreviousSubresult { set { _previousSubresult = value; } }

            public FuncitonFunction FindFunction(string functionName, HashSet<Node> alreadyVisited)
            {
                if (alreadyVisited.Contains(this))
                    return null;
                alreadyVisited.Add(this);
                return findFunction(functionName, alreadyVisited);
            }

            protected abstract FuncitonFunction findFunction(string functionName, HashSet<Node> alreadyVisited);

            public virtual void AnalysisPass1(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes)
            {
                if (multiUseNodes.Contains(this))
                    return;
                if (singleUseNodes.Contains(this))
                {
                    singleUseNodes.Remove(this);
                    multiUseNodes.Add(this);
                    return;
                }
                singleUseNodes.Add(this);
                analysisPass1(singleUseNodes, multiUseNodes);
            }

            protected abstract void analysisPass1(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes);

            public string GetExpression(Node[] letNodes, bool alwaysExpand, bool requireParentheses)
            {
                int pos;
                if (letNodes != null && !alwaysExpand && (pos = Array.IndexOf(letNodes, this)) != -1)
                    return ((char) ('a' + pos)).ToString();
                return getExpression(letNodes, requireParentheses);
            }

            protected abstract string getExpression(Node[] letNodes, bool requireParentheses);
        }

        public sealed class CallNode
        {
            public FuncitonFunction Function;
            public Node[] Inputs;
            private Node[] _clonedFunctionOutputs;
            public Node[] ClonedFunctionOutputs { get { return _clonedFunctionOutputs ?? (_clonedFunctionOutputs = Function.CloneOutputNodes(Inputs)); } }

            public CallNode Clone(Dictionary<object, object> cloned, Node[] functionInputs)
            {
                object callNode;
                if (cloned.TryGetValue(this, out callNode))
                    return (CallNode) callNode;
                var newNode = new CallNode { Function = Function };
                cloned[this] = newNode;
                newNode.Inputs = Inputs.Select(inp => inp == null ? null : inp.Clone(cloned, functionInputs)).ToArray();
                return newNode;
            }
        }

        public sealed class CallOutputNode : Node
        {
            public CallNode CallNode;
            public int OutputPosition;
            public CallOutputNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            protected override Node cloneImpl(Dictionary<object, object> cloned, Node[] functionInputs)
            {
                var newNode = new CallOutputNode(_thisFunction) { OutputPosition = OutputPosition };
                cloned[this] = newNode;
                newNode.CallNode = CallNode.Clone(cloned, functionInputs);
                return newNode;
            }

            private int _state = 0;
            protected override Node nextToEvaluate()
            {
                switch (_state)
                {
                    case 0:
                        _state = 1;
                        return CallNode.ClonedFunctionOutputs[OutputPosition];
                    case 1:
                        _result = _previousSubresult;
                        _state = 2;
                        return null;
                    default: // = 2
                        return null;
                }
            }

            protected override FuncitonFunction findFunction(string functionName, HashSet<Node> alreadyVisited)
            {
                if (functionName == CallNode.Function.Name)
                    return CallNode.Function;
                return CallNode.Inputs.Where(inp => inp != null).Select(inp => inp.FindFunction(functionName, alreadyVisited)).FirstOrDefault(fnc => fnc != null)
                    ?? CallNode.Function._outputNodes.Select(on => on == null ? null : on.FindFunction(functionName, alreadyVisited)).FirstOrDefault(fnc => fnc != null);
            }

            public override void AnalysisPass1(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes)
            {
                var inpNode = CallNode.Function._outputNodes[OutputPosition] as InputNode;
                if (inpNode != null)
                    CallNode.Inputs[inpNode.InputPosition].AnalysisPass1(singleUseNodes, multiUseNodes);
                else
                    base.AnalysisPass1(singleUseNodes, multiUseNodes);
            }

            protected override void analysisPass1(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes)
            {
                foreach (var inp in CallNode.Inputs.Where(i => i != null))
                    inp.AnalysisPass1(singleUseNodes, multiUseNodes);
            }

            protected override string getExpression(Node[] letNodes, bool requireParentheses)
            {
                var open = requireParentheses ? "(" : "";
                var close = requireParentheses ? ")" : "";

                // Detect single-parameter, single-output functions (e.g. “ℓ”)
                var inputIndexes = CallNode.Inputs.Select((node, i) => node == null ? -1 : i).Where(i => i != -1).ToArray();
                var outputIndexes = CallNode.Function._outputNodes.Select((node, i) => node == null ? -1 : i).Where(i => i != -1).ToArray();
                var config = string.Join("", outputIndexes) + string.Join("", inputIndexes);
                if (inputIndexes.Length == 1 && outputIndexes.Length == 1)
                    return CallNode.Function.Name + "(" + CallNode.Inputs.Select((inp, ind) => inp == null ? null : inp.GetExpression(letNodes, false, false)).First(str => str != null) + ")";

                // Detect two-opposite-parameter, single-perpendicular-output functions (normally binary operators, e.g. “<”)
                if (inputIndexes.Length == 2 && (config == "013" || config == "302"))
                    return open + CallNode.Inputs[inputIndexes[1]].GetExpression(letNodes, false, true) + " " + CallNode.Function.Name + " " + CallNode.Inputs[inputIndexes[0]].GetExpression(letNodes, false, true) + close;
                else if (inputIndexes.Length == 2 && (config == "102" || config == "213"))
                    return open + CallNode.Inputs[inputIndexes[0]].GetExpression(letNodes, false, true) + " " + CallNode.Function.Name + " " + CallNode.Inputs[inputIndexes[1]].GetExpression(letNodes, false, true) + close;

                // Fall back to verbose notation
                return CallNode.Function.Name + "(" +
                    string.Join(", ", CallNode.Inputs.Select((inp, ind) => inp == null ? null : (inputIndexes.Length > 1 ? "↑→↓←"[ind] + ": " : "") + inp.GetExpression(letNodes, false, false)).Where(str => str != null)) +
                    ")" + (outputIndexes.Length > 1 ? "[" + "↓←↑→"[OutputPosition] + "]" : "");
            }
        }

        public sealed class NandNode : Node
        {
            public Node Left, Right;
            public NandNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            protected override Node cloneImpl(Dictionary<object, object> cloned, Node[] functionInputs)
            {
                var newNode = new NandNode(_thisFunction);
                cloned[this] = newNode;
                newNode.Left = Left.Clone(cloned, functionInputs);
                newNode.Right = Right.Clone(cloned, functionInputs);
                return newNode;
            }

            private int _state = 0;
            private BigInteger _leftEval;
            protected override Node nextToEvaluate()
            {
                switch (_state)
                {
                    case 0:
                        _state = 1;
                        return Left;
                    case 1:
                        if (_previousSubresult.IsZero)
                        {
                            _result = BigInteger.MinusOne;
                            _state = 3;
                            return null;
                        }
                        _leftEval = _previousSubresult;
                        _state = 2;
                        return Right;
                    case 2:
                        _result = ~(_leftEval & _previousSubresult);
                        _state = 3;
                        return null;
                    default: // = 3
                        return null;
                }
            }
            protected override FuncitonFunction findFunction(string functionName, HashSet<Node> alreadyVisited) { return Left.FindFunction(functionName, alreadyVisited) ?? Right.FindFunction(functionName, alreadyVisited); }
            protected override void analysisPass1(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes) { Left.AnalysisPass1(singleUseNodes, multiUseNodes); if (Right != Left) Right.AnalysisPass1(singleUseNodes, multiUseNodes); }
            protected override string getExpression(Node[] letNodes, bool requireParentheses)
            {
                var open = requireParentheses ? "(" : "";
                var close = requireParentheses ? ")" : "";

                // detect “or” (¬a @ ¬b = a | b)
                var leftNand = Left as NandNode;
                var rightNand = Right as NandNode;
                if (leftNand != null && leftNand.Left == leftNand.Right && rightNand != null && rightNand.Left == rightNand.Right)
                    return open + leftNand.Left.GetExpression(letNodes, false, true) + " | " + rightNand.Left.GetExpression(letNodes, false, true) + close;

                // detect “and” (¬(a @ b) = a & b)
                if (Left == Right && leftNand != null)
                    return open + leftNand.Left.GetExpression(letNodes, false, true) + " & " + leftNand.Right.GetExpression(letNodes, false, true) + close;

                // detect “not” (a @ a = ¬a)
                if (Left == Right)
                    return "¬" + Left.GetExpression(letNodes, false, true);

                return open + Left.GetExpression(letNodes, false, true) + " @ " + Right.GetExpression(letNodes, false, true) + close;
            }
        }

        public abstract class CrossWireNode : Node
        {
            public Node Left, Right;
            public CrossWireNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            protected override Node cloneImpl(Dictionary<object, object> cloned, Node[] functionInputs)
            {
                var newNode = createNew();
                cloned[this] = newNode;
                newNode.Left = Left.Clone(cloned, functionInputs);
                newNode.Right = Right.Clone(cloned, functionInputs);
                return newNode;
            }
            protected abstract CrossWireNode createNew();
            protected override FuncitonFunction findFunction(string functionName, HashSet<Node> alreadyVisited) { return Left.FindFunction(functionName, alreadyVisited) ?? Right.FindFunction(functionName, alreadyVisited); }
            protected override void analysisPass1(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes) { Left.AnalysisPass1(singleUseNodes, multiUseNodes); Right.AnalysisPass1(singleUseNodes, multiUseNodes); }
            protected override string getExpression(Node[] letNodes, bool requireParentheses)
            {
                var open = requireParentheses ? "(" : "";
                var close = requireParentheses ? ")" : "";
                return open + Left.GetExpression(letNodes, false, true) + _operator + Right.GetExpression(letNodes, false, true) + close;
            }
            protected abstract string _operator { get; }
        }

        public sealed class LessThanNode : CrossWireNode
        {
            public LessThanNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            protected override CrossWireNode createNew() { return new LessThanNode(_thisFunction); }
            private int _state = 0;
            private BigInteger _leftEval;
            protected override Node nextToEvaluate()
            {
                switch (_state)
                {
                    case 0:
                        _state = 1;
                        return Left;
                    case 1:
                        _leftEval = _previousSubresult;
                        _state = 2;
                        return Right;
                    case 2:
                        _result = _leftEval < _previousSubresult ? BigInteger.MinusOne : BigInteger.Zero;
                        _state = 3;
                        return null;
                    default: // = 3
                        return null;
                }
            }
            protected override string _operator { get { return " < "; } }
        }

        public sealed class ShiftLeftNode : CrossWireNode
        {
            public ShiftLeftNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            protected override CrossWireNode createNew() { return new ShiftLeftNode(_thisFunction); }
            private int _state = 0;
            private BigInteger _leftEval;
            protected override Node nextToEvaluate()
            {
                switch (_state)
                {
                    case 0:
                        _state = 1;
                        return Left;
                    case 1:
                        _leftEval = _previousSubresult;
                        _state = 2;
                        return Right;
                    case 2:
                        _result = _previousSubresult.IsZero ? _leftEval : _previousSubresult > 0 ? _leftEval << (int) _previousSubresult : _leftEval >> (int) -_previousSubresult;
                        _state = 3;
                        return null;
                    default: // = 3
                        return null;
                }
            }
            protected override string _operator { get { return " SHL "; } }
        }

        public sealed class InputNode : Node
        {
            public int InputPosition;
            public InputNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            private Node[] _functionInputs;
            protected override Node cloneImpl(Dictionary<object, object> cloned, Node[] functionInputs)
            {
                var newNode = new InputNode(_thisFunction) { InputPosition = InputPosition, _functionInputs = functionInputs };
                cloned[this] = newNode;
                return newNode;
            }

            private int _state = 0;
            protected override Node nextToEvaluate()
            {
                switch (_state)
                {
                    case 0:
                        _state = 1;
                        return _functionInputs[InputPosition];
                    case 1:
                        _result = _previousSubresult;
                        _state = 2;
                        return null;
                    default: // = 2
                        return null;
                }
            }
            protected override FuncitonFunction findFunction(string functionName, HashSet<Node> alreadyVisited) { return null; }
            protected override void analysisPass1(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes) { }
            protected override string getExpression(Node[] letNodes, bool requireParentheses) { return "↑→↓←".Substring(InputPosition, 1); }
            public override void AnalysisPass1(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes) { singleUseNodes.Add(this); }
        }

        public sealed class LiteralNode : Node
        {
            public LiteralNode(FuncitonFunction thisFunction, BigInteger literal) : base(thisFunction) { _result = literal; }
            protected override Node cloneImpl(Dictionary<object, object> cloned, Node[] functionInputs)
            {
                var newNode = new LiteralNode(_thisFunction, _result);
                cloned[this] = newNode;
                return newNode;
            }
            protected override Node nextToEvaluate() { return null; }
            protected override FuncitonFunction findFunction(string functionName, HashSet<Node> alreadyVisited) { return null; }
            protected override void analysisPass1(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes) { }
            protected override string getExpression(Node[] letNodes, bool requireParentheses) { return _result.ToString().Replace('-', '−'); }
            public override void AnalysisPass1(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes) { singleUseNodes.Add(this); }
        }

        public sealed class StdInNode : Node
        {
            private static BigInteger? _stdin;
            public StdInNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            protected override Node cloneImpl(Dictionary<object, object> cloned, Node[] functionInputs)
            {
                var newNode = new StdInNode(_thisFunction);
                cloned[this] = newNode;
                if (_stdin != null)
                    newNode._result = _stdin.Value;
                return newNode;
            }

            private bool _evaluated = false;
            protected override Node nextToEvaluate()
            {
                if (!_evaluated)
                {
                    if (_stdin == null)
                        _stdin = FuncitonLanguage.StringToInteger(Console.In.ReadToEnd());
                    _result = _stdin.Value;
                    _evaluated = true;
                }
                return null;
            }
            protected override FuncitonFunction findFunction(string functionName, HashSet<Node> alreadyVisited) { return null; }
            protected override void analysisPass1(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes) { }
            protected override string getExpression(Node[] letNodes, bool requireParentheses) { return "♦"; }
            public override void AnalysisPass1(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes) { singleUseNodes.Add(this); }
        }

        public string Name;

        protected Node[] _outputNodes;
        public FuncitonFunction(Node[] outputNodes) { _outputNodes = outputNodes; }

        public Node[] CloneOutputNodes(Node[] functionInputs)
        {
            var cloned = new Dictionary<object, object>();
            var outputNodes = new Node[_outputNodes.Length];
            for (int i = 0; i < _outputNodes.Length; i++)
                outputNodes[i] = _outputNodes[i] == null ? null : _outputNodes[i].Clone(cloned, functionInputs);
            return outputNodes;
        }

        public string Analyse()
        {
            // Pass one: determine which nodes are single-use and which are multi-use
            var singleUseNodes = new HashSet<Node>();
            var multiUseNodes = new HashSet<Node>();
            foreach (var output in _outputNodes.Where(on => on != null))
                output.AnalysisPass1(singleUseNodes, multiUseNodes);
            var numOutputs = _outputNodes.Count(on => on != null);

            // Pass two: generate expressions
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("definition of {0}({1}):", Name, string.Join(", ", singleUseNodes.Concat(multiUseNodes).OfType<InputNode>().Distinct().OrderBy(i => i.InputPosition).Select(i => "↑→↓←"[i.InputPosition]))));
            var letNodes = multiUseNodes.ToArray();
            for (int i = 0; i < letNodes.Length; i++)
                sb.AppendLine(string.Format("    let {0} := {1}", (char) ('a' + i), letNodes[i].GetExpression(letNodes, true, false)));
            for (int i = 0; i < _outputNodes.Length; i++)
            {
                if (_outputNodes[i] == null)
                    continue;
                sb.Append("    output ");
                sb.Append("↓←↑→"[i] + " := " + _outputNodes[i].GetExpression(letNodes, false, false));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public int? GetInputForOutputIfNop(int outputPosition)
        {
            var input = _outputNodes[outputPosition] as InputNode;
            return input == null ? (int?) null : input.InputPosition;
        }
    }

    sealed class FuncitonProgram : FuncitonFunction
    {
        public FuncitonProgram(Node[] outputNodes) : base(outputNodes) { }

        public string Run(List<string> traceFunctions)
        {
            // A larger initial capacity than this does not improve performance
            var evaluationStack = new Stack<Node>(1024);

            // Should have only one output
            var currentNode = _outputNodes.Single(o => o != null);

            while (true)
            {
                var next = currentNode.NextToEvaluate(traceFunctions);

                // small performance optimisation (saves a push and a pop for every literal)
                while (next is LiteralNode)
                {
                    currentNode.PreviousSubresult = next.Result;
                    next = currentNode.NextToEvaluate(traceFunctions);
                }

                if (next != null)
                {
                    evaluationStack.Push(currentNode);
                    currentNode = next;
                }
                else
                {
                    if (evaluationStack.Count == 0)
                        return FuncitonLanguage.IntegerToString(currentNode.Result);
                    var lastResult = currentNode.Result;
                    currentNode = evaluationStack.Pop();
                    currentNode.PreviousSubresult = lastResult;
                }
            }
        }
    }
}
