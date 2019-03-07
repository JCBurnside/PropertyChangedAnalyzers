namespace PropertyChangedAnalyzers
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    internal sealed class SyntaxTokenComparer : IEqualityComparer<SyntaxToken>
    {
        public static readonly SyntaxTokenComparer ByValueText = new SyntaxTokenComparer();

        private SyntaxTokenComparer()
        {
        }

        public static bool Equals(SyntaxToken x, SyntaxToken y) => x.ValueText == y.ValueText;

        bool IEqualityComparer<SyntaxToken>.Equals(SyntaxToken x, SyntaxToken y) => Equals(x, y);

        public int GetHashCode(SyntaxToken obj) => obj.ValueText.GetHashCode();
    }
}