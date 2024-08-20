using System.Collections.Generic;
using System.Linq;

namespace Funciton
{
    /// <summary>An <see cref="UnparsedDeclaration"/> that includes a declaration box, making it a function.</summary>
    sealed class UnparsedFunctionDeclaration : UnparsedDeclaration
    {
        public string DeclarationName { get; private set; }
        public bool DeclarationIsPrivate { get; private set; }
        public UnparsedNode DeclarationNode { get; private set; }

        public UnparsedFunctionDeclaration(List<UnparsedNode> nodes, List<Edge> edges, SourceAsChars source)
            : base(nodes, edges, source)
        {
            var decls = Nodes.Where(n => n.Type == NodeType.Declaration).ToList();
            if (decls.Count > 1)
                throw new ParseErrorException(
                    new ParseError("Cannot have more than one declaration box connected with each other.", decls[0].X, decls[0].Y, _source.SourceFile),
                    new ParseError("... other declaration box is here.", decls[1].X, decls[1].Y, _source.SourceFile));
            DeclarationNode = decls[0];
            if (DeclarationNode.Height != 2)
                throw new ParseErrorException(new ParseError("Declaration box must have exactly one line of content.", DeclarationNode.X, DeclarationNode.Y, _source.SourceFile));

            foreach (var callbox in nodes.Where(n => n.Type == NodeType.Call))
                if (callbox.Height != 2)
                    throw new ParseErrorException(new ParseError("Call box must have exactly one line of content.", callbox.X, callbox.Y, _source.SourceFile));

            DeclarationName = string.Join("", source.Chars[DeclarationNode.Y + 1].Subarray(DeclarationNode.X + 1, DeclarationNode.Width - 1).Select(char.ConvertFromUtf32)).Trim();
            if (DeclarationName.Length < 1)
                throw new ParseErrorException(new ParseError("Function name missing.", DeclarationNode.X, DeclarationNode.Y, _source.SourceFile));
            DeclarationIsPrivate = false;

            // Find the private marker
            var privateMarkerPosition = 0;
            var left = source.RightLine(DeclarationNode.X, DeclarationNode.Y + 1);
            if (left == LineType.Double)
                throw new ParseErrorException(new ParseError("Unrecognized marker.", DeclarationNode.X, DeclarationNode.Y + 1, _source.SourceFile));
            else if (left == LineType.Single)
            {
                var shape = source.GetLineShape(DeclarationNode.X, DeclarationNode.Y + 1, Direction.Right, DeclarationNode.X, DeclarationNode.Y, DeclarationNode.X + DeclarationNode.Width, DeclarationNode.Y + DeclarationNode.Height);
                if (shape == "→↑" || shape == "→↓")
                {
                    DeclarationIsPrivate = true;
                    DeclarationName = string.Join("", source.Chars[DeclarationNode.Y + 1].Subarray(DeclarationNode.X + 2, DeclarationNode.Width - 2).Select(char.ConvertFromUtf32)).Trim();
                    privateMarkerPosition = shape == "→↑" ? 1 : 3;
                }
                else
                    throw new ParseErrorException(new ParseError("Unrecognized marker.", DeclarationNode.X, DeclarationNode.Y + 1, _source.SourceFile));
            }

            var right = source.LeftLine(DeclarationNode.X + DeclarationNode.Width, DeclarationNode.Y + 1);
            if (right == LineType.Double)
                throw new ParseErrorException(new ParseError("Unrecognized marker.", DeclarationNode.X + DeclarationNode.Width, DeclarationNode.Y + 1, _source.SourceFile));
            else if (right == LineType.Single)
            {
                var shape = source.GetLineShape(DeclarationNode.X, DeclarationNode.Y + 1, Direction.Left, DeclarationNode.X, DeclarationNode.Y, DeclarationNode.X + DeclarationNode.Width, DeclarationNode.Y + DeclarationNode.Height);
                if (shape == "←↑" || shape == "←↓")
                {
                    if (DeclarationIsPrivate)
                        throw new ParseErrorException(new ParseError("Duplicate private marker.", DeclarationNode.X + DeclarationNode.Width, DeclarationNode.Y + 1, _source.SourceFile));
                    DeclarationIsPrivate = true;
                    DeclarationName = string.Join("", source.Chars[DeclarationNode.Y + 1].Subarray(DeclarationNode.X + 1, DeclarationNode.Width - 2).Select(char.ConvertFromUtf32)).Trim();
                    privateMarkerPosition = shape == "←↑" ? 2 : 4;
                }
                else
                    throw new ParseErrorException(new ParseError("Unrecognized marker.", DeclarationNode.X + DeclarationNode.Width, DeclarationNode.Y + 1, _source.SourceFile));
            }

            for (int i = DeclarationNode.X + 1; i < DeclarationNode.X + DeclarationNode.Width; i++)
            {
                if ((i != DeclarationNode.X + 1 || privateMarkerPosition != 1) && (i != DeclarationNode.X + DeclarationNode.Width - 1 || privateMarkerPosition != 2))
                    if (source.BottomLine(i, DeclarationNode.Y) != LineType.None)
                        throw new ParseErrorException(new ParseError("Unrecognized marker.", i, DeclarationNode.Y, _source.SourceFile));
                if ((i != DeclarationNode.X + 1 || privateMarkerPosition != 3) && (i != DeclarationNode.X + DeclarationNode.Width - 1 || privateMarkerPosition != 4))
                    if (source.TopLine(i, DeclarationNode.Y + DeclarationNode.Height) != LineType.None)
                        throw new ParseErrorException(new ParseError("Unrecognized marker.", i, DeclarationNode.Y + DeclarationNode.Height, _source.SourceFile));
            }
        }

        public override FuncitonFunction Parse(
            Dictionary<string, UnparsedFunctionDeclaration> unparsedFunctionsByName,
            Dictionary<UnparsedNode, UnparsedFunctionDeclaration> unparsedFunctionsByNode,
            Dictionary<UnparsedDeclaration, FuncitonFunction> parsedFunctions) =>
                parsedFunctions.TryGetValue(this, out var func) ? func : base.Parse(unparsedFunctionsByName, unparsedFunctionsByNode, parsedFunctions);

        public override ConnectorType[] Connectors
        {
            get
            {
                var connectors = base.Connectors;
                foreach (var edge in Edges.Where(e => e.StartNode == DeclarationNode || e.EndNode == DeclarationNode))
                {
                    var isStart = edge.StartNode == DeclarationNode;
                    var dir = isStart ? edge.DirectionFromStartNode : edge.DirectionFromEndNode;
                    if (connectors[(int) dir] != ConnectorType.None)
                        throw new ParseErrorException(new ParseError($"Duplicate connector: ‘{dir}’ is already an ‘{connectors[(int) dir]}’.", isStart ? edge.StartX : edge.EndX, isStart ? edge.StartY : edge.EndY, _source.SourceFile));
                    connectors[(int) dir] = ConnectorType.Input;
                }
                return connectors;
            }
        }

        protected override FuncitonFunction createFuncitonFunction(Node[] outputs) => new(outputs, DeclarationName);
    }
}
