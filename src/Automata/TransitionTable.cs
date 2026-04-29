// ============================================================
// Automata/TransitionTable.cs
// ============================================================
// Encapsulates δ: Q × Σ → Q — the DFA transition function.
// ============================================================

using System.Collections.Generic;
using DfaSqlValidator.Models;

namespace DfaSqlValidator.Automata
{
    /// <summary>
    /// Immutable DFA transition table.
    /// </summary>
    public static class TransitionTable
    {
        // Key:   (current state, input token type)
        // Value: next state
        private static readonly Dictionary<(DfaState, TokenType), DfaState> Delta;

        static TransitionTable()
        {
            Delta = new Dictionary<(DfaState, TokenType), DfaState>
            {
                // ════════════════════════════════════════════════════
                // q0 → Start: dispatch on opening keyword
                // ════════════════════════════════════════════════════
                { (DfaState.q0, TokenType.SELECT), DfaState.q_SELECT },
                { (DfaState.q0, TokenType.INSERT), DfaState.q_INSERT },
                { (DfaState.q0, TokenType.DELETE), DfaState.q_DELETE },
                { (DfaState.q0, TokenType.UPDATE), DfaState.q_UPDATE },

                // ════════════════════════════════════════════════════
                // SELECT path
                // Pattern A: SELECT <col|*> FROM <table> ;
                // Pattern B: SELECT <col|*> FROM <table> WHERE <col> = <val> ;
                // ════════════════════════════════════════════════════
                { (DfaState.q_SELECT,    TokenType.COLUMN),    DfaState.q_SEL_COL  },
                { (DfaState.q_SELECT,    TokenType.STAR),      DfaState.q_SEL_COL  },
                { (DfaState.q_SEL_COL,   TokenType.FROM),      DfaState.q_SEL_FROM },
                { (DfaState.q_SEL_FROM,  TokenType.TABLE),     DfaState.q_SEL_TBL  },

                // Pattern A terminates here
                { (DfaState.q_SEL_TBL,   TokenType.SEMICOLON), DfaState.q_SEMI     },

                // Pattern B continues with WHERE
                { (DfaState.q_SEL_TBL,    TokenType.WHERE),     DfaState.q_SEL_WHERE },
                { (DfaState.q_SEL_WHERE,  TokenType.COLUMN),   DfaState.q_SEL_WCOL  },
                { (DfaState.q_SEL_WCOL,   TokenType.EQUALS),   DfaState.q_SEL_WEQ   },
                { (DfaState.q_SEL_WEQ,    TokenType.VALUE),    DfaState.q_SEL_WVAL  },
                { (DfaState.q_SEL_WEQ,    TokenType.COLUMN),   DfaState.q_SEL_WVAL  },
                { (DfaState.q_SEL_WVAL,   TokenType.SEMICOLON),DfaState.q_SEMI      },

                // ════════════════════════════════════════════════════
                // INSERT path
                // Pattern: INSERT INTO <table> VALUES ( <val> [, <val>]* ) ;
                // ════════════════════════════════════════════════════
                { (DfaState.q_INSERT,   TokenType.INTO),      DfaState.q_INS_INTO },
                { (DfaState.q_INS_INTO, TokenType.TABLE),     DfaState.q_INS_TBL  },
                { (DfaState.q_INS_TBL,  TokenType.VALUES),    DfaState.q_INS_VAL  },
                { (DfaState.q_INS_VAL,  TokenType.LPAREN),    DfaState.q_INS_LP   },
                { (DfaState.q_INS_LP,   TokenType.VALUE),     DfaState.q_INS_VVAL },
                { (DfaState.q_INS_LP,   TokenType.COLUMN),    DfaState.q_INS_VVAL },

                // Comma-separated list: after each value, a comma loops back to q_INS_LP
                { (DfaState.q_INS_VVAL, TokenType.COMMA),     DfaState.q_INS_LP   },
                { (DfaState.q_INS_VVAL, TokenType.RPAREN),    DfaState.q_INS_RP   },
                { (DfaState.q_INS_RP,   TokenType.SEMICOLON), DfaState.q_SEMI     },

                // ════════════════════════════════════════════════════
                // DELETE path
                // Pattern: DELETE FROM <table> WHERE <col> = <val> ;
                // ════════════════════════════════════════════════════
                { (DfaState.q_DELETE,    TokenType.FROM),      DfaState.q_DEL_FROM  },
                { (DfaState.q_DEL_FROM,  TokenType.TABLE),     DfaState.q_DEL_TBL   },
                { (DfaState.q_DEL_TBL,   TokenType.WHERE),     DfaState.q_DEL_WHERE },
                { (DfaState.q_DEL_WHERE,  TokenType.COLUMN),   DfaState.q_DEL_WCOL  },
                { (DfaState.q_DEL_WCOL,   TokenType.EQUALS),   DfaState.q_DEL_WEQ   },
                { (DfaState.q_DEL_WEQ,    TokenType.VALUE),    DfaState.q_DEL_WVAL  },
                { (DfaState.q_DEL_WEQ,    TokenType.COLUMN),   DfaState.q_DEL_WVAL  },
                { (DfaState.q_DEL_WVAL,   TokenType.SEMICOLON),DfaState.q_SEMI      },

                // ════════════════════════════════════════════════════
                // UPDATE path
                // Pattern: UPDATE <table> SET <col> = <val> ;
                // ════════════════════════════════════════════════════
                { (DfaState.q_UPDATE,   TokenType.TABLE),     DfaState.q_UPD_TBL  },
                { (DfaState.q_UPD_TBL,  TokenType.SET),       DfaState.q_UPD_SET  },
                { (DfaState.q_UPD_SET,  TokenType.COLUMN),    DfaState.q_UPD_SCOL },
                { (DfaState.q_UPD_SCOL, TokenType.EQUALS),    DfaState.q_UPD_SEQ  },
                { (DfaState.q_UPD_SEQ,  TokenType.VALUE),     DfaState.q_UPD_SVAL },
                { (DfaState.q_UPD_SEQ,  TokenType.COLUMN),    DfaState.q_UPD_SVAL },
                { (DfaState.q_UPD_SVAL, TokenType.SEMICOLON), DfaState.q_SEMI     },
            };
        }

        /// <summary>
        /// Look up δ(current, token) → next state.
        /// Returns false when no transition is defined (caller should go to q_DEAD).
        /// </summary>
        public static bool TryGetNext(DfaState current, TokenType token, out DfaState next)
            => Delta.TryGetValue((current, token), out next);

        /// <summary>
        /// Returns all rows of the transition table for display purposes.
        /// Each row is (FromState, InputToken, ToState) as strings.
        /// </summary>
        public static IEnumerable<(string From, string Input, string To)> GetAllRows()
        {
            foreach (var kv in Delta)
                yield return (kv.Key.Item1.ToString(),
                              kv.Key.Item2.ToString(),
                              kv.Value.ToString());
        }
    }
}
