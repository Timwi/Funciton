using System;
using System.Numerics;

namespace Funciton
{
    sealed class InputNode(FuncitonFunction thisFunction, int inputPosition) : Node(thisFunction)
    {
        public int InputPosition { get; private set; } = inputPosition;

        // We don’t need clone this node, we just directly link to the function argument
        public override Node CloneForFunctionCall(int clonedId, Node[] functionInputs) => functionInputs[InputPosition];

        // Since this node is never used in evaluation, none of these methods should ever be called
        public override Node CloneForLambdaInvoke(int clonedId, LambdaExpressionParameterNode lambdaParameter, Node lambdaArgument) => throw new InvalidOperationException();
        public override bool IsEvaluated => throw new InvalidOperationException();
        public override Node NextToEvaluate(BigInteger previousSubresult) => throw new InvalidOperationException();
        protected override void releaseMemory() => throw new InvalidOperationException();

        protected override void findChildNodes(FindNodesResult fnr) { }
        protected override string getExpression(Node[] letNodes, bool requireParentheses, bool requireOutputArrow) => "↑→↓←".Substring(InputPosition, 1);
        public override void FindNodes(FindNodesResult fnr)
        {
            fnr.AllNodes.Add(this);
            fnr.SingleUseNodes.Add(this);
        }
    }
}
