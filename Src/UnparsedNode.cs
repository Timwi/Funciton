using System;
using System.Collections.Generic;
using System.Linq;

namespace Funciton
{
    /// <summary>
    ///     Represents a syntax element in source, which could be a box (declaration, call, literal, comment, lambda
    ///     expression, lambda invocation), a T-junction, cross-junction, or a loose end.</summary>
    sealed class UnparsedNode(int x, int y, int width, int height, NodeType type)
    {
        public int X { get; private set; } = x;
        public int Y { get; private set; } = y;
        public int Width { get; private set; } = width;
        public int Height { get; private set; } = height;
        public NodeType Type { get; private set; } = type;

        public override string ToString() => $"({X}, {Y}; {Width}, {Height}) = {Type}";
        private string _contentCache;
        public string GetContent(SourceAsChars source) => _contentCache ??= string.Join("\n", Enumerable.Range(Y + 1, Height - 1)
            .Select(i => string.Join("", source.Chars[i].Subarray(X + 1, Width - 1).Select(char.ConvertFromUtf32)).Trim()));

        public Edge[] Edges { get; private set; }
        public ConnectorType[] Connectors { get; private set; }

        private static readonly ConnectorType[][] _connConf = [[ConnectorType.Input, ConnectorType.Output, ConnectorType.Output, ConnectorType.Input]];    // CrossJunction, LambdaExpression and LambdaInvocation
        private static readonly ConnectorType[][] _tJunctionConnConf = [[ConnectorType.Input, ConnectorType.Output, ConnectorType.None, ConnectorType.Output], [ConnectorType.Output, ConnectorType.Input, ConnectorType.None, ConnectorType.Input]];
        private static readonly ConnectorType[][] _endConnConf = [[ConnectorType.Input, ConnectorType.None, ConnectorType.None, ConnectorType.None]];

        public bool Deduce(Edge[] edges, bool[] known, Dictionary<string, UnparsedFunctionDeclaration> unparsedDeclarationsByName, Dictionary<UnparsedNode, UnparsedFunctionDeclaration> unparsedDeclarationsByNode, Action<Edge> isCorrect, Action<Edge> isFlipped, SourceAsChars source)
        {
            switch (Type)
            {
                case NodeType.Declaration:
                    // Declarations have only outputs, and they are always in the correct orientation because they define it
                    foreach (var e in edges)
                    {
                        if (e != null && e.StartNode == this)
                            isCorrect(e);
                        if (e != null && e.EndNode == this)
                            isFlipped(e);
                    }
                    Edges = edges;
                    Connectors = edges.Select(e => e == null ? ConnectorType.None : ConnectorType.Output).ToArray();
                    return true;

                case NodeType.Literal:
                    // Literals have only outputs
                    foreach (var e in edges)
                    {
                        if (e != null && e.StartNode == this)
                            isCorrect(e);
                        if (e != null && e.EndNode == this)
                            isFlipped(e);
                    }
                    Edges = edges;
                    Connectors = edges.Select(e => e == null ? ConnectorType.None : ConnectorType.Output).ToArray();
                    return true;

                case NodeType.Call:
                    UnparsedFunctionDeclaration func;
                    if (!unparsedDeclarationsByNode.TryGetValue(this, out func) && !unparsedDeclarationsByName.TryGetValue(GetContent(source), out func))
                        throw new ParseErrorException(new ParseError($"Call to undefined function: {GetContent(source)}", X, Y, source.SourceFile));
                    return deduceGiven(edges, known, isCorrect, isFlipped, func.Connectors.Count(fc => fc != ConnectorType.None), [func.Connectors], source,
                        $"Incorrect number of connectors to call to function: {GetContent(source)}",
                        $"Incorrect orientation of connectors to call to function: {GetContent(source)}");

                case NodeType.TJunction:
                    return deduceGiven(edges, known, isCorrect, isFlipped, 3, _tJunctionConnConf, source,
                        "Incorrect number of connectors to T junction (this error indicates a bug in the parser; please report it).",
                        "Incorrect orientation of connectors to T junction (this error indicates a bug in the parser; please report it).");

                case NodeType.CrossJunction:
                    return deduceGiven(edges, known, isCorrect, isFlipped, 4, _connConf, source,
                        "Incorrect number of connectors to cross junction (this error indicates a bug in the parser; please report it).",
                        "Incorrect orientation of connectors to cross junction (this error indicates a bug in the parser; please report it).");

                case NodeType.LambdaExpression:
                    return deduceGiven(edges, known, isCorrect, isFlipped, 4, _connConf, source,
                        "Lambda expressions must have four connectors.",
                        "Lambda expressions must have two adjacent inputs and two adjacent outputs.");

                case NodeType.LambdaInvocation:
                    return deduceGiven(edges, known, isCorrect, isFlipped, 4, _connConf, source,
                        "Lambda invocations must have four connectors.",
                        "Lambda invocations must have two adjacent inputs and two adjacent outputs.");

                case NodeType.End:
                    return deduceGiven(edges, known, isCorrect, isFlipped, 1, _endConnConf, source,
                        "Incorrect number of connectors to end node (this error indicates a bug in the parser; please report it).",
                        "Incorrect orientation of connectors to end node (this error indicates a bug in the parser; please report it).");
            }
            throw new ParseErrorException(new ParseError($"The parser encountered an internal error: unrecognized node type: {Type}", X, Y, source.SourceFile));
        }

        private bool deduceGiven(Edge[] edges, bool[] known, Action<Edge> isCorrect, Action<Edge> isFlipped, int expected, ConnectorType[][] connectors, SourceAsChars source, string connectorsError, string orientationError)
        {
            if (edges.Count(e => e != null) != expected)
                throw new ParseErrorException(new ParseError(connectorsError, X, Y, source.SourceFile));

            var result = new List<(bool[] knowns, Edge[] edges, ConnectorType[] connectors, int rotation)>();
            foreach (var conn in connectors)
            {
                for (int rot = 0; rot < 4; rot++)
                {
                    var rotatedEdges = edges.Skip(rot).Concat(edges.Take(rot)).ToArray();
                    var rotatedKnowns = known.Skip(rot).Concat(known.Take(rot)).ToArray();
                    var valid = Enumerable.Range(0, 4).All(i =>
                            (rotatedEdges[i] == null && conn[i] == ConnectorType.None) ||
                            (!rotatedKnowns[i] && conn[i] != ConnectorType.None) ||
                            (rotatedKnowns[i] && rotatedEdges[i].StartNode == this && conn[i] == ConnectorType.Output) ||
                            (rotatedKnowns[i] && rotatedEdges[i].EndNode == this && conn[i] == ConnectorType.Input));
                    if (valid)
                        result.Add((rotatedKnowns, rotatedEdges, conn, rot));
                }
            }

            if (result.Count == 0)
                throw new ParseErrorException(new ParseError(orientationError, X, Y, source.SourceFile));

            for (int i = 0; i < edges.Length; i++)
            {
                var edge = edges[i];
                if (edge == null || known[i])
                    continue;
                var conns = result.Select(r => r.connectors[(i + 4 - r.rotation) % 4]).ToArray();
                if (conns.Skip(1).All(c => c == conns[0]))
                {
                    if (edge.StartNode == this && (int) edge.DirectionFromStartNode == i)
                        (conns[0] == ConnectorType.Output ? isCorrect : isFlipped)(edge);
                    else if (edge.EndNode == this && (int) edge.DirectionFromEndNode == i)
                        (conns[0] == ConnectorType.Input ? isCorrect : isFlipped)(edge);
                }
            }

            Edges = result[0].edges;
            Connectors = result[0].connectors;
            return result.Count == 1;
        }
    }
}
