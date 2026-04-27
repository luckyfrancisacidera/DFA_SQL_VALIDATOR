// ============================================================
// Cli/SqlValidator.cs
// ============================================================
// Wires the Tokenizer and DfaEngine together into
// a single callable unit.
//
// Responsibility:
//   Accept a raw SQL string, produce a ValidationResult.
//
// ============================================================

using DfaSqlValidator.Automata;
using DfaSqlValidator.Lexer;
using DfaSqlValidator.Models;

namespace DfaSqlValidator.Cli
{
    /// <summary>
    /// Orchestrates tokenization → DFA execution → result.
    /// </summary>
    public sealed class SqlValidator
    {
        private readonly DfaEngine _engine = new DfaEngine();

        /// <summary>
        /// Tokenize <paramref name="rawSql"/> then run the DFA on the tokens.
        /// </summary>
        public ValidateResult Validate(string rawSql)
        {
            var tokens = Tokenizer.Tokenize(rawSql);
            return _engine.Run(tokens);
        }
    }
}
