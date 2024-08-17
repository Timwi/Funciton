using System.Linq;

namespace Funciton
{
    sealed class SourceAsChars(char[][] chars, string sourceFile)
    {
        public char[][] Chars { get; private set; } = chars;
        public string SourceFile { get; private set; } = sourceFile;

        public LineType TopLine(int x, int y) => y < 0 || y >= Chars.Length || x < 0 || x >= Chars[y].Length ? LineType.None :
            "│└┘├┤┴╛╘╡╧┼╞╪".Contains(Chars[y][x]) ? LineType.Single :
            "║╚╝╠╣╩╜╙╢╨╬╟╫".Contains(Chars[y][x]) ? LineType.Double : LineType.None;
        public LineType LeftLine(int x, int y) => y < 0 || y >= Chars.Length || x < 0 || x >= Chars[y].Length ? LineType.None :
            "─┐┘┤┬┴╜╖╢╨╥╫┼".Contains(Chars[y][x]) ? LineType.Single :
            "═╗╝╣╦╩╛╕╡╧╤╪╬".Contains(Chars[y][x]) ? LineType.Double : LineType.None;
        public LineType RightLine(int x, int y) => y < 0 || y >= Chars.Length || x < 0 || x >= Chars[y].Length ? LineType.None :
            "─└┌├┬┴╓╙╨╟╥╫┼".Contains(Chars[y][x]) ? LineType.Single :
            "═╚╔╠╦╩╒╘╧╞╤╪╬".Contains(Chars[y][x]) ? LineType.Double : LineType.None;
        public LineType BottomLine(int x, int y) => y < 0 || y >= Chars.Length || x < 0 || x >= Chars[y].Length ? LineType.None :
            "│┌┐├┤┬╒╕╡╞╤╪┼".Contains(Chars[y][x]) ? LineType.Single :
            "║╔╗╠╣╦╓╖╢╟╥╫╬".Contains(Chars[y][x]) ? LineType.Double : LineType.None;
        public bool AnyLine(int x, int y) => "─│┌┐└┘├┤┬┴┼═║╒╓╔╕╖╗╘╙╚╛╜╝╞╟╠╡╢╣╤╥╦╧╨╩╪╫╬".Contains(Chars[y][x]);
        public int Width => Chars[0].Length;
        public int Height => Chars.Length;

        private static string dir2str(Direction d, LineType lin) =>
            lin == LineType.Single ? (d == Direction.Up ? "↑" : d == Direction.Down ? "↓" : d == Direction.Left ? "←" : "→") :
            lin == LineType.Double ? (d == Direction.Up ? "⇑" : d == Direction.Down ? "⇓" : d == Direction.Left ? "⇐" : "⇒") : "";

        public string GetLineShape(int x, int y, Direction dir, int minX, int minY, int maxX, int maxY)
        {
            var lnType = dir == Direction.Up ? TopLine(x, y) : dir == Direction.Right ? RightLine(x, y) : dir == Direction.Down ? BottomLine(x, y) : LeftLine(x, y);
            string ret = dir2str(dir, lnType);
            while (true)
            {
                switch (dir)
                {
                    case Direction.Up: y--; break;
                    case Direction.Left: x--; break;
                    case Direction.Down: y++; break;
                    case Direction.Right: x++; break;
                }
                var arr = new[] { x > minX && x < maxX && y > minY ? TopLine(x, y) : LineType.None,
                                               x < maxX && y > minY && y < maxY ? RightLine(x, y) : LineType.None,
                                               x > minX && x < maxX && y < maxY ? BottomLine(x, y) : LineType.None,
                                               x > minX && y > minY && y < maxY ? LeftLine(x, y) : LineType.None };
                var count = arr.Count(l => l != LineType.None);
                if (count == 1)
                    return ret;
                if (count != 2)
                    return null;
                dir = dir != Direction.Down && arr[0] != LineType.None ? Direction.Up :
                        dir != Direction.Left && arr[1] != LineType.None ? Direction.Right :
                        dir != Direction.Up && arr[2] != LineType.None ? Direction.Down :
                        dir != Direction.Right && arr[3] != LineType.None ? Direction.Left :
                        throw new ParseErrorException(new ParseError("The parser encountered an internal error.", x, y, SourceFile));
                ret += dir2str(dir, arr[(int) dir]);
            }
        }
    }
}
