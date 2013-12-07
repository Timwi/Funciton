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
            public Node Cloned;
            public int ClonedId;

            public abstract Node Clone(int clonedId, Node[] functionInputs);

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
            public Node NextToEvaluate(IEnumerable<string> traceFunctions)
            {
                var res = nextToEvaluate();
                if (traceFunctions != null && traceFunctions.Contains(_thisFunction.Name))
                    trace(res);
                else
                    releaseMemory();
                return res;
            }

            private void trace(Node res)
            {
                // Only output a trace if this node is fully evaluated
                if (res != null)
                    return;

                // Don’t bother showing extra trace lines for literals
                if (this is LiteralNode)
                    return;

                // Only output a trace if we haven’t already done so for this node
                if (!_alreadyTraced.Add(this))
                    return;

                // See if the result happens to be a valid string
                string str;
                try { str = string.Format(@"""{0}""", Helpers.CLiteralEscape(FuncitonLanguage.IntegerToString(_result))); }
                catch { str = null; }

                // See if the result happens to be a valid list
                string list = null;
                try
                {
                    if (_result < 0)
                        goto notAValidList;
                    var intList = new List<BigInteger>();
                    var result = _result;
                    var mask = (BigInteger.One << 22) - 1;
                    while (result > 0)
                    {
                        var curItem = BigInteger.Zero;
                        while (true)
                        {
                            bool endHere = (result & 1) == 1;
                            curItem = (curItem << 21) | ((result & mask) >> 1);
                            result >>= 22;
                            if (endHere)
                                break;
                            if (result == 0)
                                goto notAValidList;
                        }
                        if ((result & 1) == 1)
                            curItem = ~curItem;
                        result >>= 1;
                        intList.Add(curItem);
                    }
                    list = string.Format(@"[{0}]", string.Join(", ", intList));
                    notAValidList: ;
                }
                catch { }

                ConsoleWriteLineColored(
                    ConsoleColor.White, _thisFunction.Name, ": ",
                    ConsoleColor.Gray, getExpression(null, false),
                    ConsoleColor.White, " = ",
                    ConsoleColor.Green, _result.ToString(),
                    str == null ? null : new object[] { Environment.NewLine, ConsoleColor.DarkCyan, "        ", str },
                    list == null ? null : new object[] { Environment.NewLine, ConsoleColor.DarkMagenta, "        ", list });
            }

            private void ConsoleWriteLineColored(params object[] objs)
            {
                ConsoleWriteColored(objs);
                Console.WriteLine();
                Console.ResetColor();
            }

            private void ConsoleWriteColored(params object[] objs)
            {
                foreach (var item in objs)
                    if (item is ConsoleColor)
                        Console.ForegroundColor = (ConsoleColor) item;
                    else if (item is object[])
                        ConsoleWriteColored((object[]) item);
                    else if (item != null)
                        Console.Write(item);
            }

            // This is a static field rather than a boolean instance field because an instance field would make
            // every Node instance larger and thus use significantly more memory even when not tracing.
            private static HashSet<Node> _alreadyTraced = new HashSet<Node>();

            protected abstract Node nextToEvaluate();
            protected abstract void releaseMemory();

            protected BigInteger _result;
            protected BigInteger _previousSubresult;

            /// <summary>See the remarks on <see cref="NextToEvaluate"/> for details. Until <see cref="NextToEvaluate"/> has returned null, this value is meaningless. Afterwards, it contains the result of evaluating this code.</summary>
            public BigInteger Result { get { return _result; } }

            /// <summary>See the remarks on <see cref="NextToEvaluate"/> for details. Write the result of a previous evaluation here. The previous subresult must be written before the next call to <see cref="NextToEvaluate"/> is made.</summary>
            public BigInteger PreviousSubresult { set { _previousSubresult = value; } }

            public virtual void FindNodes(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes, HashSet<Node> nodesUsedAsFunctionInputs, HashSet<Node> allNodes)
            {
                allNodes.Add(this);

                if (multiUseNodes.Contains(this))
                    return;
                if (singleUseNodes.Contains(this))
                {
                    singleUseNodes.Remove(this);
                    multiUseNodes.Add(this);
                    return;
                }
                singleUseNodes.Add(this);
                findChildNodes(singleUseNodes, multiUseNodes, nodesUsedAsFunctionInputs, allNodes);
            }

            protected abstract void findChildNodes(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes, HashSet<Node> nodesUsedAsFunctionInputs, HashSet<Node> allNodes);

            public string GetExpression(Node[] letNodes, bool alwaysExpand, bool requireParentheses)
            {
                int pos;
                if (letNodes != null && !alwaysExpand && (pos = Array.IndexOf(letNodes, this)) != -1)
                    return ((char) ('a' + pos)).ToString();
                return getExpression(letNodes, requireParentheses);
            }

            protected abstract string getExpression(Node[] letNodes, bool requireParentheses);
        }

        public sealed class Call
        {
            public FuncitonFunction Function;
            public Node[] Inputs;
            private Node[] _clonedFunctionOutputs;
            public Node[] ClonedFunctionOutputs { get { return _clonedFunctionOutputs ?? (_clonedFunctionOutputs = Function.CloneOutputNodes(Inputs)); } }
            public Call Cloned;
            public int ClonedId;

            public Call Clone(int clonedId, Node[] functionInputs)
            {
                if (ClonedId != clonedId)
                {
                    ClonedId = clonedId;
                    Cloned = new Call
                    {
                        Function = Function,
                        Inputs = Inputs.Select(inp => inp == null ? null : inp.Clone(clonedId, functionInputs)).ToArray()
                    };
                }
                return Cloned;
            }
        }

        public sealed class CallOutputNode : Node
        {
            public int OutputPosition { get; private set; }
            public Call Call { get; private set; }

            public CallOutputNode(FuncitonFunction thisFunction, int outputPosition, Call call)
                : base(thisFunction)
            {
                OutputPosition = outputPosition;
                Call = call;
            }

            public override Node Clone(int clonedId, Node[] functionInputs)
            {
                if (ClonedId != clonedId)
                {
                    ClonedId = clonedId;
                    Cloned = new CallOutputNode(_thisFunction, OutputPosition, Call.Clone(clonedId, functionInputs));
                }
                return Cloned;
            }

            private int _state = 0;
            protected override Node nextToEvaluate()
            {
                switch (_state)
                {
                    case 0:
                        _state = 1;
                        return Call.ClonedFunctionOutputs[OutputPosition];
                    case 1:
                        _result = _previousSubresult;
                        _state = 2;
                        return null;
                    default: // = 2
                        return null;
                }
            }

            protected override void releaseMemory()
            {
                if (_state == 1)
                    Call = null;
            }

            protected override void findChildNodes(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes, HashSet<Node> nodesUsedAsFunctionInputs, HashSet<Node> allNodes)
            {
                foreach (var inp in Call.Inputs.Where(i => i != null))
                {
                    nodesUsedAsFunctionInputs.Add(inp);
                    inp.FindNodes(singleUseNodes, multiUseNodes, nodesUsedAsFunctionInputs, allNodes);
                }
            }

            protected override string getExpression(Node[] letNodes, bool requireParentheses)
            {
                var open = requireParentheses ? "(" : "";
                var close = requireParentheses ? ")" : "";

                // Detect single-parameter, single-output functions (e.g. “ℓ”)
                var inputIndexes = Call.Inputs.Select((node, i) => node == null ? -1 : i).Where(i => i != -1).ToArray();
                var outputIndexes = Call.Function.OutputNodes.Select((node, i) => node == null ? -1 : i).Where(i => i != -1).ToArray();
                if (inputIndexes.Length == 1 && outputIndexes.Length == 1)
                    return Call.Function.Name + "(" + Call.Inputs.Select((inp, ind) => inp == null ? null : inp.GetExpression(letNodes, false, false)).First(str => str != null) + ")";

                // Detect two-opposite-parameter, single-perpendicular-output functions (normally binary operators, e.g. “<”)
                var config = string.Join("", outputIndexes) + string.Join("", inputIndexes);
                if (inputIndexes.Length == 2 && (config == "013" || config == "302"))
                    return open + Call.Inputs[inputIndexes[1]].GetExpression(letNodes, false, true) + " " + Call.Function.Name + " " + Call.Inputs[inputIndexes[0]].GetExpression(letNodes, false, true) + close;
                else if (inputIndexes.Length == 2 && (config == "102" || config == "213"))
                    return open + Call.Inputs[inputIndexes[0]].GetExpression(letNodes, false, true) + " " + Call.Function.Name + " " + Call.Inputs[inputIndexes[1]].GetExpression(letNodes, false, true) + close;

                // Fall back to verbose notation
                return Call.Function.Name + "(" +
                    string.Join(", ", Call.Inputs.Select((inp, ind) => inp == null ? null : (inputIndexes.Length > 1 ? "↑→↓←"[ind] + ": " : "") + inp.GetExpression(letNodes, false, false)).Where(str => str != null)) +
                    ")" + (outputIndexes.Length > 1 ? "[" + "↓←↑→"[OutputPosition] + "]" : "");
            }
        }

        public sealed class NandNode : Node
        {
            public Node Left { get; private set; }
            public Node Right { get; private set; }

            public NandNode(FuncitonFunction thisFunction, Node left, Node right)
                : base(thisFunction)
            {
                Left = left;
                Right = right;
            }

            public override Node Clone(int clonedId, Node[] functionInputs)
            {
                if (ClonedId != clonedId)
                {
                    ClonedId = clonedId;
                    Cloned = new NandNode(_thisFunction, Left.Clone(clonedId, functionInputs), Right.Clone(clonedId, functionInputs));
                }
                return Cloned;
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
                            // short-circuit evaluation
                            _result = BigInteger.MinusOne;
                            _state = 3;
                            return null;
                        }
                        else
                        {
                            _leftEval = _previousSubresult;
                            _state = 2;
                            return Right;
                        }
                    case 2:
                        _result = ~(_leftEval & _previousSubresult);
                        _state = 3;
                        return null;
                    default: // = 3
                        return null;
                }
            }

            protected override void releaseMemory()
            {
                if (_state == 1)
                    Left = null;
                else if (_state > 1)
                    Right = null;
            }

            protected override void findChildNodes(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes, HashSet<Node> nodesUsedAsFunctionInputs, HashSet<Node> allNodes)
            {
                Left.FindNodes(singleUseNodes, multiUseNodes, nodesUsedAsFunctionInputs, allNodes);
                // If the two nodes are the same, this NAND is used to express a NOT.
                // getExpression() will recognize that and just output a unary NOT (¬).
                // In such a case, we should not allocate a variable for it if that is its only use.
                if (Right != Left)
                    Right.FindNodes(singleUseNodes, multiUseNodes, nodesUsedAsFunctionInputs, allNodes);
            }

            protected override string getExpression(Node[] letNodes, bool requireParentheses)
            {
                var open = requireParentheses ? "(" : "";
                var close = requireParentheses ? ")" : "";

                // detect “or” (¬a @ ¬b = a | b)
                var leftNand = Left as NandNode;
                var rightNand = Right as NandNode;
                if (leftNand != null && leftNand.Left == leftNand.Right && rightNand != null && rightNand.Left == rightNand.Right && (letNodes == null || !letNodes.Contains(Left)) && (letNodes == null || !letNodes.Contains(Right)))
                    return open + leftNand.Left.GetExpression(letNodes, false, true) + " | " + rightNand.Left.GetExpression(letNodes, false, true) + close;

                // detect “and” (¬(a @ b) = a & b)
                if (Left == Right && leftNand != null && (letNodes == null || !letNodes.Contains(Left)))
                    return open + leftNand.Left.GetExpression(letNodes, false, true) + " & " + leftNand.Right.GetExpression(letNodes, false, true) + close;

                // detect “not” (a @ a = ¬a)
                if (Left == Right)
                    return "¬" + Left.GetExpression(letNodes, false, true);

                return open + Left.GetExpression(letNodes, false, true) + " @ " + Right.GetExpression(letNodes, false, true) + close;
            }
        }

        public abstract class CrossWireNode : Node
        {
            public Node Left { get; private set; }
            public Node Right { get; private set; }

            public CrossWireNode(FuncitonFunction thisFunction, Node left, Node right)
                : base(thisFunction)
            {
                Left = left;
                Right = right;
            }

            public override Node Clone(int clonedId, Node[] functionInputs)
            {
                if (ClonedId != clonedId)
                {
                    ClonedId = clonedId;
                    Cloned = createNew(
                        Left.Clone(clonedId, functionInputs),
                        Right.Clone(clonedId, functionInputs));
                }
                return Cloned;
            }

            protected abstract CrossWireNode createNew(Node left, Node right);

            protected override void findChildNodes(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes, HashSet<Node> nodesUsedAsFunctionInputs, HashSet<Node> allNodes)
            {
                Left.FindNodes(singleUseNodes, multiUseNodes, nodesUsedAsFunctionInputs, allNodes);
                Right.FindNodes(singleUseNodes, multiUseNodes, nodesUsedAsFunctionInputs, allNodes);
            }

            protected override string getExpression(Node[] letNodes, bool requireParentheses)
            {
                var open = requireParentheses ? "(" : "";
                var close = requireParentheses ? ")" : "";
                return open + Left.GetExpression(letNodes, false, true) + _operator + Right.GetExpression(letNodes, false, true) + close;
            }

            protected abstract string _operator { get; }

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
                        _result = getResult(_leftEval, _previousSubresult);
                        _state = 3;
                        return null;
                    default: // = 3
                        return null;
                }
            }

            protected abstract BigInteger getResult(BigInteger left, BigInteger right);

            protected override void releaseMemory()
            {
                if (_state == 1)
                    Left = null;
                else if (_state == 2)
                    Right = null;
            }
        }

        public sealed class LessThanNode : CrossWireNode
        {
            public LessThanNode(FuncitonFunction thisFunction, Node left, Node right) : base(thisFunction, left, right) { }
            protected override CrossWireNode createNew(Node left, Node right) { return new LessThanNode(_thisFunction, left, right); }
            protected override string _operator { get { return " < "; } }
            protected override BigInteger getResult(BigInteger left, BigInteger right)
            {
                return left < right ? BigInteger.MinusOne : BigInteger.Zero;
            }
        }

        public sealed class ShiftLeftNode : CrossWireNode
        {
            public ShiftLeftNode(FuncitonFunction thisFunction, Node left, Node right) : base(thisFunction, left, right) { }
            protected override CrossWireNode createNew(Node left, Node right) { return new ShiftLeftNode(_thisFunction, left, right); }
            protected override string _operator { get { return " SHL "; } }
            protected override BigInteger getResult(BigInteger left, BigInteger right)
            {
                return right.IsZero ? left : right > 0 ? left << (int) right : left >> (int) -right;
            }
        }

        public sealed class InputNode : Node
        {
            public int InputPosition { get; private set; }
            public InputNode(FuncitonFunction thisFunction, int inputPosition) : base(thisFunction) { InputPosition = inputPosition; }
            private Node[] _functionInputs;
            public override Node Clone(int clonedId, Node[] functionInputs)
            {
                if (ClonedId != clonedId)
                {
                    ClonedId = clonedId;
                    Cloned = new InputNode(_thisFunction, InputPosition) { _functionInputs = functionInputs };
                }
                return Cloned;
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
            protected override void releaseMemory()
            {
                if (_state == 1)
                    _functionInputs = null;
            }
            protected override void findChildNodes(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes, HashSet<Node> nodesUsedAsFunctionInputs, HashSet<Node> allNodes) { }
            protected override string getExpression(Node[] letNodes, bool requireParentheses) { return "↑→↓←".Substring(InputPosition, 1); }
        }

        public sealed class LiteralNode : Node
        {
            public LiteralNode(FuncitonFunction thisFunction, BigInteger literal) : base(thisFunction) { _result = literal; }
            public override Node Clone(int clonedId, Node[] functionInputs) { return this; }
            protected override Node nextToEvaluate() { return null; }
            protected override void releaseMemory() { }
            protected override void findChildNodes(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes, HashSet<Node> nodesUsedAsFunctionInputs, HashSet<Node> allNodes) { }
            protected override string getExpression(Node[] letNodes, bool requireParentheses) { return _result.ToString().Replace('-', '−'); }
            public override void FindNodes(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes, HashSet<Node> nodesUsedAsFunctionInputs, HashSet<Node> allNodes)
            {
                allNodes.Add(this);
                singleUseNodes.Add(this);
            }
        }

        public sealed class StdInNode : Node
        {
            private static BigInteger? _stdin;
            public StdInNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            public override Node Clone(int clonedId, Node[] functionInputs) { return this; }

            private bool _evaluated = false;
            protected override Node nextToEvaluate()
            {
                if (!_evaluated)
                {
                    if (_stdin == null)
                        _stdin = FuncitonLanguage.PretendStdin ?? FuncitonLanguage.StringToInteger(Console.In.ReadToEnd());
                    _result = _stdin.Value;
                    _evaluated = true;
                }
                return null;
            }
            protected override void releaseMemory() { }
            protected override void findChildNodes(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes, HashSet<Node> nodesUsedAsFunctionInputs, HashSet<Node> allNodes) { }
            protected override string getExpression(Node[] letNodes, bool requireParentheses) { return "♦"; }
            public override void FindNodes(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes, HashSet<Node> nodesUsedAsFunctionInputs, HashSet<Node> allNodes)
            {
                allNodes.Add(this);
                singleUseNodes.Add(this);
            }
        }

        public FuncitonFunction(Node[] outputNodes, string name)
        {
            OutputNodes = outputNodes;
            Name = name;
        }
        public Node[] OutputNodes { get; private set; }
        public string Name { get; private set; }

        private static int _cloneCounter = 0;

        public Node[] CloneOutputNodes(Node[] functionInputs)
        {
            _cloneCounter++;
            var outputNodes = new Node[OutputNodes.Length];
            for (int i = 0; i < OutputNodes.Length; i++)
                if (OutputNodes[i] != null)
                    outputNodes[i] = OutputNodes[i].Clone(_cloneCounter, functionInputs);
            return outputNodes;
        }

        public void Analyse(StringBuilder sb)
        {
            // Pass one: determine which nodes are single-use and which are multi-use
            var nodes = FindNodes();

            // Pass two: generate expressions
            sb.AppendLine(string.Format("definition of {0}:",
                Name == "" ? "main program" : string.Format("{0}({1})", Name, string.Join(", ", nodes.AllNodes.OfType<InputNode>().OrderBy(i => i.InputPosition).Select(i => "↑→↓←"[i.InputPosition])))));
            var letNodes = nodes.MultiUseNodes.Except(nodes.AllNodes.OfType<InputNode>()).ToArray();
            for (int i = 0; i < letNodes.Length; i++)
                sb.AppendLine(string.Format("    let {0} := {1}", (char) ('a' + i), letNodes[i].GetExpression(letNodes, true, false)));
            for (int i = 0; i < OutputNodes.Length; i++)
            {
                if (OutputNodes[i] == null)
                    continue;
                sb.Append("    output ");
                sb.Append("↓←↑→"[i] + " := " + OutputNodes[i].GetExpression(letNodes, false, false));
                sb.AppendLine();
            }
        }

        public FindNodesResult FindNodes()
        {
            var singleUseNodes = new HashSet<Node>();
            var multiUseNodes = new HashSet<Node>();
            var nodesUsedAsFunctionInputs = new HashSet<Node>();
            var allNodes = new HashSet<Node>();
            foreach (var output in OutputNodes.Where(on => on != null))
                output.FindNodes(singleUseNodes, multiUseNodes, nodesUsedAsFunctionInputs, allNodes);
            return new FindNodesResult
            {
                SingleUseNodes = singleUseNodes,
                MultiUseNodes = multiUseNodes,
                NodesUsedAsFunctionInputs = nodesUsedAsFunctionInputs,
                AllNodes = allNodes
            };
        }

        public int? GetInputForOutputIfNop(int outputPosition)
        {
            var input = OutputNodes[outputPosition] as InputNode;
            return input == null ? (int?) null : input.InputPosition;
        }
    }

    sealed class FuncitonProgram : FuncitonFunction
    {
        public FuncitonProgram(Node[] outputNodes) : base(outputNodes, "") { }

        public string Run(List<string> traceFunctions)
        {
            // A larger initial capacity than this does not improve performance
            var evaluationStack = new Stack<Node>(1024);

            // Should have only one output
            var currentNode = OutputNodes.Single(o => o != null);

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

    sealed class FindNodesResult
    {
        public HashSet<FuncitonFunction.Node> SingleUseNodes;
        public HashSet<FuncitonFunction.Node> MultiUseNodes;
        public HashSet<FuncitonFunction.Node> NodesUsedAsFunctionInputs;
        public HashSet<FuncitonFunction.Node> AllNodes;
    }
}
