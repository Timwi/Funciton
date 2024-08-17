using System.Numerics;

namespace Funciton
{
    sealed class LambdaExpressionParameterNode : Node
    {
        public static int LambdaParameterCounter = 0;
        public int LambdaParameterId; // for getExpression
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

        public override Node NextToEvaluate(BigInteger previousSubresult)
        {
            var next = Argument.NextToEvaluate(previousSubresult);
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
}
