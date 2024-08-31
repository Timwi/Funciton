using System.Numerics;

namespace Funciton
{
    sealed class LiteralNode : Node
    {
        public LiteralNode(FuncitonFunction thisFunction, BigInteger literal) : base(thisFunction) => _result = literal;
        public override Node CloneForFunctionCall(int clonedId, Node[] functionInputs) => this;
        public override Node CloneForLambdaInvoke(int clonedId, LambdaExpressionParameterNode lambdaParameter, Node lambdaArgument) => this;
        public override Node NextToEvaluate(BigInteger previousSubresult) => null;
        public override bool IsEvaluated => true;
        protected override void releaseMemory() { }
        protected override void findChildNodes(FindNodesResult fnr) { }
        protected override string getExpression(Node[] letNodes, bool requireParentheses, bool requireOutputArrow) => _result.ToString().Replace('-', '−');
        public override void FindNodes(FindNodesResult fnr)
        {
            fnr.AllNodes.Add(this);
            fnr.SingleUseNodes.Add(this);
        }
    }
}
