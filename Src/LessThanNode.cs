using System.Numerics;

namespace Funciton
{
    sealed class LessThanNode(FuncitonFunction thisFunction, Node left, Node right) : CrossWireNode(thisFunction, left, right)
    {
        protected override CrossWireNode createNew(Node left, Node right) => new LessThanNode(_thisFunction, left, right);
        protected override string _operator => " < ";
        protected override BigInteger getResult(BigInteger left, BigInteger right) => left < right ? BigInteger.MinusOne : BigInteger.Zero;
    }
}
