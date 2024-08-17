using System.Linq;
using System.Numerics;

namespace Funciton
{
    // This is the only Node type that is not immutable because it is the only one that allows a cycle in the code graph
    sealed class LambdaExpressionNode(FuncitonFunction thisFunction) : Node(thisFunction)
    {
        public LambdaExpressionParameterNode Parameter { get; set; }
        public Node ReturnValue1 { get; set; }
        public Node ReturnValue2 { get; set; }

        // Used during CloneForLambdaInvoke to determine which lambdas are nested inside which others
        public LambdaExpressionParameterNode[] OuterParameters { get; set; }

        public override Node CloneForFunctionCall(int clonedId, Node[] functionInputs)
        {
            if (clonedId != _clonedId)
            {
                _clonedId = clonedId;
                // Do not use object initialization syntax for the return values because the recursive call needs to return this instance
                var cloned = new LambdaExpressionNode(_thisFunction) { Parameter = Parameter };
                _cloned = cloned;
                cloned.ReturnValue1 = ReturnValue1.CloneForFunctionCall(clonedId, functionInputs);
                cloned.ReturnValue2 = ReturnValue2.CloneForFunctionCall(clonedId, functionInputs);
            }
            return _cloned;
        }

        public override Node CloneForLambdaInvoke(int clonedId, LambdaExpressionParameterNode lambdaParameter, Node lambdaArgument)
        {
            if (_evaluated || lambdaParameter == Parameter || (OuterParameters != null && OuterParameters.Contains(lambdaParameter)))
                return this;

            if (clonedId != _clonedId)
            {
                _clonedId = clonedId;
                var cloned = new LambdaExpressionNode(_thisFunction) { Parameter = Parameter };
                _cloned = cloned;
                cloned.ReturnValue1 = ReturnValue1.CloneForLambdaInvoke(clonedId, lambdaParameter, lambdaArgument);
                cloned.ReturnValue2 = ReturnValue2.CloneForLambdaInvoke(clonedId, lambdaParameter, lambdaArgument);
                cloned.OuterParameters = OuterParameters.ArrayUnion(lambdaParameter);
            }
            return _cloned;
        }

        private bool _evaluated = false;
        public override Node NextToEvaluate(BigInteger previousSubresult)
        {
            if (!_evaluated)
            {
                _result = FuncitonLanguage.LambdaClosures.Count;
                FuncitonLanguage.LambdaClosures.Add(new LambdaClosure(Parameter, ReturnValue1, ReturnValue2));
                _evaluated = true;
            }
            return null;
        }

        protected override void releaseMemory()
        {
        }

        protected override void findChildNodes(FindNodesResult fnr)
        {
            fnr.NodesUsedAsFunctionInputs.Add(ReturnValue1);
            fnr.NodesUsedAsFunctionInputs.Add(ReturnValue2);
            ReturnValue1.FindNodes(fnr);
            ReturnValue2.FindNodes(fnr);
        }

        protected override string getExpression(Node[] letNodes, bool requireParentheses, bool requireOutputArrow)
        {
            // Use • to specify that a lambda expression ignores its parameter
            var id = Parameter == null ? "•" : (char) ('α' + Parameter.LambdaParameterId) + "·";

            // If the second return value is a literal 0, omit it
            return ReturnValue2 is LiteralNode lit && lit.Result == 0
                ? $"{id}{ReturnValue1.GetExpression(letNodes, false, true, requireOutputArrow)}"
                : $"{id}[{ReturnValue1.GetExpression(letNodes, false, false, requireOutputArrow)}, {ReturnValue2.GetExpression(letNodes, false, false, requireOutputArrow)}]";
        }
    }
}
