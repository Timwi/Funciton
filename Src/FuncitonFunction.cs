using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Funciton
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
            public abstract Node CloneForLambdaInvoke(int clonedId, LambdaExpressionParameterNode lambdaParameter, Node lambdaArgument);

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
                    notAValidList:;
                }
                catch { }

                ConsoleWriteLineColored(
                    ConsoleColor.White, _thisFunction.Name, ": ",
                    ConsoleColor.Gray, getExpression(null, false, true) + " ",
                    ConsoleColor.White, "= ",
                    ConsoleColor.Green, _result.ToString(),
                    str == null ? null : new object[] { " ", ConsoleColor.White, "= ", ConsoleColor.DarkCyan, str },
                    list == null ? null : new object[] { " ", ConsoleColor.White, "= ", ConsoleColor.DarkMagenta, list });
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

            public virtual void FindNodes(FindNodesResult fnr)
            {
                fnr.AllNodes.Add(this);

                if (fnr.MultiUseNodes.Contains(this))
                    return;
                if (fnr.SingleUseNodes.Contains(this))
                {
                    fnr.SingleUseNodes.Remove(this);
                    fnr.MultiUseNodes.Add(this);
                    fnr.LetNodes.Add(this);
                    return;
                }
                fnr.SingleUseNodes.Add(this);
                findChildNodes(fnr);
            }

            protected abstract void findChildNodes(FindNodesResult fnr);

            public string GetExpression(Node[] letNodes, bool alwaysExpand, bool requireParentheses, bool requireOutputArrow)
            {
                int pos;
                if (letNodes != null && !alwaysExpand && (pos = Array.IndexOf(letNodes, this)) != -1)
                    return ((char) ('a' + pos)).ToString();
                return getExpression(letNodes, requireParentheses, requireOutputArrow);
            }

            protected abstract string getExpression(Node[] letNodes, bool requireParentheses, bool requireOutputArrow);
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

            public Call CloneForLambdaInvoke(int clonedId, LambdaExpressionParameterNode lambdaParameter, Node lambdaArgument)
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

            public LambdaInvocation CloneForLambdaInvoke(int clonedId, LambdaExpressionParameterNode lambdaParameter, Node lambdaArgument)
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
            public LambdaExpressionParameterNode Parameter { get; private set; }
            private Node _returnValue1;
            private Node _returnValue2;

            public LambdaClosure(LambdaExpressionParameterNode parameter, Node return1, Node return2)
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

            public override Node CloneForLambdaInvoke(int clonedId, LambdaExpressionParameterNode lambdaParameter, Node lambdaArgument)
            {
                if (_state == 2)    // fully evaluated
                    return this;
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

            protected override void findChildNodes(FindNodesResult fnr)
            {
                if (Call.Function.OutputNodes.Count(n => n != null) > 1)
                    fnr.LetNodes.Add(this);
                if (fnr.Calls.Add(Call))
                {
                    foreach (var inp in Call.Inputs.Where(i => i != null))
                    {
                        fnr.NodesUsedAsFunctionInputs.Add(inp);
                        inp.FindNodes(fnr);
                    }
                }
            }

            protected override string getExpression(Node[] letNodes, bool requireParentheses, bool requireOutputArrow)
            {
                var open = requireParentheses ? "(" : "";
                var close = requireParentheses ? ")" : "";

                // Detect single-parameter, single-output functions (e.g. “♯”)
                var inputIndexes = Call.Inputs.Select((node, i) => node == null ? -1 : i).Where(i => i != -1).ToArray();
                var outputIndexes = Call.Function.OutputNodes.Select((node, i) => node == null ? -1 : i).Where(i => i != -1).ToArray();
                if (inputIndexes.Length == 1 && outputIndexes.Length == 1)
                    return Call.Function.Name + "(" + Call.Inputs.Select((inp, ind) => inp == null ? null : inp.GetExpression(letNodes, false, false, requireOutputArrow)).First(str => str != null) + ")";

                // Detect two-opposite-parameter, single-perpendicular-output functions (normally binary operators, e.g. “<”)
                var config = string.Join("", outputIndexes) + "/" + string.Join("", inputIndexes);
                if (config == "0/13" || config == "3/02")
                    return open + Call.Inputs[inputIndexes[1]].GetExpression(letNodes, false, false, requireOutputArrow) + " " + Call.Function.Name + " " + Call.Inputs[inputIndexes[0]].GetExpression(letNodes, false, true, requireOutputArrow) + close;
                else if (config == "1/02" || config == "2/13")
                    return open + Call.Inputs[inputIndexes[0]].GetExpression(letNodes, false, false, requireOutputArrow) + " " + Call.Function.Name + " " + Call.Inputs[inputIndexes[1]].GetExpression(letNodes, false, true, requireOutputArrow) + close;

                // Fall back to verbose notation
                return Call.Function.Name + "(" +
                    string.Join(", ", Call.Inputs.Select((inp, ind) => inp == null ? null : /*(inputIndexes.Length > 1 ? "↑→↓←"[ind] + ": " : "") +*/ inp.GetExpression(letNodes, false, false, requireOutputArrow)).Where(str => str != null).Reverse()) +
                    ")" + (requireOutputArrow && outputIndexes.Length > 1 ? "[" + "↓←↑→"[OutputPosition] + "]" : "");
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

            public override Node CloneForLambdaInvoke(int clonedId, LambdaExpressionParameterNode lambdaParameter, Node lambdaArgument)
            {
                if (_state == 4)    // fully evaluated
                    return this;
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
                        if (_previousSubresult >= LambdaClosures.Count || _previousSubresult < 1)
                            throw new InvalidOperationException("Attempt to invoke lambda #{0} which does not exist.".Fmt(_previousSubresult));
                        Invocation.Closure = LambdaClosures[(int) _previousSubresult];
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

            protected override void findChildNodes(FindNodesResult fnr)
            {
                if (OutputPosition != 2)
                    fnr.LetNodes.Add(this);
                if (fnr.Invocations.Add(Invocation))
                {
                    foreach (var inp in new[] { Invocation.LambdaGetter, Invocation.Argument })
                    {
                        fnr.NodesUsedAsFunctionInputs.Add(inp);
                        inp.FindNodes(fnr);
                    }
                }
            }

            protected override string getExpression(Node[] letNodes, bool requireParentheses, bool requireOutputArrow)
            {
                return "{0}({1}){2}{3}{4}".Fmt(
                    Invocation.LambdaGetter.GetExpression(letNodes, false, true, requireOutputArrow),
                    Invocation.Argument.GetExpression(letNodes, false, false, requireOutputArrow),
                    requireOutputArrow ? "[" : null,
                    requireOutputArrow ? "↓←↑→"[OutputPosition] : (object) null,
                    requireOutputArrow ? "]" : null);
            }
        }

        public sealed class LambdaExpressionParameterNode : Node
        {
            public static int LambdaParameterCounter = 0;
            public int LambdaParameterId; // for GetExpression
            public Node Argument;

            public LambdaExpressionParameterNode(FuncitonFunction thisFunction)
                : base(thisFunction)
            {
                LambdaParameterId = LambdaParameterCounter++;
            }

            public LambdaExpressionParameterNode(FuncitonFunction thisFunction, int id)
                : base(thisFunction)
            {
                LambdaParameterId = id;
            }

            public override Node CloneForFunctionCall(int clonedId, Node[] functionInputs)
            {
                return this;
            }

            public override Node CloneForLambdaInvoke(int clonedId, LambdaExpressionParameterNode lambdaParameter, Node lambdaArgument)
            {
                return lambdaParameter == this ? new LambdaExpressionParameterNode(_thisFunction, LambdaParameterId) { Argument = lambdaArgument } : this;
            }

            protected override Node nextToEvaluate()
            {
                Argument.PreviousSubresult = _previousSubresult;
                var next = Node.NextToEvaluate(Argument);
                _result = Argument.Result;
                return next;
            }

            protected override void releaseMemory() { }
            protected override void findChildNodes(FindNodesResult fnr) { }
            protected override string getExpression(Node[] letNodes, bool requireParentheses, bool requireOutputArrow)
            {
                return ((char) ('α' + LambdaParameterId)).ToString();
            }
            public override void FindNodes(FindNodesResult fnr)
            {
                fnr.AllNodes.Add(this);
                fnr.SingleUseNodes.Add(this);
            }
        }

        // This is the only Node type that is not immutable because it is the only one that allows a cycle in the code graph
        public sealed class LambdaExpressionNode : Node
        {
            public LambdaExpressionParameterNode Parameter { get; set; }
            public Node ReturnValue1 { get; set; }
            public Node ReturnValue2 { get; set; }

            // Used during CloneForLambdaInvoke to determine which lambdas are nested inside which others
            public LambdaExpressionParameterNode[] OuterParameters { get; set; }

            public LambdaExpressionNode(FuncitonFunction thisFunction) : base(thisFunction) { }

            public override Node CloneForFunctionCall(int clonedId, Node[] functionInputs)
            {
                if (clonedId != _clonedId)
                {
                    _clonedId = clonedId;
                    // Do not use object initialization syntax for the return values because the recursive call needs to return this instance
                    var cloned = new LambdaExpressionNode(_thisFunction) { Parameter = Parameter };
                    _cloned = cloned;
                    cloned.ReturnValue1 = ReturnValue1.CloneForFunctionCall(clonedId, functionInputs);
                    cloned.ReturnValue2 = ReturnValue2.CloneForFunctionCall(clonedId, functionInputs);
                }
                return _cloned;
            }

            public override Node CloneForLambdaInvoke(int clonedId, LambdaExpressionParameterNode lambdaParameter, Node lambdaArgument)
            {
                if (_evaluated || lambdaParameter == Parameter || (OuterParameters != null && OuterParameters.Contains(lambdaParameter)))
                    return this;

                if (clonedId != _clonedId)
                {
                    _clonedId = clonedId;
                    var cloned = new LambdaExpressionNode(_thisFunction) { Parameter = Parameter };
                    _cloned = cloned;
                    cloned.ReturnValue1 = ReturnValue1.CloneForLambdaInvoke(clonedId, lambdaParameter, lambdaArgument);
                    cloned.ReturnValue2 = ReturnValue2.CloneForLambdaInvoke(clonedId, lambdaParameter, lambdaArgument);
                    cloned.OuterParameters = OuterParameters.ArrayUnion(lambdaParameter);
                }
                return _cloned;
            }

            private bool _evaluated = false;
            protected override Node nextToEvaluate()
            {
                if (!_evaluated)
                {
                    _result = LambdaClosures.Count;
                    LambdaClosures.Add(new LambdaClosure(Parameter, ReturnValue1, ReturnValue2));
                    _evaluated = true;
                }
                return null;
            }

            protected override void releaseMemory()
            {
            }

            protected override void findChildNodes(FindNodesResult fnr)
            {
                fnr.NodesUsedAsFunctionInputs.Add(ReturnValue1);
                fnr.NodesUsedAsFunctionInputs.Add(ReturnValue2);
                ReturnValue1.FindNodes(fnr);
                ReturnValue2.FindNodes(fnr);
            }

            protected override string getExpression(Node[] letNodes, bool requireParentheses, bool requireOutputArrow)
            {
                // Use • to specify that a lambda expression ignores its parameter
                var id = Parameter == null ? "•" : (char) ('α' + Parameter.LambdaParameterId) + "·";

                // If the second return value is a literal 0, omit it
                if (ReturnValue2 is LiteralNode && ((LiteralNode) ReturnValue2).Result == 0)
                    return "{0}{1}".Fmt(id, ReturnValue1.GetExpression(letNodes, false, true, requireOutputArrow));

                return "{0}[{1}, {2}]".Fmt(
                    id,
                    ReturnValue1.GetExpression(letNodes, false, false, requireOutputArrow),
                    ReturnValue2.GetExpression(letNodes, false, false, requireOutputArrow));
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

            public override Node CloneForLambdaInvoke(int clonedId, LambdaExpressionParameterNode lambdaParameter, Node lambdaArgument)
            {
                if (_state == 3)    // fully evaluated
                    return this;
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

            protected override void findChildNodes(FindNodesResult fnr)
            {
                Left.FindNodes(fnr);
                // If the two nodes are the same, this NAND is used to express a NOT.
                // getExpression() will recognize that and just output a unary NOT (¬).
                // In such a case, we should not allocate a variable for it if that is its only use.
                if (Right != Left)
                    Right.FindNodes(fnr);
            }

            protected override string getExpression(Node[] letNodes, bool requireParentheses, bool requireOutputArrow)
            {
                var open = requireParentheses ? "(" : "";
                var close = requireParentheses ? ")" : "";

                // detect “or” (¬a @ ¬b = a | b)
                var leftNand = Left as NandNode;
                var rightNand = Right as NandNode;
                if (leftNand != null && leftNand.Left == leftNand.Right && rightNand != null && rightNand.Left == rightNand.Right && (letNodes == null || !letNodes.Contains(Left)) && (letNodes == null || !letNodes.Contains(Right)))
                    return open + leftNand.Left.GetExpression(letNodes, false, true, requireOutputArrow) + " | " + rightNand.Left.GetExpression(letNodes, false, true, requireOutputArrow) + close;

                // detect “and” (¬(a @ b) = a & b)
                if (Left == Right && leftNand != null && (letNodes == null || !letNodes.Contains(Left)))
                    return open + leftNand.Left.GetExpression(letNodes, false, true, requireOutputArrow) + " & " + leftNand.Right.GetExpression(letNodes, false, true, requireOutputArrow) + close;

                // detect “not” (a @ a = ¬a)
                if (Left == Right)
                    return "¬" + Left.GetExpression(letNodes, false, true, requireOutputArrow);

                return open + Left.GetExpression(letNodes, false, true, requireOutputArrow) + " @ " + Right.GetExpression(letNodes, false, true, requireOutputArrow) + close;
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

            public override Node CloneForLambdaInvoke(int clonedId, LambdaExpressionParameterNode lambdaParameter, Node lambdaArgument)
            {
                if (_state == 3)    // fully evaluated
                    return this;
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

            protected override void findChildNodes(FindNodesResult fnr)
            {
                Left.FindNodes(fnr);
                Right.FindNodes(fnr);
            }

            protected override string getExpression(Node[] letNodes, bool requireParentheses, bool requireOutputArrow)
            {
                var open = requireParentheses ? "(" : "";
                var close = requireParentheses ? ")" : "";
                return open + Left.GetExpression(letNodes, false, true, requireOutputArrow) + _operator + Right.GetExpression(letNodes, false, true, requireOutputArrow) + close;
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

            public override Node CloneForLambdaInvoke(int clonedId, LambdaExpressionParameterNode lambdaParameter, Node lambdaArgument) { return this; }

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

            protected override void findChildNodes(FindNodesResult fnr) { }
            protected override string getExpression(Node[] letNodes, bool requireParentheses, bool requireOutputArrow) { return "↑→↓←".Substring(InputPosition, 1); }
            public override void FindNodes(FindNodesResult fnr)
            {
                fnr.AllNodes.Add(this);
                fnr.SingleUseNodes.Add(this);
            }
        }

        public sealed class LiteralNode : Node
        {
            public LiteralNode(FuncitonFunction thisFunction, BigInteger literal) : base(thisFunction) { _result = literal; }
            public override Node CloneForFunctionCall(int clonedId, Node[] functionInputs) { return this; }
            public override Node CloneForLambdaInvoke(int clonedId, LambdaExpressionParameterNode lambdaParameter, Node lambdaArgument) { return this; }
            protected override Node nextToEvaluate() { return null; }
            protected override void releaseMemory() { }
            protected override void findChildNodes(FindNodesResult fnr) { }
            protected override string getExpression(Node[] letNodes, bool requireParentheses, bool requireOutputArrow) { return _result.ToString().Replace('-', '−'); }
            public override void FindNodes(FindNodesResult fnr)
            {
                fnr.AllNodes.Add(this);
                fnr.SingleUseNodes.Add(this);
            }
        }

        public sealed class StdInNode : Node
        {
            private static BigInteger? _stdin;
            public StdInNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            public override Node CloneForFunctionCall(int clonedId, Node[] functionInputs) { return this; }
            public override Node CloneForLambdaInvoke(int clonedId, LambdaExpressionParameterNode lambdaParameter, Node lambdaArgument) { return this; }

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
            protected override void findChildNodes(FindNodesResult fnr) { }
            protected override string getExpression(Node[] letNodes, bool requireParentheses, bool requireOutputArrow) { return "♦"; }
            public override void FindNodes(FindNodesResult fnr)
            {
                fnr.AllNodes.Add(this);
                fnr.SingleUseNodes.Add(this);
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
        public static readonly List<LambdaClosure> LambdaClosures = new List<LambdaClosure> { null };

        private static int _cloneCounter = 0;

        public Node[] CloneOutputNodes(Node[] functionInputs)
        {
            _cloneCounter++;
            return OutputNodes.Select(node => node == null ? null : node.CloneForFunctionCall(_cloneCounter, functionInputs)).ToArray();
        }

        public void Analyze(StringBuilder sb)
        {
            // Pass one: determine which nodes are single-use and which are multi-use
            var nodes = FindNodes();

            // Pass two: generate expressions
            sb.AppendLine(string.Format("Analysis of {0}:",
                Name == "" ? "main program" : string.Format("{0}({1})", Name, string.Join(", ", nodes.AllNodes.OfType<InputNode>().OrderByDescending(i => i.InputPosition).Select(i => "↑→↓←"[i.InputPosition])))));

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
                            .Concat(new[] { new { lion.OutputPosition, Index = i } })
                            .ToArray()
                        : letNodes
                            .Skip(i + 1)
                            .Select((ln, ix) => new { Node = ln as CallOutputNode, Index = ix + i + 1 })
                            .Where(x => x.Node != null && x.Node.Call == con.Call)
                            .Select(x => new { x.Node.OutputPosition, x.Index })
                            .Concat(new[] { new { con.OutputPosition, Index = i } })
                            .ToArray();

                    var outPosses = lion != null
                        ? new[] { 2, 1 }
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
                    sb.AppendLine(string.Format("    let {0} := {1}[{2}]",
                        string.Format(outputs.Length == 1 ? "{0}" : "[{0}]", string.Join(", ", outputs.Select(op => op.Letter))),
                        letNodes[i].GetExpression(letNodes, true, false, false),
                        string.Join(", ", outputs.Select(op => op.Dir))));
                    continue;
                }

                sb.AppendLine(string.Format("    let {0} := {1}", (char) ('a' + i), letNodes[i].GetExpression(letNodes, true, false, false)));
            }

            for (int i = 0; i < OutputNodes.Length; i++)
            {
                if (OutputNodes[i] == null)
                    continue;
                sb.Append("    output ");
                sb.Append("↓←↑→"[i] + " := " + OutputNodes[i].GetExpression(letNodes, false, false, false));
                sb.AppendLine();
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

        public int? GetInputForOutputIfNop(int outputPosition)
        {
            var input = OutputNodes[outputPosition] as InputNode;
            return input == null ? (int?) null : input.InputPosition;
        }

        public override string ToString()
        {
            return Name == "" ? "main program" : "function " + Name;
        }
    }
}
