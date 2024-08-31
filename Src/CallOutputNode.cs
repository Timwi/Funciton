using System;
using System.Linq;
using System.Numerics;

namespace Funciton
{
    sealed class CallOutputNode(FuncitonFunction thisFunction, int outputPosition, Call call) : Node(thisFunction)
    {
        public int OutputPosition { get; private set; } = outputPosition;
        public Call Call { get; private set; } = call ?? throw new ArgumentNullException("call");

        public override Node CloneForFunctionCall(int clonedId, Node[] functionInputs)
        {
            if (_clonedId != clonedId)
            {
                _clonedId = clonedId;
                _cloned = new CallOutputNode(_thisFunction, OutputPosition, Call.CloneForFunctionCall(clonedId, functionInputs));
            }
            return _cloned;
        }

        public override bool IsEvaluated => _state == 2;

        public override Node CloneForLambdaInvoke(int clonedId, LambdaExpressionParameterNode lambdaParameter, Node lambdaArgument)
        {
            if (_state == 2)    // fully evaluated
                return this;
            if (_clonedId != clonedId)
            {
                _clonedId = clonedId;
                var clonedCall = Call.CloneForLambdaInvoke(clonedId, lambdaParameter, lambdaArgument);
                _cloned = clonedCall == Call ? this : new CallOutputNode(_thisFunction, OutputPosition, clonedCall);
            }
            return _cloned;
        }

        private int _state = 0;
        public override Node NextToEvaluate(BigInteger previousSubresult)
        {
            switch (_state)
            {
                case 0:
                    _state = 1;
                    return Call.ClonedFunctionOutputs[OutputPosition];
                case 1:
                    _result = previousSubresult;
                    _state = 2;
                    return null;
                default: // = 2
                    return null;
            }
        }

        protected override void releaseMemory()
        {
            if (_state == 1)
                Call = null;
        }

        protected override void findChildNodes(FindNodesResult fnr)
        {
            if (Call.Function.OutputNodes.Count(n => n != null) > 1)
                fnr.LetNodes.Add(this);
            if (fnr.Calls.Add(Call))
            {
                foreach (var inp in Call.Inputs.Where(i => i != null))
                {
                    fnr.NodesUsedAsFunctionInputs.Add(inp);
                    inp.FindNodes(fnr);
                }
            }
        }

        protected override string getExpression(Node[] letNodes, bool requireParentheses, bool requireOutputArrow)
        {
            var open = requireParentheses ? "(" : "";
            var close = requireParentheses ? ")" : "";

            // Detect single-parameter, single-output functions (e.g. “♯”)
            var inputIndexes = Call.Inputs.Select((node, i) => node == null ? -1 : i).Where(i => i != -1).ToArray();
            var outputIndexes = Call.Function.OutputNodes.Select((node, i) => node == null ? -1 : i).Where(i => i != -1).ToArray();
            if (inputIndexes.Length == 1 && outputIndexes.Length == 1)
                return Call.Function.Name + "(" + Call.Inputs.Select((inp, ind) => inp?.GetExpression(letNodes, false, false, requireOutputArrow)).First(str => str != null) + ")";

            // Detect two-opposite-parameter, single-perpendicular-output functions (normally binary operators, e.g. “<”)
            var config = string.Join("", outputIndexes) + "/" + string.Join("", inputIndexes);
            if (config == "0/13" || config == "3/02")
                return open + Call.Inputs[inputIndexes[1]].GetExpression(letNodes, false, false, requireOutputArrow) + " " + Call.Function.Name + " " + Call.Inputs[inputIndexes[0]].GetExpression(letNodes, false, true, requireOutputArrow) + close;
            else if (config == "1/02" || config == "2/13")
                return open + Call.Inputs[inputIndexes[0]].GetExpression(letNodes, false, false, requireOutputArrow) + " " + Call.Function.Name + " " + Call.Inputs[inputIndexes[1]].GetExpression(letNodes, false, true, requireOutputArrow) + close;

            // Fall back to verbose notation
            return $"{Call.Function.Name}({string.Join(", ", Call.Inputs.Select((inp, ind) => inp?.GetExpression(letNodes, false, false, requireOutputArrow)).Where(str => str != null).Reverse())}){(requireOutputArrow && outputIndexes.Length > 1 ? "[" + "↓←↑→"[OutputPosition] + "]" : "")}";
        }
    }
}
