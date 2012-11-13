﻿using System;
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
            public Node NextToEvaluate(List<string> traceFunctions)
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
            public CallNode Cloned;
            public int ClonedId;

            public CallNode Clone(int clonedId, Node[] functionInputs)
            {
                if (ClonedId == clonedId)
                    return Cloned;
                ClonedId = clonedId;
                Cloned = new CallNode { Function = Function };
                Cloned.Inputs = new Node[Inputs.Length];
                for (int i = 0; i < Inputs.Length; i++)
                    if (Inputs[i] != null)
                        Cloned.Inputs[i] = Inputs[i].Clone(clonedId, functionInputs);
                return Cloned;
            }
        }

        public sealed class CallOutputNode : Node
        {
            public CallNode CallNode;
            public int OutputPosition;
            public CallOutputNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            public override Node Clone(int clonedId, Node[] functionInputs)
            {
                if (ClonedId == clonedId)
                    return Cloned;
                ClonedId = clonedId;
                var newNode = new CallOutputNode(_thisFunction) { OutputPosition = OutputPosition };
                Cloned = newNode;
                newNode.CallNode = CallNode.Clone(clonedId, functionInputs);
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

            protected override void releaseMemory()
            {
                if (_state == 1)
                    CallNode = null;
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
                if (inputIndexes.Length == 1 && outputIndexes.Length == 1)
                    return CallNode.Function.Name + "(" + CallNode.Inputs.Select((inp, ind) => inp == null ? null : inp.GetExpression(letNodes, false, false)).First(str => str != null) + ")";

                // Detect two-opposite-parameter, single-perpendicular-output functions (normally binary operators, e.g. “<”)
                var config = string.Join("", outputIndexes) + string.Join("", inputIndexes);
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

            public override Node Clone(int clonedId, Node[] functionInputs)
            {
                if (ClonedId == clonedId)
                    return Cloned;
                ClonedId = clonedId;
                var newNode = new NandNode(_thisFunction);
                Cloned = newNode;
                newNode.Left = Left.Clone(clonedId, functionInputs);
                newNode.Right = Right.Clone(clonedId, functionInputs);
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

            protected override FuncitonFunction findFunction(string functionName, HashSet<Node> alreadyVisited)
            {
                return Left.FindFunction(functionName, alreadyVisited) ?? Right.FindFunction(functionName, alreadyVisited);
            }

            protected override void analysisPass1(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes)
            {
                Left.AnalysisPass1(singleUseNodes, multiUseNodes);
                // If the two nodes are the same, this NAND is used to express a NOT.
                // getExpression() will recognize that and just output a unary NOT (¬).
                // In such a case, we should not allocate a variable for it if that is its only use.
                if (Right != Left)
                    Right.AnalysisPass1(singleUseNodes, multiUseNodes);
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
            public Node Left, Right;
            public CrossWireNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            public override Node Clone(int clonedId, Node[] functionInputs)
            {
                if (ClonedId == clonedId)
                    return Cloned;
                ClonedId = clonedId;
                var newNode = createNew();
                Cloned = newNode;
                newNode.Left = Left.Clone(clonedId, functionInputs);
                newNode.Right = Right.Clone(clonedId, functionInputs);
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
            protected override void releaseMemory()
            {
                if (_state == 1)
                    Left = null;
                else if (_state == 2)
                    Right = null;
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
            protected override void releaseMemory()
            {
                if (_state == 1)
                    Left = null;
                else if (_state == 2)
                    Right = null;
            }
            protected override string _operator { get { return " SHL "; } }
        }

        public sealed class InputNode : Node
        {
            public int InputPosition;
            public InputNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            private Node[] _functionInputs;
            public override Node Clone(int clonedId, Node[] functionInputs)
            {
                if (ClonedId == clonedId)
                    return Cloned;
                ClonedId = clonedId;
                var newNode = new InputNode(_thisFunction) { InputPosition = InputPosition, _functionInputs = functionInputs };
                Cloned = newNode;
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
            protected override void releaseMemory()
            {
                if (_state == 1)
                    _functionInputs = null;
            }
            protected override FuncitonFunction findFunction(string functionName, HashSet<Node> alreadyVisited) { return null; }
            protected override void analysisPass1(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes) { }
            protected override string getExpression(Node[] letNodes, bool requireParentheses) { return "↑→↓←".Substring(InputPosition, 1); }
            public override void AnalysisPass1(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes) { singleUseNodes.Add(this); }
        }

        public sealed class LiteralNode : Node
        {
            public LiteralNode(FuncitonFunction thisFunction, BigInteger literal) : base(thisFunction) { _result = literal; }
            public override Node Clone(int clonedId, Node[] functionInputs) { return this; }
            protected override Node nextToEvaluate() { return null; }
            protected override void releaseMemory() { }
            protected override FuncitonFunction findFunction(string functionName, HashSet<Node> alreadyVisited) { return null; }
            protected override void analysisPass1(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes) { }
            protected override string getExpression(Node[] letNodes, bool requireParentheses) { return _result.ToString().Replace('-', '−'); }
            public override void AnalysisPass1(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes) { singleUseNodes.Add(this); }
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
            protected override FuncitonFunction findFunction(string functionName, HashSet<Node> alreadyVisited) { return null; }
            protected override void analysisPass1(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes) { }
            protected override string getExpression(Node[] letNodes, bool requireParentheses) { return "♦"; }
            public override void AnalysisPass1(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes) { singleUseNodes.Add(this); }
        }

        public string Name;

        protected Node[] _outputNodes;
        public FuncitonFunction(Node[] outputNodes) { _outputNodes = outputNodes; }

        private static int _cloneCounter = 0;

        public Node[] CloneOutputNodes(Node[] functionInputs)
        {
            _cloneCounter++;
            var outputNodes = new Node[_outputNodes.Length];
            for (int i = 0; i < _outputNodes.Length; i++)
                if (_outputNodes[i] != null)
                    outputNodes[i] = _outputNodes[i].Clone(_cloneCounter, functionInputs);
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
