// ============================================================
// Models/TraceStep.cs
// ============================================================
// One recorded step produced by the DFA engine as it processes
// a token.  The Display layer uses a List<TraceStep> to render
// the step-by-step transition trace without coupling the engine
// to any console output concern.
// ============================================================

namespace DfaSqlValidator.Models
{
    /// <summary>
    /// Immutable record of a single DFA transition (or failure).
    /// </summary>
    public readonly struct TraceStep
    {
        /// <summary>State the machine was in before consuming the token.</summary>
        public DfaState From { get; }

        /// <summary>The token type that triggered this transition.</summary>
        public TokenType Input { get; }

        /// <summary>The original source text of the token (for display).</summary>
        public string Lexeme { get; }

        /// <summary>State the machine moved to after consuming the token.</summary>
        public DfaState To { get; }

        /// <summary>True when no transition existed — machine went to q_DEAD.</summary>
        public bool IsError { get; }

        public TraceStep(DfaState from, TokenType input, string lexeme,
                         DfaState to, bool isError = false)
        {
            From = from;
            Input = input;
            Lexeme = lexeme;
            To = to;
            IsError = isError;
        }
    }
}
