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
            protected Node _cloned;
            protected int _clonedId;

            /// <summary>Clones a function for the purpose of calling it.</summary>
            public abstract Node CloneForFunctionCall(int clonedId, Node[] functionInputs);

            /// <summary>Clones those parts of a function that depend on the specified <paramref name="lambdaParameter"/>.</summary>
            public abstract Node CloneForLambdaInvoke(int clonedId, Node lambdaParameter, Node lambdaArgument);

            /// <summary>
            ///     This function is designed to evaluate an entire Funciton program without using .NET’s own call stack (so
            ///     that we are not limited to its size). See remarks for details.</summary>
            /// <param name="traceFunctions">
            ///     A list of function names for which to output debug trace information.</param>
            /// <returns>
            ///     A node to evaluate next, or null to indicate evaluation is complete. See remarks for details.</returns>
            /// <remarks>
            ///     <para>
            ///         The code contract is this:</para>
            ///     <list type="bullet">
            ///         <item><description>
            ///             The caller calls <see cref="NextToEvaluate"/>.</description></item>
            ///         <item><description>
            ///             If <see cref="NextToEvaluate"/> returns <c>null</c>, the node is fully evaluated and the result
            ///             can be read from <see cref="Result"/>.</description></item>
            ///         <item><description>
            ///             If <see cref="NextToEvaluate"/> returns a node, the caller is expected to fully evaluate that
            ///             node, read its result, store that in <see cref="PreviousSubresult"/> and then call <see
            ///             cref="NextToEvaluate"/> again.</description></item></list></remarks>
            public Node NextToEvaluate(IEnumerable<string> traceFunctions)
            {
                var res = nextToEvaluate();
                if (traceFunctions != null && traceFunctions.Contains(_thisFunction.Name))
                    trace(res);
                else
                    releaseMemory();
                return res;
            }

            public static Node NextToEvaluate(Node n) { return n.nextToEvaluate(); }

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

            /// <summary>
            ///     See the remarks on <see cref="NextToEvaluate"/> for details. Until <see cref="NextToEvaluate"/> has
            ///     returned null, this value is meaningless. Afterwards, it contains the result of evaluating this code.</summary>
            public BigInteger Result { get { return _result; } }

            /// <summary>
            ///     See the remarks on <see cref="NextToEvaluate"/> for details. Write the result of a previous evaluation
            ///     here. The previous subresult must be written before the next call to <see cref="NextToEvaluate"/> is made.</summary>
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
            public FuncitonFunction Function { get; private set; }
            public Node[] Inputs { get; private set; }
            private Call _cloned;
            private int _clonedId;

            private Node[] _clonedFunctionOutputs;
            public Node[] ClonedFunctionOutputs { get { return _clonedFunctionOutputs ?? (_clonedFunctionOutputs = Function.CloneOutputNodes(Inputs)); } }

            public Call(FuncitonFunction function, Node[] inputs)
            {
                Function = function;
                Inputs = inputs;
            }

            public Call CloneForFunctionCall(int clonedId, Node[] functionInputs)
            {
                if (_clonedId != clonedId)
                {
                    _clonedId = clonedId;
                    _cloned = new Call(Function, Inputs.Select(inp => inp == null ? null : inp.CloneForFunctionCall(clonedId, functionInputs)).ToArray());
                }
                return _cloned;
            }

            public Call CloneForLambdaInvoke(int clonedId, Node lambdaParameter, Node lambdaArgument)
            {
                if (_clonedId != clonedId)
                {
                    _clonedId = clonedId;
                    var clonedInputs = Inputs.Select(inp => inp == null ? null : inp.CloneForLambdaInvoke(clonedId, lambdaParameter, lambdaArgument)).ToArray();
                    _cloned = clonedInputs.SequenceEqual(Inputs) ? this : new Call(Function, clonedInputs);
                }
                return _cloned;
            }
        }

        public sealed class LambdaInvocation
        {
            public Node Argument { get; private set; }
            public Node LambdaGetter { get; private set; }
            public LambdaClosure Closure { get; set; }  // only set after LambdaGetter is evaluated

            private LambdaInvocation _cloned;
            private int _clonedId;

            private Tuple<Node, Node> _clonedReturnValues;
            public Tuple<Node, Node> ClonedReturnValues { get { return _clonedReturnValues ?? (_clonedReturnValues = Closure.CloneReturnValues(Argument)); } }

            public LambdaInvocation(Node argument, Node lambdaGetter)
            {
                Argument = argument;
                LambdaGetter = lambdaGetter;
            }

            public LambdaInvocation CloneForFunctionCall(int clonedId, Node[] functionInputs)
            {
                if (_clonedId != clonedId)
                {
                    _clonedId = clonedId;
                    _cloned = new LambdaInvocation(Argument.CloneForFunctionCall(clonedId, functionInputs), LambdaGetter.CloneForFunctionCall(clonedId, functionInputs));
                }
                return _cloned;
            }

            public LambdaInvocation CloneForLambdaInvoke(int clonedId, Node lambdaParameter, Node lambdaArgument)
            {
                if (_clonedId != clonedId)
                {
                    _clonedId = clonedId;
                    var clonedArgument = Argument.CloneForLambdaInvoke(clonedId, lambdaParameter, lambdaArgument);
                    var clonedGetter = LambdaGetter.CloneForLambdaInvoke(clonedId, lambdaParameter, lambdaArgument);
                    _cloned = clonedArgument == Argument && clonedGetter == LambdaGetter ? this :
                        new LambdaInvocation(clonedArgument, clonedGetter);
                }
                return _cloned;
            }
        }

        public sealed class LambdaClosure
        {
            public Node Parameter { get; private set; }
            private Node _returnValue1;
            private Node _returnValue2;

            public LambdaClosure(Node parameter, Node return1, Node return2)
            {
                Parameter = parameter;
                _returnValue1 = return1;
                _returnValue2 = return2;
            }

            public Tuple<Node, Node> CloneReturnValues(Node argument)
            {
                _cloneCounter++;
                return Tuple.Create(
                    _returnValue1.CloneForLambdaInvoke(_cloneCounter, Parameter, argument),
                    _returnValue2.CloneForLambdaInvoke(_cloneCounter, Parameter, argument));
            }
        }

        public sealed class CallOutputNode : Node
        {
            public int OutputPosition { get; private set; }
            public Call Call { get; private set; }

            public CallOutputNode(FuncitonFunction thisFunction, int outputPosition, Call call)
                : base(thisFunction)
            {
                if (call == null)
                    throw new ArgumentNullException("call");
                OutputPosition = outputPosition;
                Call = call;
            }

            public override Node CloneForFunctionCall(int clonedId, Node[] functionInputs)
            {
                if (_clonedId != clonedId)
                {
                    _clonedId = clonedId;
                    _cloned = new CallOutputNode(_thisFunction, OutputPosition, Call.CloneForFunctionCall(clonedId, functionInputs));
                }
                return _cloned;
            }

            public override Node CloneForLambdaInvoke(int clonedId, Node lambdaParameter, Node lambdaArgument)
            {
                if (_clonedId != clonedId)
                {
                    _clonedId = clonedId;
                    var clonedCall = Call.CloneForLambdaInvoke(clonedId, lambdaParameter, lambdaArgument);
                    _cloned = clonedCall == Call ? this : new CallOutputNode(_thisFunction, OutputPosition, clonedCall);
                }
                return _cloned;
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

                // Detect single-parameter, single-output functions (e.g. “♯”)
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

        public sealed class LambdaInvocationOutputNode : Node
        {
            public int OutputPosition { get; private set; } // 1 = → = output 2, 2 = ↓ = output 1
            public LambdaInvocation Invocation { get; private set; }

            public LambdaInvocationOutputNode(FuncitonFunction thisFunction, int outputPosition, LambdaInvocation invocation)
                : base(thisFunction)
            {
                OutputPosition = outputPosition;
                Invocation = invocation;
            }

            public override Node CloneForFunctionCall(int clonedId, Node[] functionInputs)
            {
                if (_clonedId != clonedId)
                {
                    _clonedId = clonedId;
                    _cloned = new LambdaInvocationOutputNode(_thisFunction, OutputPosition, Invocation.CloneForFunctionCall(clonedId, functionInputs));
                }
                return _cloned;
            }

            public override Node CloneForLambdaInvoke(int clonedId, Node lambdaParameter, Node lambdaArgument)
            {
                if (_clonedId != clonedId)
                {
                    _clonedId = clonedId;
                    var clonedInvocation = Invocation.CloneForLambdaInvoke(clonedId, lambdaParameter, lambdaArgument);
                    _cloned = clonedInvocation == Invocation ? this : new LambdaInvocationOutputNode(_thisFunction, OutputPosition, clonedInvocation);
                }
                return _cloned;
            }

            private int _state = 0;
            protected override Node nextToEvaluate()
            {
                switch (_state)
                {
                    case 0:
                        if (Invocation.Closure != null)
                            goto case 2;
                        _state = 1;
                        return Invocation.LambdaGetter;

                    case 1:
                        if (_previousSubresult >= _closures.Count || _previousSubresult < 1)
                            throw new InvalidOperationException("Attempt to invoke lambda #{0} which does not exist.".Fmt(_previousSubresult));
                        Invocation.Closure = _closures[(int) _previousSubresult];
                        goto case 2;

                    case 2:
                        _state = 3;
                        switch (OutputPosition)
                        {
                            case 1: // →
                                return Invocation.ClonedReturnValues.Item2;
                            case 2: // ↓
                                return Invocation.ClonedReturnValues.Item1;
                            default:
                                throw new InvalidOperationException("Attempt to retrieve lambda return value that does not exist.".Fmt(_previousSubresult));
                        }

                    case 3:
                        _result = _previousSubresult;
                        _state = 4;
                        return null;

                    default: // = 4
                        return null;
                }
            }

            protected override void releaseMemory()
            {
                if (_state == 3)
                    Invocation = null;
            }

            protected override void findChildNodes(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes, HashSet<Node> nodesUsedAsFunctionInputs, HashSet<Node> allNodes)
            {
                foreach (var inp in new[] { Invocation.LambdaGetter, Invocation.Argument })
                {
                    nodesUsedAsFunctionInputs.Add(inp);
                    inp.FindNodes(singleUseNodes, multiUseNodes, nodesUsedAsFunctionInputs, allNodes);
                }
            }

            protected override string getExpression(Node[] letNodes, bool requireParentheses)
            {
                return "{0}({1})[{2}]".Fmt(
                    Invocation.LambdaGetter.GetExpression(letNodes, false, true),
                    Invocation.Argument.GetExpression(letNodes, false, false),
                    "↓←↑→"[OutputPosition]);
            }
        }

        public sealed class LambdaExpressionParameterNode : Node
        {
            public int LambdaIdForGetExpression;
            public Node Argument;

            public LambdaExpressionParameterNode(FuncitonFunction thisFunction) : base(thisFunction) { }

            public override Node CloneForFunctionCall(int clonedId, Node[] functionInputs)
            {
                return this;
            }

            public override Node CloneForLambdaInvoke(int clonedId, Node lambdaParameter, Node lambdaArgument)
            {
                return lambdaParameter == this ? new LambdaExpressionParameterNode(_thisFunction) { Argument = lambdaArgument } : this;
            }

            protected override Node nextToEvaluate()
            {
                Argument.PreviousSubresult = _previousSubresult;
                var next = Node.NextToEvaluate(Argument);
                _result = Argument.Result;
                return next;
            }

            protected override void releaseMemory() { }
            protected override void findChildNodes(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes, HashSet<Node> nodesUsedAsFunctionInputs, HashSet<Node> allNodes) { }
            protected override string getExpression(Node[] letNodes, bool requireParentheses)
            {
                return "ρ" + LambdaIdForGetExpression;
            }
        }

        public sealed class LambdaExpressionNode : Node
        {
            public LambdaExpressionParameterNode Parameter { get; private set; }
            public Node ReturnValue1 { get; private set; }
            public Node ReturnValue2 { get; private set; }

            public LambdaExpressionNode(FuncitonFunction thisFunction, LambdaExpressionParameterNode parameter, Node return1, Node return2)
                : base(thisFunction)
            {
                Parameter = parameter;
                ReturnValue1 = return1;
                ReturnValue2 = return2;
            }

            public override Node CloneForFunctionCall(int clonedId, Node[] functionInputs)
            {
                if (clonedId != _clonedId)
                {
                    _clonedId = clonedId;
                    _cloned = new LambdaExpressionNode(
                        _thisFunction,
                        Parameter,
                        ReturnValue1.CloneForFunctionCall(clonedId, functionInputs),
                        ReturnValue2.CloneForFunctionCall(clonedId, functionInputs));
                }
                return _cloned;
            }

            public override Node CloneForLambdaInvoke(int clonedId, Node lambdaParameter, Node lambdaArgument)
            {
                if (clonedId != _clonedId)
                {
                    _clonedId = clonedId;
                    Helpers.Assert(lambdaParameter != Parameter);

                    var clonedReturn1 = ReturnValue1.CloneForLambdaInvoke(clonedId, lambdaParameter, lambdaArgument);
                    var clonedReturn2 = ReturnValue2.CloneForLambdaInvoke(clonedId, lambdaParameter, lambdaArgument);
                    _cloned = clonedReturn1 == ReturnValue1 && clonedReturn2 == ReturnValue2 ? this : new LambdaExpressionNode(_thisFunction, Parameter, clonedReturn1, clonedReturn2);
                }
                return _cloned;
            }

            private bool _evaluated = false;
            protected override Node nextToEvaluate()
            {
                if (!_evaluated)
                {
                    _result = _closures.Count;
                    _closures.Add(new LambdaClosure(Parameter, ReturnValue1, ReturnValue2));
                    _evaluated = true;
                }
                return null;
            }

            protected override void releaseMemory()
            {
            }

            protected override void findChildNodes(HashSet<Node> singleUseNodes, HashSet<Node> multiUseNodes, HashSet<Node> nodesUsedAsFunctionInputs, HashSet<Node> allNodes)
            {
                ReturnValue1.FindNodes(singleUseNodes, multiUseNodes, nodesUsedAsFunctionInputs, allNodes);
                ReturnValue2.FindNodes(singleUseNodes, multiUseNodes, nodesUsedAsFunctionInputs, allNodes);
            }

            private static int _lambdaCounter = 0;
            private int? _lambdaId = null;
            protected override string getExpression(Node[] letNodes, bool requireParentheses)
            {
                if (_lambdaId == null)
                {
                    _lambdaCounter++;
                    _lambdaId = _lambdaCounter;
                    Parameter.LambdaIdForGetExpression = _lambdaId.Value;
                }
                return "λ{0}(↓ = {1}, → = {2})".Fmt(
                    _lambdaId,
                    ReturnValue1.GetExpression(letNodes, false, false),
                    ReturnValue2.GetExpression(letNodes, false, false));
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

            public override Node CloneForFunctionCall(int clonedId, Node[] functionInputs)
            {
                if (_clonedId != clonedId)
                {
                    _clonedId = clonedId;
                    _cloned = new NandNode(_thisFunction, Left.CloneForFunctionCall(clonedId, functionInputs), Right.CloneForFunctionCall(clonedId, functionInputs));
                }
                return _cloned;
            }

            public override Node CloneForLambdaInvoke(int clonedId, Node lambdaParameter, Node lambdaArgument)
            {
                if (_clonedId != clonedId)
                {
                    _clonedId = clonedId;
                    var clonedLeft = Left.CloneForLambdaInvoke(clonedId, lambdaParameter, lambdaArgument);
                    var clonedRight = Right.CloneForLambdaInvoke(clonedId, lambdaParameter, lambdaArgument);
                    _cloned = clonedLeft == Left && clonedRight == Right ? this : new NandNode(_thisFunction, clonedLeft, clonedRight);
                }
                return _cloned;
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

            public override Node CloneForFunctionCall(int clonedId, Node[] functionInputs)
            {
                if (_clonedId != clonedId)
                {
                    _clonedId = clonedId;
                    _cloned = createNew(
                        Left.CloneForFunctionCall(clonedId, functionInputs),
                        Right.CloneForFunctionCall(clonedId, functionInputs));
                }
                return _cloned;
            }

            public override Node CloneForLambdaInvoke(int clonedId, Node lambdaParameter, Node lambdaArgument)
            {
                if (_clonedId != clonedId)
                {
                    _clonedId = clonedId;
                    var clonedLeft = Left.CloneForLambdaInvoke(clonedId, lambdaParameter, lambdaArgument);
                    var clonedRight = Right.CloneForLambdaInvoke(clonedId, lambdaParameter, lambdaArgument);
                    _cloned = clonedLeft == Left && clonedRight == Right ? this : createNew(clonedLeft, clonedRight);
                }
                return _cloned;
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

            public override Node CloneForFunctionCall(int clonedId, Node[] functionInputs)
            {
                if (_clonedId != clonedId)
                {
                    _clonedId = clonedId;
                    _cloned = new InputNode(_thisFunction, InputPosition) { _functionInputs = functionInputs };
                }
                return _cloned;
            }

            public override Node CloneForLambdaInvoke(int clonedId, Node lambdaParameter, Node lambdaArgument) { return this; }

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
            public override Node CloneForFunctionCall(int clonedId, Node[] functionInputs) { return this; }
            public override Node CloneForLambdaInvoke(int clonedId, Node lambdaParameter, Node lambdaArgument) { return this; }
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
            public override Node CloneForFunctionCall(int clonedId, Node[] functionInputs) { return this; }
            public override Node CloneForLambdaInvoke(int clonedId, Node lambdaParameter, Node lambdaArgument) { return this; }

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

        // List containing all lambda closures ever created. They are identified in Funciton by their index in this list.
        // Add a null element at the front so that they start numbering at 1, so you can still use 0 in Funciton to mean null/false
        private static List<LambdaClosure> _closures = new List<LambdaClosure> { null };

        private static int _cloneCounter = 0;

        public Node[] CloneOutputNodes(Node[] functionInputs)
        {
            _cloneCounter++;
            return OutputNodes.Select(node => node == null ? null : node.CloneForFunctionCall(_cloneCounter, functionInputs)).ToArray();
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
