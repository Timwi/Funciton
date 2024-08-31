using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Funciton
{
    abstract class Node(FuncitonFunction thisFunction)
    {
        protected FuncitonFunction _thisFunction = thisFunction;
        protected Node _cloned;
        protected int _clonedId;

        /// <summary>Clones a function for the purpose of calling it.</summary>
        public abstract Node CloneForFunctionCall(int clonedId, Node[] functionInputs);

        /// <summary>Clones those parts of a function that depend on the specified <paramref name="lambdaParameter"/>.</summary>
        public abstract Node CloneForLambdaInvoke(int clonedId, LambdaExpressionParameterNode lambdaParameter, Node lambdaArgument);

        /// <summary>
        ///     This function is designed to evaluate an entire Funciton program without using .NET’s own call stack (so that
        ///     we are not limited to its size). See <see cref="NextToEvaluate(BigInteger)"/> for details.</summary>
        public Node NextToEvaluate(BigInteger previousSubresult, IEnumerable<string> traceFunctions)
        {
            var res = NextToEvaluate(previousSubresult);
            if (traceFunctions != null && traceFunctions.Contains(_thisFunction.Name))
                trace(res);
            else
                releaseMemory();
            return res;
        }

        /// <summary>
        ///     This function is designed to evaluate an entire Funciton program without using .NET’s own call stack (so that
        ///     we are not limited to its size). See remarks for details.</summary>
        /// <param name="previousSubresult">
        ///     The result of the previous node’s evaluation.</param>
        /// <returns>
        ///     A node to evaluate next, or null to indicate evaluation is complete. See remarks for details.</returns>
        /// <remarks>
        ///     <para>
        ///         The code contract is this:</para>
        ///     <list type="bullet">
        ///         <item><description>
        ///             The caller calls <see cref="NextToEvaluate"/>. The value of <paramref name="previousSubresult"/> is
        ///             immaterial.</description></item>
        ///         <item><description>
        ///             If <see cref="NextToEvaluate"/> returns <c>null</c>, the node is fully evaluated and the result can be
        ///             read from <see cref="Result"/>.</description></item>
        ///         <item><description>
        ///             If <see cref="NextToEvaluate"/> returns a node, the caller is expected to recursively evaluate that
        ///             node, read its result, and then call <see cref="NextToEvaluate"/> again, this time passing the result
        ///             into <paramref name="previousSubresult"/>.</description></item></list></remarks>
        public abstract Node NextToEvaluate(BigInteger previousSubresult);

        private void trace(Node res)
        {
            // Only output a trace if this node is fully evaluated
            if (res != null)
                return;

            // Don’t bother showing extra trace lines for literals
            if (this is LiteralNode)
                return;

            // Only output a trace if we haven’t already done so for this node
            if (!_alreadyTraced.Add(this))
                return;

            // See if the result happens to be a valid string
            string str = null;
            try
            {
                var istr = FuncitonLanguage.IntegerToString(_result);
                if (istr != null)
                    str = $@"""{istr.CLiteralEscape()}""";
            }
            catch { }

            // See if the result happens to be a valid list
            string list = null;
            try
            {
                if (_result < 0)
                    goto notAValidList;
                var intList = new List<BigInteger>();
                var result = _result;
                var mask = ~(BigInteger.MinusOne << 22);
                while (result > 0)
                {
                    var curItem = BigInteger.Zero;
                    var signBit = (result & 1) != 0;
                    var itemBit = 0;
                    result >>= 1;
                    while ((result & 1) == 0)
                    {
                        curItem |= ((result & mask) >> 1) << itemBit;
                        itemBit += 21;
                        result >>= 22;
                        if (result == 0)
                            goto notAValidList;
                    }
                    result >>= 1;
                    intList.Add(signBit ? ~curItem : curItem);
                }
                list = $"[{string.Join(", ", intList)}]";
                notAValidList:;
            }
            catch { }

            ConsoleWriteLineColored(
                ConsoleColor.White, _thisFunction.Name, ": ",
                ConsoleColor.Gray, getExpression(null, false, true) + " ",
                ConsoleColor.White, "= ",
                ConsoleColor.Green, _result.ToString(),
                str == null ? null : new object[] { ConsoleColor.White, " = ", ConsoleColor.DarkCyan, str },
                list == null ? null : new object[] { ConsoleColor.White, " = ", ConsoleColor.DarkMagenta, list });
        }

        private void ConsoleWriteLineColored(params object[] objs)
        {
            ConsoleWriteColored(objs);
            Console.WriteLine();
            Console.ResetColor();
        }

        private void ConsoleWriteColored(params object[] objs)
        {
            foreach (var item in objs)
                if (item is ConsoleColor color)
                    Console.ForegroundColor = color;
                else if (item is object[] array)
                    ConsoleWriteColored(array);
                else if (item != null)
                    Console.Write(item);
        }

        // This is a static field rather than a boolean instance field because an instance field would make
        // every Node instance larger and thus use significantly more memory even when not tracing.
        private static readonly HashSet<Node> _alreadyTraced = [];

        protected abstract void releaseMemory();

        protected BigInteger _result;

        /// <summary>
        ///     See the remarks on <see cref="NextToEvaluate"/> for details. Until <see cref="NextToEvaluate"/> has returned
        ///     null, this value is meaningless. Afterwards, it contains the result of evaluating this code.</summary>
        public BigInteger Result => _result;

        public virtual void FindNodes(FindNodesResult fnr)
        {
            fnr.AllNodes.Add(this);

            if (fnr.MultiUseNodes.Contains(this))
                return;
            if (fnr.SingleUseNodes.Contains(this))
            {
                fnr.SingleUseNodes.Remove(this);
                fnr.MultiUseNodes.Add(this);
                fnr.LetNodes.Add(this);
                return;
            }
            fnr.SingleUseNodes.Add(this);
            findChildNodes(fnr);
        }

        protected abstract void findChildNodes(FindNodesResult fnr);

        public string GetExpression(Node[] letNodes, bool alwaysExpand, bool requireParentheses, bool requireOutputArrow)
        {
            int pos;
            if (letNodes != null && !alwaysExpand && (pos = Array.IndexOf(letNodes, this)) != -1)
                return ((char) ('a' + pos)).ToString();
            return getExpression(letNodes, requireParentheses, requireOutputArrow);
        }

        protected abstract string getExpression(Node[] letNodes, bool requireParentheses, bool requireOutputArrow);
    }
}
