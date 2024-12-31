namespace Funciton
{
    abstract class InputNode(FuncitonFunction thisFunction, int inputPosition) : Node(thisFunction)
    {
        public int InputPosition { get; private set; } = inputPosition;
    }
}
