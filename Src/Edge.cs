namespace Funciton
{
    /// <summary>Represents an edge (line) connecting two <see cref="UnparsedNode"/> objects in the source.</summary>
    sealed class Edge(UnparsedNode start, Direction directionFromStart, UnparsedNode end, Direction directionFromEnd, int startX, int startY, int endX, int endY)
    {
        public UnparsedNode StartNode { get; private set; } = start;
        public Direction DirectionFromStartNode { get; private set; } = directionFromStart;
        public UnparsedNode EndNode { get; private set; } = end;
        public Direction DirectionFromEndNode { get; private set; } = directionFromEnd;
        public int StartX { get; private set; } = startX;
        public int StartY { get; private set; } = startY;
        public int EndX { get; private set; } = endX;
        public int EndY { get; private set; } = endY;

        public override string ToString() => $"[{StartNode}] {DirectionFromStartNode} → [{EndNode}] {DirectionFromEndNode}";
        public void Flip()
        {
            (EndNode, StartNode) = (StartNode, EndNode);
            (DirectionFromEndNode, DirectionFromStartNode) = (DirectionFromStartNode, DirectionFromEndNode);
            (EndX, StartX) = (StartX, EndX);
            (EndY, StartY) = (StartY, EndY);
        }
    }
}
