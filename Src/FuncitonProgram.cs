using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Funciton
{
    sealed class FuncitonProgram(Node[] outputNodes) : FuncitonFunction(outputNodes, "")
    {
        public string Run(List<string> traceFunctions)
        {
            // A larger initial capacity than this does not improve performance
            var evaluationStack = new Stack<Node>(1024);

            // Should have only one output
            var currentNode = OutputNodes.Single(o => o != null);

            var previousSubresult = BigInteger.Zero;

            while (true)
            {
                var next = currentNode.NextToEvaluate(previousSubresult, traceFunctions);

                // small performance optimization (saves a push and a pop for every literal)
                while (next is LiteralNode)
                    next = currentNode.NextToEvaluate(next.Result, traceFunctions);

                if (next != null)
                {
                    evaluationStack.Push(currentNode);
                    previousSubresult = BigInteger.Zero;
                    currentNode = next;
                }
                else if (evaluationStack.Count != 0)
                {
                    previousSubresult = currentNode.Result;
                    currentNode = evaluationStack.Pop();
                }
                else
                    return FuncitonLanguage.IntegerToString(currentNode.Result);
            }
        }
    }
}
