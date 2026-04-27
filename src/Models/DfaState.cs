// ============================================================
// Models/DfaState.cs
// ============================================================
// Defines Q — the complete set of DFA states.
//
// Naming convention:
//   q0          — initial state
//   q_<CMD>     — just saw the opening keyword
//   q_<CMD>_*   — intermediate steps within that command's path
//   q_SEMI      — ★ sole accept state F = { q_SEMI }
//   q_DEAD      — trap / error state (once entered, stays here)
//
// Every valid SQL pattern is a directed path from q0 to q_SEMI
// through the states below.  Any unrecognized (state, token)
// pair sends the machine to q_DEAD with no exit.
// ============================================================

namespace DfaSqlValidator.Models
{
    /// <summary>
    /// All states in Q for the SQL-validator DFA.
    /// </summary>
    public enum DfaState
    {
        // ── Start ────────────────────────────────────────────
        q0,

        // ── SELECT path ──────────────────────────────────────
        // SELECT <col|*> FROM <table> [WHERE <col> = <val>] ;
        q_SELECT,       // seen: SELECT
        q_SEL_COL,      // seen: SELECT <column|*>
        q_SEL_FROM,     // seen: SELECT … FROM
        q_SEL_TBL,      // seen: SELECT … FROM <table>      ← pre-accept
        q_SEL_WHERE,    // seen: … WHERE
        q_SEL_WCOL,     // seen: … WHERE <column>
        q_SEL_WEQ,      // seen: … WHERE <column> =
        q_SEL_WVAL,     // seen: … WHERE <column> = <value> ← pre-accept

        // ── INSERT path ──────────────────────────────────────
        // INSERT INTO <table> VALUES ( <val> [, <val>]* ) ;
        q_INSERT,       // seen: INSERT
        q_INS_INTO,     // seen: INSERT INTO
        q_INS_TBL,      // seen: INSERT INTO <table>
        q_INS_VAL,      // seen: … VALUES
        q_INS_LP,       // seen: … VALUES (
        q_INS_VVAL,     // seen: … ( <value>
        q_INS_RP,       // seen: … ( <value> )              ← pre-accept

        // ── DELETE path ──────────────────────────────────────
        // DELETE FROM <table> WHERE <col> = <val> ;
        q_DELETE,       // seen: DELETE
        q_DEL_FROM,     // seen: DELETE FROM
        q_DEL_TBL,      // seen: DELETE FROM <table>
        q_DEL_WHERE,    // seen: … WHERE
        q_DEL_WCOL,     // seen: … WHERE <column>
        q_DEL_WEQ,      // seen: … WHERE <column> =
        q_DEL_WVAL,     // seen: … WHERE <column> = <value> ← pre-accept

        // ── UPDATE path ──────────────────────────────────────
        // UPDATE <table> SET <col> = <val> ;
        q_UPDATE,       // seen: UPDATE
        q_UPD_TBL,      // seen: UPDATE <table>
        q_UPD_SET,      // seen: … SET
        q_UPD_SCOL,     // seen: … SET <column>
        q_UPD_SEQ,      // seen: … SET <column> =
        q_UPD_SVAL,     // seen: … SET <column> = <value>   ← pre-accept

        // ── Terminal ─────────────────────────────────────────
        q_SEMI,         // ★ ACCEPT — valid query, terminated with ;
        q_DEAD          // ✗ TRAP   — invalid token, non-recoverable
    }
}
