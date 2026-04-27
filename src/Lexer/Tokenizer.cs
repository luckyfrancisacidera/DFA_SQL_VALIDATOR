// ============================================================
// Lexer/Tokenizer.cs
// ============================================================
// The Tokenizer is the lexical analysis stage — equivalent to
// the "scanner" in a traditional compiler pipeline.
//
// Responsibility (single):
//   Convert a raw SQL string → List<Token>
//
// Algorithm:
//   1. Pre-process: pad single-char symbols with spaces so
//      splitting on whitespace naturally isolates them.
//   2. Split into raw parts.
//   3. Classify each part:
//      a. Known symbol → fixed TokenType (STAR, EQUALS, etc.)
//      b. Reserved keyword → keyword TokenType (SELECT, FROM, …)
//      c. Numeric literal → VALUE
//      d. Quoted string  → VALUE
//      e. Bare identifier → TABLE | COLUMN | VALUE
//         decided by a lightweight "context" cursor that tracks
//         what kind of identifier the grammar expects next.
//      f. Anything else  → UNKNOWN (DFA will reject)
//
// The context cursor is the only "intelligence" here; the
// Tokenizer is intentionally dumb about SQL semantics — all
// structural validation belongs to the DFA engine.
// ============================================================

using System.Text;
using DfaSqlValidator.Models;

namespace DfaSqlValidator.Lexer
{
    /// <summary>
    /// Converts a raw SQL string into a flat list of classified tokens.
    /// </summary>
    public static class Tokenizer
    {
        // ── Keyword lookup (case-insensitive) ────────────────────────────
        private static readonly Dictionary<string, TokenType> Keywords =
            new Dictionary<string, TokenType>(StringComparer.OrdinalIgnoreCase)
        {
            { "SELECT",  TokenType.SELECT  },
            { "FROM",    TokenType.FROM    },
            { "WHERE",   TokenType.WHERE   },
            { "INSERT",  TokenType.INSERT  },
            { "INTO",    TokenType.INTO    },
            { "VALUES",  TokenType.VALUES  },
            { "DELETE",  TokenType.DELETE  },
            { "UPDATE",  TokenType.UPDATE  },
            { "SET",     TokenType.SET     },
        };

        // ── Single-character symbol map ───────────────────────────────────
        private static readonly Dictionary<char, TokenType> Symbols =
            new Dictionary<char, TokenType>
        {
            { '*', TokenType.STAR      },
            { '=', TokenType.EQUALS    },
            { ';', TokenType.SEMICOLON },
            { '(', TokenType.LPAREN    },
            { ')', TokenType.RPAREN    },
            { ',', TokenType.COMMA     },
        };

        // ── Context labels (internal, not exposed) ────────────────────────
        private const string CTX_NONE = "NONE";
        private const string CTX_COLUMN = "COLUMN";
        private const string CTX_TABLE = "TABLE";
        private const string CTX_VALUE = "VALUE";

        /// <summary>
        /// Tokenize <paramref name="input"/> into an ordered list of tokens.
        /// Never throws; unknown content produces <see cref="TokenType.UNKNOWN"/> tokens.
        /// </summary>
        public static List<Token> Tokenize(string input)
        {
            var tokens = new List<Token>();
            var parts = Preprocess(input);
            var context = CTX_NONE;

            foreach (string part in parts)
            {
                // ── 1. Single-character symbols ───────────────────────────
                if (part.Length == 1 && Symbols.TryGetValue(part[0], out TokenType symType))
                {
                    tokens.Add(new Token(symType, part));
                    context = ContextAfterSymbol(symType);
                    continue;
                }

                // ── 2. Reserved keywords ──────────────────────────────────
                if (Keywords.TryGetValue(part, out TokenType kwType))
                {
                    tokens.Add(new Token(kwType, part.ToUpper()));
                    context = ContextAfterKeyword(kwType);
                    continue;
                }

                // ── 3. Quoted string literal → VALUE ─────────────────────
                if (part.StartsWith("'") || part.StartsWith("\""))
                {
                    tokens.Add(new Token(TokenType.VALUE, part));
                    context = CTX_NONE;
                    continue;
                }

                // ── 4. Numeric literal → VALUE ────────────────────────────
                if (double.TryParse(part, out _))
                {
                    tokens.Add(new Token(TokenType.VALUE, part));
                    context = CTX_NONE;
                    continue;
                }

                // ── 5. Bare identifier — classify by context ──────────────
                switch (context)
                {
                    case CTX_COLUMN:
                        tokens.Add(new Token(TokenType.COLUMN, part));
                        context = CTX_NONE;
                        break;

                    case CTX_TABLE:
                        tokens.Add(new Token(TokenType.TABLE, part));
                        context = CTX_NONE;
                        break;

                    case CTX_VALUE:
                        tokens.Add(new Token(TokenType.VALUE, part));
                        context = CTX_NONE;
                        break;

                    default:
                        // Cannot classify — DFA will reject this
                        tokens.Add(new Token(TokenType.UNKNOWN, part));
                        break;
                }
            }

            return tokens;
        }

        // ── Helpers ───────────────────────────────────────────────────────

        /// <summary>
        /// Pad known single-char symbols with spaces so the split step
        /// always isolates them as their own tokens.
        /// </summary>
        private static string[] Preprocess(string input)
        {
            var sb = new StringBuilder(input.Length * 2);
            foreach (char c in input)
            {
                if (Symbols.ContainsKey(c))
                    sb.Append(' ').Append(c).Append(' ');
                else
                    sb.Append(c);
            }
            return sb.ToString().Split(
                new[] { ' ', '\t', '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// After emitting a keyword, what kind of identifier does the grammar
        /// expect next?  This drives the context-sensitive classification of
        /// bare identifiers (step 5 above).
        /// </summary>
        private static string ContextAfterKeyword(TokenType kw)
        {
            switch (kw)
            {
                case TokenType.SELECT: return CTX_COLUMN;
                case TokenType.FROM: return CTX_TABLE;
                case TokenType.INTO: return CTX_TABLE;
                case TokenType.WHERE: return CTX_COLUMN;
                case TokenType.UPDATE: return CTX_TABLE;
                case TokenType.SET: return CTX_COLUMN;
                case TokenType.VALUES: return CTX_VALUE;
                default: return CTX_NONE;
            }
        }

        /// <summary>
        /// After emitting a symbol, what context should the next identifier
        /// be resolved in?
        /// </summary>
        private static string ContextAfterSymbol(TokenType sym)
        {
            switch (sym)
            {
                case TokenType.EQUALS: return CTX_VALUE;
                case TokenType.LPAREN: return CTX_VALUE;
                case TokenType.COMMA: return CTX_VALUE;
                default: return CTX_NONE;
            }
        }
    }
}
