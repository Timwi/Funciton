namespace Funciton
{
    /// <summary>
    ///     Represents an edge (line) coming out of an <see cref="UnparsedNode"/> before the parser has determined where
    ///     it connects to. Once the parser has determined that, see <see cref="Edge"/>.</summary>
    sealed class UnfinishedEdge
    {
        public UnparsedNode StartNode;
        public Direction DirectionFromStartNode;
        public int StartX, StartY, EndX, EndY;
        public Direction DirectionGoingTo;
        public override string ToString() => $"[{StartNode}] ({DirectionFromStartNode}) → [{EndX}, {EndY}] ({DirectionGoingTo})";
    }
}
