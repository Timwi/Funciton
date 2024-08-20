using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Funciton
{
    static partial class FuncitonLanguage
    {
        public static BigInteger? PretendStdin;

        // List containing all lambda closures ever created. They are identified in Funciton by their index in this list.
        // Add a null element at the front so that they start numbering at 1, so you can still use 0 in Funciton to mean null/false
        public static readonly List<LambdaClosure> LambdaClosures = [null];

        // Used by all the code that clones functions and lambda expressions as they are called/invoked
        public static int CloneCounter = 0;

        public static (FuncitonProgram program, string analysis, string functionNames) CompileFiles(IEnumerable<string> paths, bool getFunctionNames)
        {
            return compileAndAnalyze(paths, null, getFunctionNames);
        }

        public static string AnalyzeFunctions(IEnumerable<string> paths, List<string> functionsToAnalyze)
        {
            return compileAndAnalyze(paths, functionsToAnalyze, getFunctionNames: false).analysis;
        }

        private static (FuncitonProgram program, string analysis, string functionNames) compileAndAnalyze(IEnumerable<string> paths, List<string> functionNamesToAnalyze, bool getFunctionNames)
        {
            UnparsedProgram program = null;
            Dictionary<string, UnparsedDeclaration> functionsToAnalyze = [];
            var declarationsByCallNode = new Dictionary<UnparsedNode, UnparsedFunctionDeclaration>();
            var declarationsByName = new Dictionary<string, UnparsedFunctionDeclaration>();

            foreach (var sourceFile in paths)
            {
                var sourceText = File.ReadAllText(sourceFile);

                // Turn into array of characters
                var lines = sourceText.Replace("\r", "").Split('\n');
                if (lines.Length == 0)
                    continue;

                var source = new SourceAsChars(lines, sourceFile);

                // Find boxes and their outgoing edges
                var nodes = new List<UnparsedNode>();
                var unfinishedEdges = new List<UnfinishedEdge>();
                for (var y = 0; y < source.Chars.Length; y++)
                {
                    for (var x = 0; x < source.Chars[y].Length; x++)
                    {
                        // Start finding a box here if this is a top-left corner of a box
                        if (source.TopLine(x, y) != LineType.None || source.LeftLine(x, y) != LineType.None || source.RightLine(x, y) == LineType.None || source.BottomLine(x, y) == LineType.None)
                            continue;

                        // Find width of box by walking along top edge
                        var top = source.RightLine(x, y);
                        var index = x + 1;
                        while (index < source.Chars[y].Length && source.LeftLine(index, y) == top && source.RightLine(index, y) == top)
                            index++;
                        if (index == source.Chars[y].Length || source.LeftLine(index, y) != top || source.BottomLine(index, y) == LineType.None || source.TopLine(index, y) != LineType.None || source.RightLine(index, y) != LineType.None)
                            continue;
                        var width = index - x;

                        // Find height of box by walking along left edge
                        var left = source.BottomLine(x, y);
                        index = y + 1;
                        while (index < source.Chars.Length && source.TopLine(x, index) == left && source.BottomLine(x, index) == left)
                            index++;
                        if (index == source.Chars.Length || source.TopLine(x, index) != left || source.RightLine(x, index) == LineType.None || source.LeftLine(x, index) != LineType.None || source.BottomLine(x, index) != LineType.None)
                            continue;
                        var height = index - y;

                        // Verify the bottom edge
                        var bottom = source.RightLine(x, y + height);
                        index = x + 1;
                        while (index < source.Chars[y].Length && source.LeftLine(index, y + height) == bottom && source.RightLine(index, y + height) == bottom)
                            index++;
                        if (index == source.Chars[y].Length || source.LeftLine(index, y + height) != bottom || source.TopLine(index, y + height) == LineType.None || source.BottomLine(index, y + height) != LineType.None || source.RightLine(index, y + height) != LineType.None)
                            continue;
                        if (index - x != width)
                            continue;

                        // Verify the right edge
                        var right = source.BottomLine(x + width, y);
                        index = y + 1;
                        while (index < source.Chars.Length && source.TopLine(x + width, index) == right && source.BottomLine(x + width, index) == right)
                            index++;
                        if (index == source.Chars.Length || source.TopLine(x + width, index) != right || source.LeftLine(x + width, index) == LineType.None || source.RightLine(x + width, index) != LineType.None || source.BottomLine(x + width, index) != LineType.None)
                            continue;
                        if (index - y != height)
                            continue;

                        // Determine type of box
                        NodeType type;
                        var edgeTypes = new[] { left, top, right, bottom };
                        switch (edgeTypes.Count(e => e == LineType.Double))
                        {
                            case 0:
                                // Not actually a box but a NAND square
                                continue;

                            case 1:
                                type = NodeType.LambdaInvocation;
                                break;

                            case 2:
                                type = edgeTypes[0] != edgeTypes[1] && edgeTypes[1] != edgeTypes[2]
                                    ? NodeType.Declaration
                                    : NodeType.Call;
                                break;

                            case 3:
                                type = NodeType.LambdaExpression;
                                break;

                            case 4:
                                type = NodeType.Literal;
                                break;

                            default:
                                throw new ParseErrorException(new ParseError("Unrecognized box type.", x, y, sourceFile));
                        }

                        // Right now, “type” is “Literal” if it is a double-lined box, but it could be a Comment too,
                        // so don’t create the box yet. When we encounter an outgoing edge, we’ll know it’s a literal.
                        UnparsedNode box = null;
                        UnparsedNode getBox() => box ??= new UnparsedNode(x, y, width, height, type);

                        // Search for outgoing edges
                        UnfinishedEdge topEdge = null, rightEdge = null, bottomEdge = null, leftEdge = null;
                        for (int i = x + 1; i < x + width; i++)
                        {
                            if (source.TopLine(i, y) == LineType.Double)
                                throw new ParseErrorException(new ParseError("Box has outgoing double edge.", i, y, sourceFile));
                            else if (source.TopLine(i, y) == LineType.Single)
                            {
                                if (topEdge != null)
                                    throw new ParseErrorException(new ParseError("Box has duplicate outgoing edge along the top.", i, y, sourceFile));
                                topEdge = new UnfinishedEdge { StartNode = getBox(), DirectionFromStartNode = Direction.Up, DirectionGoingTo = Direction.Up, StartX = i, StartY = y, EndX = i, EndY = y };
                            }

                            if (source.BottomLine(i, y + height) == LineType.Double)
                                throw new ParseErrorException(new ParseError("Box has outgoing double edge.", i, y + height, sourceFile));
                            else if (source.BottomLine(i, y + height) == LineType.Single)
                            {
                                if (bottomEdge != null)
                                    throw new ParseErrorException(new ParseError("Box has duplicate outgoing edge along the bottom.", i, y + height, sourceFile));
                                bottomEdge = new UnfinishedEdge { StartNode = getBox(), DirectionFromStartNode = Direction.Down, DirectionGoingTo = Direction.Down, StartX = i, StartY = y + height, EndX = i, EndY = y + height };
                            }
                        }
                        for (int i = y + 1; i < y + height; i++)
                        {
                            if (source.LeftLine(x, i) == LineType.Double)
                                throw new ParseErrorException(new ParseError("Box has outgoing double edge.", x, i, sourceFile));
                            else if (source.LeftLine(x, i) == LineType.Single)
                            {
                                if (leftEdge != null)
                                    throw new ParseErrorException(new ParseError("Box has duplicate outgoing edge along the left.", x, i, sourceFile));
                                leftEdge = new UnfinishedEdge { StartNode = getBox(), DirectionFromStartNode = Direction.Left, DirectionGoingTo = Direction.Left, StartX = x, StartY = i, EndX = x, EndY = i };
                            }

                            if (source.RightLine(x + width, i) == LineType.Double)
                                throw new ParseErrorException(new ParseError("Box has outgoing double edge.", x + width, i, sourceFile));
                            else if (source.RightLine(x + width, i) == LineType.Single)
                            {
                                if (rightEdge != null)
                                    throw new ParseErrorException(new ParseError("Box has duplicate outgoing edge along the right.", x + width, i, sourceFile));
                                rightEdge = new UnfinishedEdge { StartNode = getBox(), DirectionFromStartNode = Direction.Right, DirectionGoingTo = Direction.Right, StartX = x + width, StartY = i, EndX = x + width, EndY = i };
                            }
                        }

                        // If box is still null, then it has no outgoing edges.
                        if (box == null)
                        {
                            if (type == NodeType.Literal)
                                type = NodeType.Comment;
                            else
                                throw new ParseErrorException(new ParseError("Box without outgoing edges not allowed unless it has only double-lined edges (making it a comment).", x, y, sourceFile));
                        }

                        // If it’s a comment, kill its contents so that it can contain boxes if it wants to.
                        if (type == NodeType.Comment)
                        {
                            for (int yy = y; yy <= y + height; yy++)
                                for (int xx = x; xx <= x + width; xx++)
                                    source.Chars[yy][xx] = ' ';
                        }
                        else
                        {
                            nodes.Add(getBox());
                            unfinishedEdges.AddRange(new[] { topEdge, rightEdge, bottomEdge, leftEdge }.Where(e => e != null));
                        }
                    }
                }

                // Add T-junctions and cross-junctions (but not loose ends yet), and also complain about any stray characters
                for (int y = 0; y < source.Chars.Length; y++)
                {
                    for (int x = 0; x < source.Chars[y].Length; x++)
                    {
                        if (source.Chars[y][x] == ' ')
                            continue;
                        // ignore boxes
                        if (nodes.Any(b => b.X <= x && b.X + b.Width >= x && b.Y <= y && b.Y + b.Height >= y))
                            continue;
                        if ((!source.AnyLine(x, y) || source.TopLine(x, y) == LineType.Double || source.LeftLine(x, y) == LineType.Double || source.BottomLine(x, y) == LineType.Double || source.RightLine(x, y) == LineType.Double))
                            throw new ParseErrorException(new ParseError($"Stray character: {char.ConvertFromUtf32(source.Chars[y][x])} (U+{source.Chars[y][x]:X4})", x, y, sourceFile));
                        if (x < source.Chars[y].Length - 1 && source.RightLine(x, y) != LineType.None && source.LeftLine(x + 1, y) != LineType.None && source.RightLine(x, y) != source.LeftLine(x + 1, y))
                            throw new ParseErrorException(new ParseError("Single line cannot suddenly switch to double line.", x + 1, y, sourceFile));
                        if (y < source.Chars.Length - 1 && source.BottomLine(x, y) != LineType.None && source.TopLine(x, y + 1) != LineType.None && source.BottomLine(x, y) != source.TopLine(x, y + 1))
                            throw new ParseErrorException(new ParseError("Single line cannot suddenly switch to double line.", x, y + 1, sourceFile));

                        var singleLines = new[] { source.TopLine(x, y), source.RightLine(x, y), source.BottomLine(x, y), source.LeftLine(x, y) }.Select(line => line == LineType.Single).ToArray();
                        var count = singleLines.Count(sl => sl);
                        if (count < 3)
                            continue;

                        var nodetype = count == 4 ? NodeType.CrossJunction : NodeType.TJunction;
                        var node = new UnparsedNode(x, y, 0, 0, nodetype);
                        nodes.Add(node);
                        for (int i = 0; i < 4; i++)
                            if (singleLines[i])
                                unfinishedEdges.Add(new UnfinishedEdge { StartNode = node, DirectionFromStartNode = (Direction) i, StartX = x, StartY = y, EndX = x, EndY = y, DirectionGoingTo = (Direction) i });
                    }
                }

                // Parse the connections between nodes and discover all the loose ends
                var visited = new bool[source.Chars.Length][];
                for (var y = 0; y < visited.Length; y++)
                    visited[y] = new bool[source.Chars[y].Length];
                var edges = new List<Edge>();
                while (unfinishedEdges.Count > 0)
                {
                    var edge = unfinishedEdges[0];
                    int x = edge.EndX, y = edge.EndY;
                    LineType connector;
                    switch (edge.DirectionGoingTo)
                    {
                        case Direction.Up: y--; connector = source.BottomLine(x, y); break;
                        case Direction.Left: x--; connector = source.RightLine(x, y); break;
                        case Direction.Down: y++; connector = source.TopLine(x, y); break;
                        case Direction.Right: x++; connector = source.LeftLine(x, y); break;
                        default: throw new ParseErrorException(new ParseError("The parser encountered an internal error.", x, y, sourceFile));
                    }
                    if (y >= 0 && y < visited.Length && x >= 0 && x < visited[y].Length)
                        visited[y][x] = true;
                    switch (connector)
                    {
                        case LineType.None:
                            // We encountered a loose end
                            unfinishedEdges.RemoveAt(0);
                            var node = new UnparsedNode(edge.EndX, edge.EndY, 0, 0, NodeType.End);
                            edges.Add(new Edge(edge.StartNode, edge.DirectionFromStartNode, node, edge.DirectionGoingTo.Opposite(), edge.StartX, edge.StartY, edge.EndX, edge.EndY));
                            nodes.Add(node);
                            break;

                        case LineType.Single:
                            // Check whether this edge connects to any other edge
                            var otherEdge = unfinishedEdges.FirstOrDefault(ue => ue.EndX == x && ue.EndY == y && ue.DirectionGoingTo == edge.DirectionGoingTo.Opposite());
                            if (otherEdge != null)
                            {
                                unfinishedEdges.RemoveAt(0);
                                unfinishedEdges.Remove(otherEdge);
                                edges.Add(new Edge(edge.StartNode, edge.DirectionFromStartNode, otherEdge.StartNode, otherEdge.DirectionFromStartNode, edge.StartX, edge.StartY, x, y));
                                break;
                            }
                            // We can now assume this is not a junction, so just check which direction it’s going
                            edge.DirectionGoingTo =
                                edge.DirectionGoingTo != Direction.Down && source.TopLine(x, y) == LineType.Single ? Direction.Up :
                                edge.DirectionGoingTo != Direction.Up && source.BottomLine(x, y) == LineType.Single ? Direction.Down :
                                edge.DirectionGoingTo != Direction.Left && source.RightLine(x, y) == LineType.Single ? Direction.Right :
                                edge.DirectionGoingTo != Direction.Right && source.LeftLine(x, y) == LineType.Single ? Direction.Left :
                                throw new ParseErrorException(new ParseError("The parser encountered an internal error.", x, y, sourceFile));
                            edge.EndX = x;
                            edge.EndY = y;
                            break;

                        case LineType.Double:
                        default:
                            throw new ParseErrorException(new ParseError("Unexpected double line.", x, y, sourceFile));
                    }
                }

                // Complain about any extraneous characters anywhere
                for (int y = 0; y < source.Chars.Length; y++)
                    for (int x = 0; x < source.Chars[y].Length; x++)
                        if (source.Chars[y][x] != ' ' && !visited[y][x] && !nodes.Any(b => b.X <= x && b.X + b.Width >= x && b.Y <= y && b.Y + b.Height >= y))
                            throw new ParseErrorException(new ParseError("Stray line not connected to any program or function.", x, y, sourceFile));

                // Collect everything that is connected to each declaration node
                var declarations = new List<UnparsedFunctionDeclaration>();
                while (true)
                {
                    var declaration = nodes.FirstOrDefault(n => n.Type == NodeType.Declaration);
                    if (declaration == null)
                        break;
                    var (collectedNodes, collectedEdges) = collectAllConnected(nodes, edges, declaration);
                    var unparsedFunction = new UnparsedFunctionDeclaration(collectedNodes, collectedEdges, source);
                    declarations.Add(unparsedFunction);
                    if (functionNamesToAnalyze != null && functionNamesToAnalyze.Contains(unparsedFunction.DeclarationName))
                        functionsToAnalyze[unparsedFunction.DeclarationName] = unparsedFunction;
                }

                // If there is anything left, it must be the program. There must be exactly one output
                var outputs = nodes.Where(n => n.Type == NodeType.End).ToList();
                if (outputs.Count > 1)
                    throw new ParseErrorException(new ParseError("Cannot have more than one program output.", outputs[1].X, outputs[1].Y, sourceFile));
                else if (outputs.Count == 1)
                {
                    if (program != null)
                        throw new ParseErrorException(new ParseError("Cannot have more than one program.", outputs[0].X, outputs[0].Y, sourceFile));
                    var (collectedNodes, collectedEdges) = collectAllConnected(nodes, edges, outputs[0]);
                    program = new UnparsedProgram(collectedNodes, collectedEdges, source);
                }

                // If there is *still* anything left (other than comments), it’s an error
                var strayNode = nodes.FirstOrDefault(n => n.Type != NodeType.Comment);
                if (strayNode != null)
                    throw new ParseErrorException(new ParseError("Stray node unconnected to any declaration or program.", strayNode.X, strayNode.Y, sourceFile));
                var strayEdge = edges.FirstOrDefault();
                if (strayEdge != null)
                    throw new ParseErrorException(new ParseError("Stray edge unconnected to any declaration or program.", strayEdge.StartX, strayEdge.StartY, sourceFile));

                // Check that all function names are unique
                var privateDeclarationsByName = new Dictionary<string, UnparsedFunctionDeclaration>();
                foreach (var decl in declarations)
                {
                    if (declarationsByName.ContainsKey(decl.DeclarationName) || privateDeclarationsByName.ContainsKey(decl.DeclarationName))
                        throw new ParseErrorException(new ParseError($"Duplicate function declaration: ‘{decl.DeclarationName}’.", decl.DeclarationNode.X, decl.DeclarationNode.Y, sourceFile));
                    (decl.DeclarationIsPrivate ? privateDeclarationsByName : declarationsByName)[decl.DeclarationName] = decl;
                }

                // Associate all the call nodes that call a private function with the relevant declaration
                IEnumerable<UnparsedDeclaration> decls = declarations;
                if (outputs.Count == 1)
                    decls = decls.Concat([program]);
                foreach (var decl in decls)
                    foreach (var node in decl.Nodes.Where(n => n.Type == NodeType.Call))
                        if (privateDeclarationsByName.TryGetValue(node.GetContent(source), out var ufd))
                            declarationsByCallNode[node] = ufd;
            }

            if (program == null)
                throw new ParseErrorException(new ParseError("Source files do not contain a program (program must have an output)."));

            var functionNames = getFunctionNames ? string.Join(Environment.NewLine, declarationsByName.Keys.OrderBy(x => x, StringComparer.Ordinal)) : null;
            var functions = new Dictionary<UnparsedDeclaration, FuncitonFunction>();

            if (functionNamesToAnalyze == null)
                return (program: program.Parse(declarationsByName, declarationsByCallNode, functions), analysis: null, functionNames);

            var sb = new StringBuilder();
            foreach (var functionName in functionNamesToAnalyze)
            {
                if (functionName == "")
                    program.Parse(declarationsByName, declarationsByCallNode, functions).Analyze(sb);
                else if (!functionsToAnalyze.ContainsKey(functionName))
                    sb.AppendLine($"No such function: “{functionName}”.");
                else
                    functionsToAnalyze[functionName].Parse(declarationsByName, declarationsByCallNode, functions).Analyze(sb);
                sb.AppendLine();
            }
            return (program: null, analysis: sb.ToString(), functionNames);
        }

        private static (List<UnparsedNode> outNodes, List<Edge> outEdges) collectAllConnected(List<UnparsedNode> nodes, List<Edge> edges, UnparsedNode initialNode)
        {
            var theseNodes = new List<UnparsedNode> { initialNode };
            var theseEdges = new List<Edge>();
            nodes.Remove(initialNode);

            while (true)
            {
                var edge = edges.FirstOrDefault(e => theseNodes.Contains(e.StartNode));
                if (edge != null)
                {
                    edges.Remove(edge);
                    theseEdges.Add(edge);
                    nodes.Remove(edge.EndNode);
                    if (!theseNodes.Contains(edge.EndNode))
                        theseNodes.Add(edge.EndNode);
                }
                else
                {
                    edge = edges.FirstOrDefault(e => theseNodes.Contains(e.EndNode));
                    if (edge == null)
                        break;
                    edges.Remove(edge);
                    theseEdges.Add(edge);
                    nodes.Remove(edge.StartNode);
                    if (!theseNodes.Contains(edge.StartNode))
                        theseNodes.Add(edge.StartNode);
                }
            }
            return (theseNodes, theseEdges);
        }

        public static BigInteger StringToInteger(string str)
        {
            BigInteger result = BigInteger.Zero;
            int atBit = 0;
            int i = 0;
            while (i < str.Length)
            {
                var cp = char.ConvertToUtf32(str, i);
                result |= (BigInteger) (cp == 0 ? 0x110000 : cp) << atBit;
                i += char.IsSurrogate(str, i) ? 2 : 1;
                atBit += 21;
            }
            return result;
        }

        public static string IntegerToString(BigInteger integer)
        {
            if (integer < 0)
                return null;
            var sb = new StringBuilder();
            while (integer != BigInteger.Zero && integer != BigInteger.MinusOne)
            {
                var cp = (int) (integer & ((1 << 21) - 1));
                sb.Append(char.ConvertFromUtf32(cp == 0x110000 ? 0 : cp));
                integer >>= 21;
            }
            return sb.ToString();
        }
    }
}
