namespace Funciton
{
    sealed class LambdaClosure(LambdaExpressionParameterNode parameter, Node return1, Node return2)
    {
        public LambdaExpressionParameterNode Parameter { get; private set; } = parameter;
        public (Node return1, Node return2) CloneReturnValues(Node argument)
        {
            FuncitonLanguage.CloneCounter++;
            return (
                return1.CloneForLambdaInvoke(FuncitonLanguage.CloneCounter, Parameter, argument),
                return2.CloneForLambdaInvoke(FuncitonLanguage.CloneCounter, Parameter, argument));
        }
    }
}
