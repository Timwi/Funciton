using System;
using System.Numerics;

namespace Funciton
{
    sealed class StdInNode(FuncitonFunction thisFunction) : Node(thisFunction)
    {
        private static BigInteger? _stdin;

        public override Node CloneForFunctionCall(int clonedId, Node[] functionInputs) { return this; }
        public override Node CloneForLambdaInvoke(int clonedId, LambdaExpressionParameterNode lambdaParameter, Node lambdaArgument) { return this; }

        private bool _evaluated = false;
        public override Node NextToEvaluate(BigInteger previousSubresult)
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
}
