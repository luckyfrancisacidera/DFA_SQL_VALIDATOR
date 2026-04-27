// ============================================================
// Models/ValidationResult.cs
// ============================================================
// Bundles everything the DFA engine produces in a single run
// into one coherent result object.
// ============================================================

namespace DfaSqlValidator.Models
{
    /// <summary>
    /// The complete output of one DFA validation run.
    /// </summary>
    public sealed class ValidateResult
    {
        /// <summary>True iff the input string was accepted (ended in q_SEMI).</summary>
        public bool Accepted { get; }

        /// <summary>Every transition (or dead-state failure) recorded in order.</summary>
        public List<TraceStep> Trace { get; }

        /// <summary>
        /// Human-readable error message when Accepted == false.
        /// Empty string when Accepted == true.
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>The tokenized input that was fed to the DFA.</summary>
        public List<Token> Tokens { get; }

        public ValidateResult(bool accepted,
                                List<TraceStep> trace,
                                List<Token> tokens,
                                string errorMessage = "")
        {
            Accepted = accepted;
            Trace = trace;
            Tokens = tokens;
            ErrorMessage = errorMessage;
        }
    }
}
