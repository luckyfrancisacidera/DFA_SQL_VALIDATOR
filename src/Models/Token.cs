// ============================================================
// Models/Token.cs
// ============================================================
// A Token is the atomic unit produced by the Tokenizer and
// consumed by the DFA engine.  It pairs a classified type
// with the original source lexeme so the display layer can
// show the user exactly what text triggered each transition.
// ============================================================

namespace DfaSqlValidator.Models
{
    /// <summary>
    /// Immutable data carrier representing one lexical unit.
    /// </summary>
    public readonly struct Token
    {
        /// <summary>The classified category of this lexeme.</summary>
        public TokenType Type { get; }

        /// <summary>The original source text (preserved for trace display).</summary>
        public string Lexeme { get; }

        public Token(TokenType type, string lexeme)
        {
            Type = type;
            Lexeme = lexeme;
        }

        public override string ToString() => $"[{Type}:\"{Lexeme}\"]";
    }
}
