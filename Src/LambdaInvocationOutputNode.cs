using System;
using System.Linq;
using System.Numerics;

namespace Funciton
{
    sealed class LambdaInvocationOutputNode(FuncitonFunction thisFunction, int outputPosition, LambdaInvocation invocation) : Node(thisFunction)
    {
        public int OutputPosition { get; private set; } = outputPosition;   // 1 = → = output 2, 2 = ↓ = output 1
        public LambdaInvocation Invocation { get; private set; } = invocation;

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
        public override bool IsEvaluated => _state == 4;
        public override Node NextToEvaluate(BigInteger previousSubresult)
        {
            switch (_state)
            {
                case 0:
                    if (Invocation.Closure != null)
                        goto case 2;
                    _state = 1;
                    return Invocation.LambdaGetter;

                case 1:
                    if (previousSubresult >= FuncitonLanguage.LambdaClosures.Count || previousSubresult < 1)
                        throw new InvalidOperationException($"Attempt to invoke lambda #{previousSubresult} which does not exist.");
                    Invocation.Closure = FuncitonLanguage.LambdaClosures[(int) previousSubresult];
                    goto case 2;

                case 2:
                    _state = 3;
                    return OutputPosition switch
                    {
                        1 /* → */ => Invocation.ClonedReturnValues.return2,
                        2 /* ↓ */ => Invocation.ClonedReturnValues.return1,
                        _ => throw new InvalidOperationException("Attempt to retrieve lambda return value that does not exist."),
                    };
                case 3:
                    _result = previousSubresult;
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

        protected override string getExpression(Node[] letNodes, bool requireParentheses, bool requireOutputArrow) =>
            $"{Invocation.LambdaGetter.GetExpression(letNodes, false, true, requireOutputArrow)}({Invocation.Argument.GetExpression(letNodes, false, false, requireOutputArrow)}){(requireOutputArrow ? $"[{"↓←↑→"[OutputPosition]}]" : "")}";
    }
}
