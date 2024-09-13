using System.Numerics;

namespace Funciton
{
    sealed class NandNode(FuncitonFunction thisFunction, Node left, Node right) : BinaryOperatorNode(thisFunction, left, right)
    {
        public override BigInteger Calculate(BigInteger one, BigInteger two) => ~(one & two);
        protected override BigInteger? shortcircuit(BigInteger value) => value.IsZero ? BigInteger.MinusOne : null;
        public override BinaryOperatorNode Create(FuncitonFunction thisFunction, Node left, Node right) => new NandNode(thisFunction, left, right);
        protected override string operatorString => "@";
    }
}
