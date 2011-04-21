using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace FuncitonInterpreter
{
    class FuncitonFunction
    {
        public abstract class Node
        {
            protected FuncitonFunction _thisFunction;
            public Node(FuncitonFunction thisFunction) { _thisFunction = thisFunction; }

            public Node Clone(Dictionary<object, object> cloned, Node[] functionInputs)
            {
                object node;
                if (cloned.TryGetValue(this, out node))
                    return (Node) node;
                return cloneImpl(cloned, functionInputs);
            }
            protected abstract Node cloneImpl(Dictionary<object, object> cloned, Node[] functionInputs);

            /// <summary>This function is designed to evaluate an entire Funciton program without using .NET’s own call stack (so that we are not limited to its size). See remarks for details.</summary>
            /// <returns>A node to evaluate next, or null to indicate evaluation is complete. See remarks for details.</returns>
            /// <remarks>
            /// <para>The code contract is this:</para>
            /// <list type="bullet">
            /// <item><description>The caller calls <see cref="NextToEvaluate"/>.</description></item>
            /// <item><description>If <see cref="NextToEvaluate"/> returns null, node is fully evaluated and the result can be read from <see cref="Result"/>.</description></item>
            /// <item><description>If <see cref="NextToEvaluate"/> returns a node, the caller is expected to fully evaluate that node,
            /// read its result, store that in <see cref="PreviousSubresult"/> and then call <see cref="NextToEvaluate"/> again.</description></item>
            /// </list>
            /// </remarks>
            public abstract Node NextToEvaluate();

            protected BigInteger _result;
            protected BigInteger _previousSubresult;

            /// <summary>See the remarks on <see cref="NextToEvaluate"/> for details. Until <see cref="NextToEvaluate"/> has returned null, this value is meaningless. Afterwards, it contains the result of evaluating this code.</summary>
            public BigInteger Result { get { return _result; } }

            /// <summary>See the remarks on <see cref="NextToEvaluate"/> for details. Write the result of a previous evaluation here. The previous subresult must be written before the next call to <see cref="NextToEvaluate"/> is made.</summary>
            public BigInteger PreviousSubresult { set { _previousSubresult = value; } }
        }

        public sealed class CallNode
        {
            public FuncitonFunction Function;
            public Node[] Inputs;
            private Node[] _clonedFunctionOutputs;
            public Node[] ClonedFunctionOutputs { get { return _clonedFunctionOutputs ?? (_clonedFunctionOutputs = Function.CloneOutputNodes(Inputs)); } }

            public CallNode Clone(Dictionary<object, object> cloned, Node[] functionInputs)
            {
                object callNode;
                if (cloned.TryGetValue(this, out callNode))
                    return (CallNode) callNode;
                var newNode = new CallNode { Function = Function };
                cloned[this] = newNode;
                newNode.Inputs = Inputs.Select(inp => inp == null ? null : inp.Clone(cloned, functionInputs)).ToArray();
                return newNode;
            }
        }

        public sealed class CallOutputNode : Node
        {
            public CallNode CallNode;
            public int OutputPosition;
            public CallOutputNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            protected override Node cloneImpl(Dictionary<object, object> cloned, Node[] functionInputs)
            {
                var newNode = new CallOutputNode(_thisFunction) { OutputPosition = OutputPosition };
                cloned[this] = newNode;
                newNode.CallNode = CallNode.Clone(cloned, functionInputs);
                return newNode;
            }

            private int _state = 0;
            public override Node NextToEvaluate()
            {
                switch (_state)
                {
                    case 0:
                        _state = 1;
                        return CallNode.ClonedFunctionOutputs[OutputPosition];
                    case 1:
                        _result = _previousSubresult;
                        _state = 2;
                        return null;
                    default: // = 2
                        return null;
                }
            }
        }

        public sealed class NandNode : Node
        {
            public Node Left, Right;
            public NandNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            protected override Node cloneImpl(Dictionary<object, object> cloned, Node[] functionInputs)
            {
                var newNode = new NandNode(_thisFunction);
                cloned[this] = newNode;
                newNode.Left = Left.Clone(cloned, functionInputs);
                newNode.Right = Right.Clone(cloned, functionInputs);
                return newNode;
            }

            private int _state = 0;
            private BigInteger _leftEval;
            public override Node NextToEvaluate()
            {
                switch (_state)
                {
                    case 0:
                        _state = 1;
                        return Left;
                    case 1:
                        if (_previousSubresult.IsZero)
                        {
                            _result = BigInteger.MinusOne;
                            _state = 3;
                            return null;
                        }
                        _leftEval = _previousSubresult;
                        _state = 2;
                        return Right;
                    case 2:
                        _result = ~(_leftEval & _previousSubresult);
                        _state = 3;
                        return null;
                    default: // = 3
                        return null;
                }
            }
        }

        public abstract class CrossWireNode : Node
        {
            public Node Left, Right;
            public CrossWireNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            protected override Node cloneImpl(Dictionary<object, object> cloned, Node[] functionInputs)
            {
                var newNode = createNew();
                cloned[this] = newNode;
                newNode.Left = Left.Clone(cloned, functionInputs);
                newNode.Right = Right.Clone(cloned, functionInputs);
                return newNode;
            }
            protected abstract CrossWireNode createNew();
        }

        public sealed class LessThanNode : CrossWireNode
        {
            public LessThanNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            protected override CrossWireNode createNew() { return new LessThanNode(_thisFunction); }
            private int _state = 0;
            private BigInteger _leftEval;
            public override Node NextToEvaluate()
            {
                switch (_state)
                {
                    case 0:
                        _state = 1;
                        return Left;
                    case 1:
                        _leftEval = _previousSubresult;
                        _state = 2;
                        return Right;
                    case 2:
                        _result = _leftEval < _previousSubresult ? BigInteger.MinusOne : BigInteger.Zero;
                        _state = 3;
                        return null;
                    default: // = 3
                        return null;
                }
            }
        }

        public sealed class ShiftLeftNode : CrossWireNode
        {
            public ShiftLeftNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            protected override CrossWireNode createNew() { return new ShiftLeftNode(_thisFunction); }
            private int _state = 0;
            private BigInteger _leftEval;
            public override Node NextToEvaluate()
            {
                switch (_state)
                {
                    case 0:
                        _state = 1;
                        return Left;
                    case 1:
                        _leftEval = _previousSubresult;
                        _state = 2;
                        return Right;
                    case 2:
                        _result = _previousSubresult.IsZero ? _leftEval : _previousSubresult > 0 ? _leftEval << (int) _previousSubresult : _leftEval >> (int) -_previousSubresult;
                        _state = 3;
                        return null;
                    default: // = 3
                        return null;
                }
            }
        }

        public sealed class InputNode : Node
        {
            public int InputPosition;
            public InputNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            private Node[] _functionInputs;
            protected override Node cloneImpl(Dictionary<object, object> cloned, Node[] functionInputs)
            {
                var newNode = new InputNode(_thisFunction) { InputPosition = InputPosition, _functionInputs = functionInputs };
                cloned[this] = newNode;
                return newNode;
            }

            private int _state = 0;
            public override Node NextToEvaluate()
            {
                switch (_state)
                {
                    case 0:
                        _state = 1;
                        return _functionInputs[InputPosition];
                    case 1:
                        _result = _previousSubresult;
                        _state = 2;
                        return null;
                    default: // = 2
                        return null;
                }
            }
        }

        public sealed class LiteralNode : Node
        {
            public LiteralNode(FuncitonFunction thisFunction, BigInteger literal) : base(thisFunction) { _result = literal; }
            protected override Node cloneImpl(Dictionary<object, object> cloned, Node[] functionInputs)
            {
                var newNode = new LiteralNode(_thisFunction, _result);
                cloned[this] = newNode;
                return newNode;
            }
            public override Node NextToEvaluate() { return null; }
        }

        public string Name;

        protected Node[] _outputNodes;
        public FuncitonFunction(Node[] outputNodes) { _outputNodes = outputNodes; }

        public Node[] CloneOutputNodes(Node[] functionInputs)
        {
            var cloned = new Dictionary<object, object>();
            var outputNodes = new Node[_outputNodes.Length];
            for (int i = 0; i < _outputNodes.Length; i++)
                outputNodes[i] = _outputNodes[i] == null ? null : _outputNodes[i].Clone(cloned, functionInputs);
            return outputNodes;
        }
    }

    sealed class FuncitonProgram : FuncitonFunction
    {
        public FuncitonProgram(Node[] outputNodes) : base(outputNodes) { }

        public BigInteger Run()
        {
            // A larger initial capacity than this does not improve performance
            var evaluationStack = new Stack<Node>(1024);

            // Should have only one output
            var currentNode = _outputNodes.Single(o => o != null);

            while (true)
            {
                var next = currentNode.NextToEvaluate();

                // small performance optimisation (saves a push and a pop for every literal)
                while (next is LiteralNode)
                {
                    currentNode.PreviousSubresult = next.Result;
                    next = currentNode.NextToEvaluate();
                }

                if (next != null)
                {
                    evaluationStack.Push(currentNode);
                    currentNode = next;
                }
                else
                {
                    if (evaluationStack.Count == 0)
                        return currentNode.Result;
                    var lastResult = currentNode.Result;
                    currentNode = evaluationStack.Pop();
                    currentNode.PreviousSubresult = lastResult;
                }
            }
        }
    }
}
