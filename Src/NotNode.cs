using System.Numerics;

namespace Funciton
{
    sealed class NotNode(FuncitonFunction thisFunction, Node arg) : Node(thisFunction)
    {
        public Node Argument { get; private set; } = arg;

        public override Node CloneForFunctionCall(int clonedId, Node[] functionInputs)
        {
            if (_clonedId != clonedId)
            {
                _clonedId = clonedId;
                _cloned = new NotNode(_thisFunction, Argument.CloneForFunctionCall(clonedId, functionInputs));
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
                var clonedArg = Argument.CloneForLambdaInvoke(clonedId, lambdaParameter, lambdaArgument);
                _cloned = clonedArg == Argument ? this : new NotNode(_thisFunction, clonedArg);
            }
            return _cloned;
        }

        private int _state = 0;
        public override bool IsEvaluated => _state == 2;
        public override Node NextToEvaluate(BigInteger previousSubresult)
        {
            switch (_state)
            {
                case 0:
                    _state = 1;
                    return Argument;
                case 1:
                    _result = ~previousSubresult;
                    _state = 2;
                    return null;
                default: // = 2
                    return null;
            }
        }

        protected override void releaseMemory()
        {
            if (_state >= 1)
                Argument = null;
        }

        protected override void findChildNodes(FindNodesResult fnr) => Argument.FindNodes(fnr);

        protected override string getExpression(Node[] letNodes, bool requireParentheses, bool requireOutputArrow) =>
            $"¬{Argument.GetExpression(letNodes, false, true, requireOutputArrow)}";
    }
}
