using System.Collections.Generic;

namespace Funciton
{
    /// <summary>An <see cref="UnparsedDeclaration"/> without a declaration box, making it a program.</summary>
    sealed class UnparsedProgram(List<UnparsedNode> nodes, List<Edge> edges, SourceAsChars source) : UnparsedDeclaration(nodes, edges, source)
    {
        protected override FuncitonFunction createFuncitonFunction(Node[] outputs) => new FuncitonProgram(outputs);
        public new FuncitonProgram Parse(
            Dictionary<string, UnparsedFunctionDeclaration> unparsedFunctionsByName,
            Dictionary<UnparsedNode, UnparsedFunctionDeclaration> unparsedFunctionsByNode,
            Dictionary<UnparsedDeclaration, FuncitonFunction> parsedFunctions) =>
                (FuncitonProgram) base.Parse(unparsedFunctionsByName, unparsedFunctionsByNode, parsedFunctions);
    }
}
