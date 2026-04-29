// ============================================================
// Automata/DfaEngine.cs
// ============================================================
// The DFA engine is the simulation core.
// ============================================================

using DfaSqlValidator.Models;

namespace DfaSqlValidator.Automata
{
    /// <summary>
    /// Simulates the DFA M = (Q, Σ, δ, q0, F) on a token stream.
    /// </summary>
    public sealed class DfaEngine
    {
        // F — the accept states.  Only q_SEMI is a final/accept state.
        // A query is valid iff the machine is in an accept state after
        // consuming all tokens.
        private static readonly HashSet<DfaState> AcceptStates = new HashSet<DfaState> { DfaState.q_SEMI };

        /// <summary>
        /// Run the DFA on <paramref name="tokens"/> and return the full result.
        /// </summary>
        public ValidateResult Run(List<Token> tokens)
        {
            var trace = new List<TraceStep>();
            var current = DfaState.q0;   // q0 — initial state

            foreach (Token tok in tokens)
            {
                // δ(current, tok.Type) → next
                if (TransitionTable.TryGetNext(current, tok.Type, out DfaState next))
                {
                    trace.Add(new TraceStep(current, tok.Type, tok.Lexeme, next));
                    current = next;
                }
                else
                {
                    // No defined transition → trap in q_DEAD (non-recoverable)
                    trace.Add(new TraceStep(current, tok.Type, tok.Lexeme,
                                            DfaState.q_DEAD, isError: true));

                    string errorMsg =
                        $"No transition from [{current}] on token [{tok.Type}:\"{tok.Lexeme}\"].";

                    return new ValidateResult(
                        accepted: false,
                        trace: trace,
                        tokens: tokens,
                        errorMessage: errorMsg);
                }
            }

            // Check final state membership in F
            if (AcceptStates.Contains(current))
                return new ValidateResult(accepted: true, trace: trace, tokens: tokens);

            string incompleteMsg =
                $"Input ended in non-accept state [{current}]. " +
                "Query is incomplete (missing ';' or required clause).";

            return new ValidateResult(
                accepted: false,
                trace: trace,
                tokens: tokens,
                errorMessage: incompleteMsg);
        }
    }
}
