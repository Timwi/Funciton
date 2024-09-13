using System.Numerics;

namespace Funciton
{
    abstract class BinaryOperatorNode(FuncitonFunction thisFunction, Node left, Node right) : Node(thisFunction)
    {
        public Node Left { get; private set; } = left;
        public Node Right { get; private set; } = right;
        public abstract BigInteger Calculate(BigInteger one, BigInteger two);
        public abstract BinaryOperatorNode Create(FuncitonFunction thisFunction, Node left, Node right);

        public override Node CloneForFunctionCall(int clonedId, Node[] functionInputs)
        {
            if (_clonedId != clonedId)
            {
                _clonedId = clonedId;
                _cloned = Create(_thisFunction, Left.CloneForFunctionCall(clonedId, functionInputs), Right.CloneForFunctionCall(clonedId, functionInputs));
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
                _cloned = clonedLeft == Left && clonedRight == Right ? this : Create(_thisFunction, clonedLeft, clonedRight);
            }
            return _cloned;
        }

        private int _state = 0;
        private BigInteger _leftEval;
        public override bool IsEvaluated => _state == 3;
        protected abstract BigInteger? shortcircuit(BigInteger value);
        public override Node NextToEvaluate(BigInteger previousSubresult)
        {
            switch (_state)
            {
                case 0:
                    _state = 1;
                    return Left;
                case 1:
                    // short-circuit evaluation
                    if (shortcircuit(previousSubresult) is BigInteger shorted)
                    {
                        _result = shorted;
                        _state = 3;
                        return null;
                    }
                    else
                    {
                        _leftEval = previousSubresult;
                        _state = 2;
                        return Right;
                    }
                case 2:
                    _result = Calculate(_leftEval, previousSubresult);
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

        protected abstract string operatorString { get; }
        protected override string getExpression(Node[] letNodes, bool requireParentheses, bool requireOutputArrow)
        {
            var open = requireParentheses ? "(" : "";
            var close = requireParentheses ? ")" : "";
            var leftExpr = Left.GetExpression(letNodes, false, true, requireOutputArrow);
            var rightExpr = Right.GetExpression(letNodes, false, true, requireOutputArrow);
            return $"{open}{leftExpr} {operatorString} {rightExpr}{close}";
        }
    }
}
