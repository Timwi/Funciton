using System.Collections.Generic;
using System.Linq;

namespace Funciton
{
    sealed class FuncitonProgram : FuncitonFunction
    {
        public FuncitonProgram(Node[] outputNodes) : base(outputNodes, "") { }

        public string Run(List<string> traceFunctions)
        {
            // A larger initial capacity than this does not improve performance
            var evaluationStack = new Stack<Node>(1024);

            // Should have only one output
            var currentNode = OutputNodes.Single(o => o != null);

            while (true)
            {
                var next = currentNode.NextToEvaluate(traceFunctions);

                // small performance optimization (saves a push and a pop for every literal)
                while (next is LiteralNode)
                {
                    currentNode.PreviousSubresult = next.Result;
                    next = currentNode.NextToEvaluate(traceFunctions);
                }

                if (next != null)
                {
                    evaluationStack.Push(currentNode);
                    currentNode = next;
                }
                else
                {
                    if (evaluationStack.Count == 0)
                        return FuncitonLanguage.IntegerToString(currentNode.Result);
                    var lastResult = currentNode.Result;
                    currentNode = evaluationStack.Pop();
                    currentNode.PreviousSubresult = lastResult;
                }
            }
        }
    }
}
