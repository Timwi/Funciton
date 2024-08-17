namespace Funciton
{
    sealed class LambdaInvocation(Node argument, Node lambdaGetter)
    {
        public Node Argument { get; private set; } = argument;
        public Node LambdaGetter { get; private set; } = lambdaGetter;
        public LambdaClosure Closure { get; set; }  // only set after LambdaGetter is evaluated

        private LambdaInvocation _cloned;
        private int _clonedId;

        private (Node return1, Node return2)? _clonedReturnValues;
        public (Node return1, Node return2) ClonedReturnValues => _clonedReturnValues ??= Closure.CloneReturnValues(Argument);

        public LambdaInvocation CloneForFunctionCall(int clonedId, Node[] functionInputs)
        {
            if (_clonedId != clonedId)
            {
                _clonedId = clonedId;
                _cloned = new LambdaInvocation(Argument.CloneForFunctionCall(clonedId, functionInputs), LambdaGetter.CloneForFunctionCall(clonedId, functionInputs));
            }
            return _cloned;
        }

        public LambdaInvocation CloneForLambdaInvoke(int clonedId, LambdaExpressionParameterNode lambdaParameter, Node lambdaArgument)
        {
            if (_clonedId != clonedId)
            {
                _clonedId = clonedId;
                var clonedArgument = Argument.CloneForLambdaInvoke(clonedId, lambdaParameter, lambdaArgument);
                var clonedGetter = LambdaGetter.CloneForLambdaInvoke(clonedId, lambdaParameter, lambdaArgument);
                _cloned = clonedArgument == Argument && clonedGetter == LambdaGetter ? this :
                    new LambdaInvocation(clonedArgument, clonedGetter);
            }
            return _cloned;
        }
    }
}
