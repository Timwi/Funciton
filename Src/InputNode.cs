using System.Numerics;

namespace Funciton
{
    sealed class InputNode(FuncitonFunction thisFunction, int inputPosition) : Node(thisFunction)
    {
        public int InputPosition { get; private set; } = inputPosition;

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

        public override Node CloneForLambdaInvoke(int clonedId, LambdaExpressionParameterNode lambdaParameter, Node lambdaArgument) => this;

        public override Node NextToEvaluate(BigInteger previousSubresult)
        {
            var next = _functionInputs[InputPosition].NextToEvaluate(previousSubresult);
            _result = _functionInputs[InputPosition].Result;
            return next;
        }

        protected override void releaseMemory() { }
        protected override void findChildNodes(FindNodesResult fnr) { }
        protected override string getExpression(Node[] letNodes, bool requireParentheses, bool requireOutputArrow) => "↑→↓←".Substring(InputPosition, 1);
        public override void FindNodes(FindNodesResult fnr)
        {
            fnr.AllNodes.Add(this);
            fnr.SingleUseNodes.Add(this);
        }
    }
}
