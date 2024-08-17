using System.Collections.Generic;

namespace Funciton
{
    struct FindNodesResult
    {
        public HashSet<Node> SingleUseNodes = [];
        public HashSet<Node> MultiUseNodes = [];
        public HashSet<Node> NodesUsedAsFunctionInputs = [];
        public HashSet<Node> LetNodes = [];
        public HashSet<Node> AllNodes = [];
        public HashSet<Call> Calls = [];
        public HashSet<LambdaInvocation> Invocations = [];

        public FindNodesResult()
        {
        }
    }
}
