using System.Numerics;

namespace Funciton
{
    abstract class CrossWireNode(FuncitonFunction thisFunction, Node left, Node right) : Node(thisFunction)
    {
        public Node Left { get; private set; } = left;
        public Node Right { get; private set; } = right;

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
        public override bool IsEvaluated => _state == 3;
        public override Node NextToEvaluate(BigInteger previousSubresult)
        {
            switch (_state)
            {
                case 0:
                    _state = 1;
                    return Left;
                case 1:
                    _leftEval = previousSubresult;
                    _state = 2;
                    return Right;
                case 2:
                    _result = getResult(_leftEval, previousSubresult);
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
}
