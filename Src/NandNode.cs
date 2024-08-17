using System.Linq;
using System.Numerics;

namespace Funciton
{
    sealed class NandNode(FuncitonFunction thisFunction, Node left, Node right) : Node(thisFunction)
    {
        public Node Left { get; private set; } = left;
        public Node Right { get; private set; } = right;

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
        public override Node NextToEvaluate(BigInteger previousSubresult)
        {
            switch (_state)
            {
                case 0:
                    _state = 1;
                    return Left;
                case 1:
                    if (previousSubresult.IsZero)
                    {
                        // short-circuit evaluation
                        _result = BigInteger.MinusOne;
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
                    _result = ~(_leftEval & previousSubresult);
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

            if (Left is NandNode leftNand)
            {
                // detect “or” (¬a @ ¬b = a | b)
                if (leftNand.Left == leftNand.Right && Right is NandNode rightNand && rightNand.Left == rightNand.Right && (letNodes == null || !letNodes.Contains(Left)) && (letNodes == null || !letNodes.Contains(Right)))
                    return $"{open}{leftNand.Left.GetExpression(letNodes, false, true, requireOutputArrow)} | {rightNand.Left.GetExpression(letNodes, false, true, requireOutputArrow)}{close}";

                // detect “and” (¬(a @ b) = a & b)
                if (Left == Right && (letNodes == null || !letNodes.Contains(Left)))
                    return $"{open}{leftNand.Left.GetExpression(letNodes, false, true, requireOutputArrow)} & {leftNand.Right.GetExpression(letNodes, false, true, requireOutputArrow)}{close}";
            }

            // detect “not” (a @ a = ¬a)
            if (Left == Right)
                return $"¬{Left.GetExpression(letNodes, false, true, requireOutputArrow)}";

            // actual NAND
            return $"{open}{Left.GetExpression(letNodes, false, true, requireOutputArrow)} @ {Right.GetExpression(letNodes, false, true, requireOutputArrow)}{close}";
        }
    }
}
