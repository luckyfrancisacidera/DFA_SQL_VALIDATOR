// ============================================================
// Models/TokenType.cs
// ============================================================
// Defines the DFA alphabet Σ — every token the system can
// recognize. The Tokenizer maps raw SQL text → TokenType,
// and the DFA engine uses TokenType as its input symbol.
// ============================================================

namespace DfaSqlValidator.Models
{
    /// <summary>
    /// Every symbol in the DFA alphabet Σ.
    /// Grouped by category for readability.
    /// </summary>
    public enum TokenType
    {
        // ── SQL Keywords ─────────────────────────────────────
        SELECT,
        FROM,
        WHERE,
        INSERT,
        INTO,
        VALUES,
        DELETE,
        UPDATE,
        SET,

        // ── Punctuation / Operator Symbols ───────────────────
        STAR,       // *
        EQUALS,     // =
        SEMICOLON,  // ;
        LPAREN,     // (
        RPAREN,     // )
        COMMA,      // ,

        // ── Context-Resolved Generic Tokens ──────────────────
        COLUMN,     // identifier used as a column name
        TABLE,      // identifier used as a table name
        VALUE,      // numeric literal, quoted string, or bare identifier used as data

        // ── Sentinel ─────────────────────────────────────────
        UNKNOWN     // unrecognized — immediately drives DFA to q_DEAD
    }
}
