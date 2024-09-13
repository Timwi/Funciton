using System.Numerics;

namespace Funciton
{
    sealed class AndNode(FuncitonFunction thisFunction, Node left, Node right) : BinaryOperatorNode(thisFunction, left, right)
    {
        public override BigInteger Calculate(BigInteger one, BigInteger two) => one & two;
        protected override BigInteger? shortcircuit(BigInteger value) => value.IsZero ? BigInteger.Zero : null;
        public override BinaryOperatorNode Create(FuncitonFunction thisFunction, Node left, Node right) => new AndNode(thisFunction, left, right);
        protected override string operatorString => "&";
    }
}
