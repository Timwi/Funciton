using System.Numerics;

namespace Funciton
{
    sealed class ShiftLeftNode(FuncitonFunction thisFunction, Node left, Node right) : CrossWireNode(thisFunction, left, right)
    {
        protected override CrossWireNode createNew(Node left, Node right) => new ShiftLeftNode(_thisFunction, left, right);
        protected override string _operator => " SHL ";
        protected override BigInteger getResult(BigInteger left, BigInteger right) => right.IsZero ? left : right > 0 ? left << (int) right : left >> (int) -right;
    }
}
