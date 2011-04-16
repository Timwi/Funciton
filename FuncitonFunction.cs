using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace FuncitonInterpreter
{
    class FuncitonFunction
    {
        protected static int _debug_log_indent = 0;
        //protected bool _debug_log_enabled { get { return Name == "÷%"; } }
        protected bool _debug_log_enabled { get { return false; } }
        protected void _debug_log(string msg)
        {
            if (_debug_log_enabled)
                Console.WriteLine(new string(' ', _debug_log_indent * 2) + msg);
        }

        public abstract class Node
        {
            private BigInteger _value;
            private bool _evaluated = false;
            protected FuncitonFunction _thisFunction;
            public Node(FuncitonFunction thisFunction) { _thisFunction = thisFunction; }

            public Node Clone(Dictionary<object, object> cloned)
            {
                object node;
                if (cloned.TryGetValue(this, out node))
                    return (Node) node;
                return cloneImpl(cloned);
            }
            protected abstract Node cloneImpl(Dictionary<object, object> cloned);

            public BigInteger Evaluate(Func<BigInteger>[] functionInputs)
            {
                if (!_evaluated)
                {
                    if (_thisFunction._debug_log_enabled)
                        _debug_log_indent++;
                    _value = evaluateImpl(functionInputs);
                    if (_thisFunction._debug_log_enabled)
                        _debug_log_indent--;
                    _evaluated = true;
                }
                return _value;
            }
            protected abstract BigInteger evaluateImpl(Func<BigInteger>[] functionInputs);
        }

        public sealed class CallNode
        {
            public FuncitonFunction Function;
            public Node[] Inputs;

            private Func<BigInteger>[] _evaluator;
            public Func<BigInteger>[] GetEvaluator(Func<BigInteger>[] functionInputs)
            {
                return _evaluator ?? (_evaluator = Function.MakeEvaluator(Inputs.Select(ie => ie == null ? (Func<BigInteger>) null : () => ie.Evaluate(functionInputs)).ToArray()));
            }

            public CallNode Clone(Dictionary<object, object> cloned)
            {
                object callNode;
                if (cloned.TryGetValue(this, out callNode))
                    return (CallNode) callNode;
                var newNode = new CallNode { Function = Function };
                cloned[this] = newNode;
                newNode.Inputs = Inputs.Select(ie => ie == null ? null : ie.Clone(cloned)).ToArray();
                return newNode;
            }
        }

        public sealed class CallOutputNode : Node
        {
            public CallNode CallNode;
            public int OutputPosition;
            public CallOutputNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            protected override BigInteger evaluateImpl(Func<BigInteger>[] functionInputs)
            {
                _thisFunction._debug_log("call: " + CallNode.Function.Name);
                return CallNode.GetEvaluator(functionInputs)[OutputPosition]();
            }
            protected override Node cloneImpl(Dictionary<object, object> cloned)
            {
                var newNode = new CallOutputNode(_thisFunction) { OutputPosition = OutputPosition };
                cloned[this] = newNode;
                newNode.CallNode = CallNode.Clone(cloned);
                return newNode;
            }
        }

        public sealed class NandNode : Node
        {
            public Node Left, Right;
            public NandNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            protected override Node cloneImpl(Dictionary<object, object> cloned)
            {
                var newNode = new NandNode(_thisFunction);
                cloned[this] = newNode;
                newNode.Left = Left.Clone(cloned);
                newNode.Right = Right.Clone(cloned);
                return newNode;
            }
            protected override BigInteger evaluateImpl(Func<BigInteger>[] functionInputs)
            {
                _thisFunction._debug_log("nand");
                var val = Left.Evaluate(functionInputs);
                // Important short-circuit evaluation
                if (val.IsZero)
                    return BigInteger.MinusOne;
                var val2 = Right.Evaluate(functionInputs);
                return ~(val & val2);
            }
        }

        public abstract class CrossWireNode : Node
        {
            public Node Left, Right;
            public CrossWireNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            protected override Node cloneImpl(Dictionary<object, object> cloned)
            {
                var newNode = createNew();
                cloned[this] = newNode;
                newNode.Left = Left.Clone(cloned);
                newNode.Right = Right.Clone(cloned);
                return newNode;
            }
            protected abstract CrossWireNode createNew();
        }

        public sealed class LessThanNode : CrossWireNode
        {
            public LessThanNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            protected override CrossWireNode createNew() { return new LessThanNode(_thisFunction); }
            protected override BigInteger evaluateImpl(Func<BigInteger>[] functionInputs)
            {
                _thisFunction._debug_log("less-than");
                var val1 = Left.Evaluate(functionInputs);
                var val2 = Right.Evaluate(functionInputs);
                return val1 < val2 ? BigInteger.MinusOne : BigInteger.Zero;
            }
        }

        public sealed class ShiftLeftNode : CrossWireNode
        {
            public ShiftLeftNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            protected override CrossWireNode createNew() { return new ShiftLeftNode(_thisFunction); }
            protected override BigInteger evaluateImpl(Func<BigInteger>[] functionInputs)
            {
                _thisFunction._debug_log("shift-left");
                var left = Left.Evaluate(functionInputs);
                var right = Right.Evaluate(functionInputs);
                return right.IsZero ? left : right > 0 ? left << (int) right : left >> (int) -right;
            }
        }

        public sealed class InputNode : Node
        {
            public int InputPosition;
            public InputNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            protected override Node cloneImpl(Dictionary<object, object> cloned)
            {
                var newNode = new InputNode(_thisFunction) { InputPosition = InputPosition };
                cloned[this] = newNode;
                return newNode;
            }
            protected override BigInteger evaluateImpl(Func<BigInteger>[] functionInputs)
            {
                _thisFunction._debug_log("function input " + InputPosition);
                var result = functionInputs[InputPosition]();
                _thisFunction._debug_log("result = " + result);
                return result;
            }
        }

        public sealed class LiteralNode : Node
        {
            public BigInteger Literal;
            public LiteralNode(FuncitonFunction thisFunction) : base(thisFunction) { }
            protected override Node cloneImpl(Dictionary<object, object> cloned)
            {
                var newNode = new LiteralNode(_thisFunction) { Literal = Literal };
                cloned[this] = newNode;
                return newNode;
            }
            protected override BigInteger evaluateImpl(Func<BigInteger>[] functionInputs)
            {
                _thisFunction._debug_log("literal: " + Literal);
                return Literal;
            }
        }

        public string Name;
        protected Node[] _outputNodes;
        public FuncitonFunction(Node[] outputNodes) { _outputNodes = outputNodes; }

        public Func<BigInteger>[] MakeEvaluator(Func<BigInteger>[] functionInputs)
        {
            var cloned = new Dictionary<object, object>();
            return _outputNodes.Select(n => n == null ? null : n.Clone(cloned))
                                           .Select(n => n == null ? (Func<BigInteger>) null : () => n.Evaluate(functionInputs))
                                           .ToArray();
        }
    }

    sealed class FuncitonProgram : FuncitonFunction
    {
        public FuncitonProgram(Node[] outputNodes) : base(outputNodes) { }

        public BigInteger Run()
        {
            // Should have only one output
            var outputNode = _outputNodes.Single(o => o != null);
            return outputNode.Evaluate(new Func<BigInteger>[4]);
        }
    }
}
