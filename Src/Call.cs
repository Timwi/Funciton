using System.Linq;

namespace Funciton
{
    sealed class Call(FuncitonFunction function, Node[] inputs)
    {
        public FuncitonFunction Function { get; private set; } = function;
        public Node[] Inputs { get; private set; } = inputs;
        private Call _cloned;
        private int _clonedId;

        private Node[] _clonedFunctionOutputs;
        public Node[] ClonedFunctionOutputs => _clonedFunctionOutputs ??= Function.CloneOutputNodes(Inputs);

        public Call CloneForFunctionCall(int clonedId, Node[] functionInputs)
        {
            if (_clonedId != clonedId)
            {
                _clonedId = clonedId;
                _cloned = new Call(Function, Inputs.Select(inp => inp?.CloneForFunctionCall(clonedId, functionInputs)).ToArray());
            }
            return _cloned;
        }

        public Call CloneForLambdaInvoke(int clonedId, LambdaExpressionParameterNode lambdaParameter, Node lambdaArgument)
        {
            if (_clonedId != clonedId)
            {
                _clonedId = clonedId;
                var clonedInputs = Inputs.Select(inp => inp?.CloneForLambdaInvoke(clonedId, lambdaParameter, lambdaArgument)).ToArray();
                _cloned = clonedInputs.SequenceEqual(Inputs) ? this : new Call(Function, clonedInputs);
            }
            return _cloned;
        }
    }
}
