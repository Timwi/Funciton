using System.Collections.Generic;

namespace Funciton
{
    struct FindNodesResult
    {
        public HashSet<FuncitonFunction.Node> SingleUseNodes = [];
        public HashSet<FuncitonFunction.Node> MultiUseNodes = [];
        public HashSet<FuncitonFunction.Node> NodesUsedAsFunctionInputs = [];
        public HashSet<FuncitonFunction.Node> LetNodes = [];
        public HashSet<FuncitonFunction.Node> AllNodes = [];
        public HashSet<FuncitonFunction.Call> Calls = [];
        public HashSet<FuncitonFunction.LambdaInvocation> Invocations = [];

        public FindNodesResult()
        {
        }
    }
}
