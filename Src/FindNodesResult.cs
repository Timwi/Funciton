using System.Collections.Generic;

namespace Funciton
{
    sealed class FindNodesResult
    {
        public HashSet<FuncitonFunction.Node> SingleUseNodes = new HashSet<FuncitonFunction.Node>();
        public HashSet<FuncitonFunction.Node> MultiUseNodes = new HashSet<FuncitonFunction.Node>();
        public HashSet<FuncitonFunction.Node> NodesUsedAsFunctionInputs = new HashSet<FuncitonFunction.Node>();
        public HashSet<FuncitonFunction.Node> LetNodes = new HashSet<FuncitonFunction.Node>();
        public HashSet<FuncitonFunction.Node> AllNodes = new HashSet<FuncitonFunction.Node>();
        public HashSet<FuncitonFunction.Call> Calls = new HashSet<FuncitonFunction.Call>();
        public HashSet<FuncitonFunction.LambdaInvocation> Invocations = new HashSet<FuncitonFunction.LambdaInvocation>();
    }
}
