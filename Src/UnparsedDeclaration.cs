using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Funciton
{
    /// <summary>
    ///     <para>
    ///         Represents a connected set of <see cref="UnparsedNode"/> and <see cref="Edge"/> objects. This is an
    ///         abstract class; see <see cref="UnparsedFunctionDeclaration"/> for function declarations (which have a
    ///         declaration box among their nodes) and <see cref="UnparsedProgram"/> for the main program (which does
    ///         not).</para>
    ///     <para>
    ///         After parsing, this turns into a <see cref="FuncitonFunction"/> or a <see cref="FuncitonProgram"/>.</para></summary>
    abstract class UnparsedDeclaration(List<UnparsedNode> nodes, List<Edge> edges, SourceAsChars source)
    {
        public List<UnparsedNode> Nodes { get; private set; } = nodes;
        public List<Edge> Edges { get; private set; } = edges;
        protected SourceAsChars _source = source;

        public virtual FuncitonFunction Parse(Dictionary<string, UnparsedFunctionDeclaration> unparsedFunctionsByName, Dictionary<UnparsedNode, UnparsedFunctionDeclaration> unparsedFunctionsByNode, Dictionary<UnparsedDeclaration, FuncitonFunction> parsedFunctions)
        {
            var processedEdges = new HashSet<Edge>();

            void isCorrect(Edge e) { processedEdges.Add(e); }
            void isFlipped(Edge e)
            {
                if (processedEdges.Contains(e))
                    throw new ParseErrorException(
                        new ParseError("Program is ambiguous: cannot determine the direction of this edge.", e.StartX, e.StartY, _source.SourceFile),
                        new ParseError("... edge ends here.", e.EndX, e.EndY, _source.SourceFile));
                e.Flip();
                processedEdges.Add(e);
            }

            // Deduce all the inputs and outputs on every node
            var q = new Queue<UnparsedNode>(Nodes);
            var enqueued = 0;
            while (q.Count > 0)
            {
                var node = q.Dequeue();
                var edges = new[] { Direction.Up, Direction.Right, Direction.Down, Direction.Left }
                    .Select(dir => Edges.SingleOrDefault(e => (e.StartNode == node && e.DirectionFromStartNode == dir) || (e.EndNode == node && e.DirectionFromEndNode == dir))).ToArray();
                var known = edges.Select(e => e != null && processedEdges.Contains(e)).ToArray();
                if (!node.Deduce(edges, known, unparsedFunctionsByName, unparsedFunctionsByNode, isCorrect, isFlipped, _source))
                {
                    q.Enqueue(node);
                    enqueued++;
                    if (enqueued == q.Count)
                        throw new ParseErrorException(new ParseError($"Program is ambiguous: cannot determine the direction of all the edges in {(this is UnparsedFunctionDeclaration fnc ? $"function “{fnc.DeclarationName}”" : "the main program")}.", null, null, _source.SourceFile));
                }
                else
                    enqueued = 0;
            }
            Helpers.Assert(Nodes.All(n => n.Edges != null && n.Connectors != null));

            var outputs = new Node[4];
            parsedFunctions[this] = _function = createFuncitonFunction(outputs);

            _unparsedFunctionsByName = unparsedFunctionsByName;
            _unparsedFunctionsByNode = unparsedFunctionsByNode;
            _parsedFunctions = parsedFunctions;

            foreach (var node in Nodes.Where(n => n.Type == NodeType.End))
            {
                Helpers.Assert(node.Edges[0] != null);
                Helpers.Assert(node.Edges[1] == null && node.Edges[2] == null && node.Edges[3] == null);
                outputs[(int) node.Edges[0].DirectionFromEndNode] = walk(node.Edges[0], [], node.Edges[0]).node;
            }
            return _function;
        }

        protected abstract FuncitonFunction createFuncitonFunction(Node[] outputs);

        private FuncitonFunction _function;
        // In all the following tuples, the second element is a list of lambda parameter dependencies
        private readonly Dictionary<Edge, (Node node, Edge[] λParamDeps)> _edgesAlready = [];
        private readonly Dictionary<UnparsedNode, (Call call, Edge[] λParamDeps)> _callsAlready = [];
        private readonly Dictionary<UnparsedNode, (LambdaInvocation invocation, Edge[] λParamDeps)> _lambdasAlready = [];
        private readonly Dictionary<UnparsedNode, LambdaExpressionParameterNode> _lambdaParameters = [];
        private Dictionary<string, UnparsedFunctionDeclaration> _unparsedFunctionsByName;
        private Dictionary<UnparsedNode, UnparsedFunctionDeclaration> _unparsedFunctionsByNode;
        private Dictionary<UnparsedDeclaration, FuncitonFunction> _parsedFunctions;

        private (Node node, Edge[] λParamDeps) walk(Edge edge, Edge[] allowedDependencies, Edge latestOutput)
        {
            if (_edgesAlready.TryGetValue(edge, out var tryNode))
            {
                if (tryNode.node == null)
                    throw new ParseErrorException(new ParseError($"The {(_function.Name == "" ? "main program" : $"function “{_function.Name}”")} has a cycle in it. It can never evaluate because it would always be an infinite loop.", edge.EndX, edge.EndY, _source.SourceFile));
                var disallowedDependency = tryNode.λParamDeps.FirstOrDefault(d => !allowedDependencies.Contains(d));
                if (disallowedDependency != null)
                    throwDisallowedDependency(latestOutput, disallowedDependency);
                return tryNode;
            }
            _edgesAlready[edge] = (null, null);

            var node = edge.StartNode;
            var outputPosition = Enumerable.Range(0, 4).First(i => node.Edges[i] == edge && node.Connectors[i] == ConnectorType.Output);

            switch (node.Type)
            {
                case NodeType.TJunction:
                    if (node.Connectors[0] == ConnectorType.Output)
                    {
                        // NAND
                        Helpers.Assert(node.Connectors[1] == ConnectorType.Input);
                        Helpers.Assert(node.Connectors[3] == ConnectorType.Input);
                        var left = walk(node.Edges[3], allowedDependencies, latestOutput);
                        var right = walk(node.Edges[1], allowedDependencies, latestOutput);
                        return _edgesAlready[edge] = (
                            node: new NandNode(_function, left.node, right.node),
                            λParamDeps: left.λParamDeps.ArrayUnion(right.λParamDeps));
                    }
                    else
                    {
                        // splitter
                        Helpers.Assert(node.Connectors[0] == ConnectorType.Input);
                        Helpers.Assert(node.Connectors[1] == ConnectorType.Output);
                        Helpers.Assert(node.Connectors[3] == ConnectorType.Output);
                        if (node.Edges[0] == edge)
                            throw new ParseErrorException(new ParseError("This splitter is connected to itself. Such a construct is not allowed as it would always cause an infinite loop.", node.X, node.Y, _source.SourceFile));
                        return _edgesAlready[edge] = walk(node.Edges[0], allowedDependencies, latestOutput);
                    }

                case NodeType.CrossJunction:
                {
                    Helpers.Assert(node.Connectors[0] == ConnectorType.Input);
                    Helpers.Assert(node.Connectors[1] == ConnectorType.Output);
                    Helpers.Assert(node.Connectors[2] == ConnectorType.Output);
                    Helpers.Assert(node.Connectors[3] == ConnectorType.Input);
                    Helpers.Assert(node.Edges[1] == edge || node.Edges[2] == edge);

                    var left = walk(node.Edges[0], allowedDependencies, latestOutput);
                    var right = walk(node.Edges[3], allowedDependencies, latestOutput);
                    return _edgesAlready[edge] = (node: node.Edges[1] == edge
                        ? new LessThanNode(_function, left.node, right.node)
                        : new ShiftLeftNode(_function, left.node, right.node), left.λParamDeps.ArrayUnion(right.λParamDeps));
                }

                case NodeType.Declaration:
                    return _edgesAlready[edge] = (node: new InputNode(_function, (int) edge.DirectionFromStartNode), λParamDeps: []);

                case NodeType.Call:
                    UnparsedFunctionDeclaration decl;
                    if (!_unparsedFunctionsByNode.TryGetValue(node, out decl) && !_unparsedFunctionsByName.TryGetValue(node.GetContent(_source), out decl))
                        throw new ParseErrorException(new ParseError($"Call to undefined function “{node.GetContent(_source)}”.", node.X, node.Y, _source.SourceFile));

                    FuncitonFunction func;
                    if (!_parsedFunctions.TryGetValue(decl, out func))
                        func = decl.Parse(_unparsedFunctionsByName, _unparsedFunctionsByNode, _parsedFunctions);

                    // Try to optimize away no-op functions
                    int? inputPosition = func.GetInputForOutputIfNop(outputPosition);
                    Helpers.Assert(inputPosition == null || node.Connectors[inputPosition.Value] == ConnectorType.Input);
                    if (inputPosition != null)
                        return _edgesAlready[edge] = walk(node.Edges[inputPosition.Value], allowedDependencies, latestOutput);

                    if (!_callsAlready.ContainsKey(node))
                    {
                        var inputs = new Node[4];
                        var λParamDeps = Array.Empty<Edge>();
                        for (int i = 0; i < 4; i++)
                        {
                            if (node.Connectors[i] != ConnectorType.Input)
                                continue;
                            var (rNode, rλParamDeps) = walk(node.Edges[i], allowedDependencies, latestOutput);
                            inputs[i] = rNode;
                            λParamDeps = λParamDeps.ArrayUnion(rλParamDeps);
                        }
                        _callsAlready[node] = (call: new Call(func, inputs), λParamDeps);
                    }
                    return _edgesAlready[edge] = (node: new CallOutputNode(_function, outputPosition, _callsAlready[node].call), _callsAlready[node].λParamDeps);

                case NodeType.Literal:
                    var content = Regex.Replace(node.GetContent(_source), @"\s*\n\s*", "").Trim().Replace('−', '-');
                    Node newLiteralNode;
                    if (content.Length == 0)
                        newLiteralNode = new StdInNode(_function);
                    else
                    {
                        if (!BigInteger.TryParse(content, out var literal))
                            throw new ParseErrorException(new ParseError("Literal does not represent a valid integer.", node.X, node.Y, _source.SourceFile));
                        newLiteralNode = new LiteralNode(_function, literal);
                    }
                    return _edgesAlready[edge] = (node: newLiteralNode, λParamDeps: []);

                case NodeType.LambdaInvocation:
                    if (!string.IsNullOrWhiteSpace(node.GetContent(_source)))
                        throw new ParseErrorException(new ParseError("Lambda invocation boxes must be empty.", node.X, node.Y, _source.SourceFile));

                    if (!_lambdasAlready.ContainsKey(node))
                    {
                        Helpers.Assert(node.Connectors[0] == ConnectorType.Input);
                        Helpers.Assert(node.Connectors[1] == ConnectorType.Output);
                        Helpers.Assert(node.Connectors[2] == ConnectorType.Output);
                        Helpers.Assert(node.Connectors[3] == ConnectorType.Input);
                        var lambdaGetter = walk(node.Edges[0], allowedDependencies, latestOutput);
                        var argument = walk(node.Edges[3], allowedDependencies, latestOutput);
                        _lambdasAlready[node] = (
                            invocation: new LambdaInvocation(argument.node, lambdaGetter.node),
                            λParamDeps: lambdaGetter.λParamDeps.ArrayUnion(argument.λParamDeps));
                    }
                    return _edgesAlready[edge] = (
                        node: new LambdaInvocationOutputNode(_function, outputPosition, _lambdasAlready[node].invocation),
                        _lambdasAlready[node].λParamDeps);

                case NodeType.LambdaExpression:
                    if (!string.IsNullOrWhiteSpace(node.GetContent(_source)))
                        throw new ParseErrorException(new ParseError("Lambda expression boxes must be empty.", node.X, node.Y, _source.SourceFile));
                    Helpers.Assert(node.Connectors[0] == ConnectorType.Input);
                    Helpers.Assert(node.Connectors[1] == ConnectorType.Output);
                    Helpers.Assert(node.Connectors[2] == ConnectorType.Output);
                    Helpers.Assert(node.Connectors[3] == ConnectorType.Input);

                    switch (outputPosition)
                    {
                        case 1: // parameter
                            if (!allowedDependencies.Contains(edge))
                                throwDisallowedDependency(latestOutput, edge);
                            if (!_lambdaParameters.ContainsKey(node))
                                _lambdaParameters[node] = new LambdaExpressionParameterNode(_function);
                            return _edgesAlready[edge] = (_lambdaParameters[node], [edge]);

                        case 2: // lambdaGetter
                                // Need to put a skeleton instance into _edgesAlready because this node allows cycles
                            var clonedNode = new LambdaExpressionNode(_function);
                            _edgesAlready[edge] = (clonedNode, []);
                            // Walk the return values first so that they will create the lambda parameter node
                            var newAllowedDependencies = allowedDependencies.ArrayUnion(node.Edges[1]);
                            clonedNode.ReturnValue1 = walk(node.Edges[0], newAllowedDependencies, node.Edges[0]).node;
                            clonedNode.ReturnValue2 = walk(node.Edges[3], newAllowedDependencies, node.Edges[3]).node;
                            // If the lambda parameter is not in _lambdaParameters, it means we did not reach the lambda input and therefore the lambda
                            // ignores its input, so we can just pass a null node because it will never get evaluated anyway
                            clonedNode.Parameter = _lambdaParameters.Get(node, null);
                            return _edgesAlready[edge];

                        default:
                            throw new ParseErrorException(new ParseError("The parser encountered an internal error parsing a lambda expression.", node.X, node.Y, _source.SourceFile));
                    }

                case NodeType.End:
                case NodeType.Comment:
                default:
                    throw new ParseErrorException(new ParseError("The parser encountered an internal error.", node.X, node.Y, _source.SourceFile));
            }
        }

        private void throwDisallowedDependency(Edge latestOutput, Edge disallowedDependency)
        {
            throw new ParseErrorException(
                new ParseError("Output cannot depend on a more-deeply nested lambda expression input.", latestOutput.EndX, latestOutput.EndY, _source.SourceFile),
                new ParseError("    — Lambda expression input is here.", disallowedDependency.StartX, disallowedDependency.StartY, _source.SourceFile)
            );
        }

        public virtual ConnectorType[] Connectors
        {
            get
            {
                var connectors = new ConnectorType[4];
                foreach (var edge in Edges.Where(e => e.StartNode.Type == NodeType.End || e.EndNode.Type == NodeType.End))
                {
                    var isStart = edge.StartNode.Type == NodeType.End;
                    var dir = isStart ? edge.DirectionFromStartNode : edge.DirectionFromEndNode;
                    if (connectors[(int) dir] != ConnectorType.None)
                        throw new ParseErrorException(new ParseError($"Duplicate connector: ‘{dir}’ is already an ‘{connectors[(int) dir]}’.", isStart ? edge.StartX : edge.EndX, isStart ? edge.StartY : edge.EndY, _source.SourceFile));
                    connectors[(int) dir] = ConnectorType.Output;
                }
                return connectors;
            }
        }
    }
}
