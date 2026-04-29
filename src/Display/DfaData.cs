// ============================================================
// Display/DfaData.cs
// ============================================================

namespace DfaSqlValidator.Display
{
    public record StateRow(
        string Name,
        string Color,
        string Desc,
        string?[] KwCells,   // 8 keyword columns
        string?[] SymCells   // 9 symbol/token columns
    );

    public static class DfaData
    {
        // ── Alphabet ─────────────────────────────────────────────────────

        public static readonly string[] KeywordLabels =
        {
            "SELECT","INSERT","DELETE","UPDATE",
            "FROM","INTO","WHERE/SET","VALUES",
        };

        public static readonly string[] SymbolLabels =
        {
            "STAR","=",";",
            "(",")",",",
            "COLUMN","TABLE","VALUE",
        };

        // ── Transition rows ──────────────────────────────────────────────

        private const string SEMI = "q_SEMI";
        private const string DEAD = "q_DEAD";
        private const string? _ = null;

        // S(name, color, desc, kw[8], sym[9])
        public static readonly StateRow[] States = new[]
        {
            // ── Start ────────────────────────────────────────────────────
            S("q0", "cyan", "start state",
              kw:  new[]{ "q_SELECT","q_INSERT","q_DELETE","q_UPDATE", _,_,_,_ },
              sym: new string?[]{ _,_,_,_,_,_,_,_,_ }),

            // ── SELECT ───────────────────────────────────────────────────
            S("q_SELECT", "cyan", "after: SELECT",
              kw:  new string?[]{ _,_,_,_,_,_,_,_ },
              sym: new[]{ "q_SEL_COL",_,_,_,_,_,"q_SEL_COL",_,_ }),

            S("q_SEL_COL", "cyan", "after: SELECT <col|*>",
              kw:  new[]{ _,_,_,_,"q_SEL_FROM",_,_,_ },
              sym: new string?[]{ _,_,_,_,_,_,_,_,_ }),

            S("q_SEL_FROM", "cyan", "after: SELECT <col> FROM",
              kw:  new string?[]{ _,_,_,_,_,_,_,_ },
              sym: new[]{ _,_,_,_,_,_,_,"q_SEL_TBL",_ }),

            S("q_SEL_TBL", "cyan", "after: SELECT <col> FROM <tbl>  // accept if ;",
              kw:  new string?[]{ _,_,_,_,_,_,"q_WHERE",_ },
              sym: new[]{ _,_,SEMI,_,_,_,_,_,_ }),

            // ── INSERT ───────────────────────────────────────────────────
            S("q_INSERT", "cyan", "after: INSERT",
              kw:  new[]{ _,_,_,_,_,"q_INS_INTO",_,_ },
              sym: new string?[]{ _,_,_,_,_,_,_,_,_ }),

            S("q_INS_INTO", "cyan", "after: INSERT INTO",
              kw:  new string?[]{ _,_,_,_,_,_,_,_ },
              sym: new[]{ _,_,_,_,_,_,_,"q_INS_TBL",_ }),

            S("q_INS_TBL", "cyan", "after: INSERT INTO <tbl>",
              kw:  new[]{ _,_,_,_,_,_,_,"q_INS_VAL" },
              sym: new string?[]{ _,_,_,_,_,_,_,_,_ }),

            S("q_INS_VAL", "cyan", "after: INSERT INTO <tbl> VALUES",
              kw:  new string?[]{ _,_,_,_,_,_,_,_ },
              sym: new[]{ _,_,_,"q_INS_LP",_,_,_,_,_ }),

            S("q_INS_LP", "cyan", "after: VALUES (",
              kw:  new string?[]{ _,_,_,_,_,_,_,_ },
              sym: new[]{ _,_,_,_,_,_,"q_INS_VVAL",_,"q_INS_VVAL" }),

            S("q_INS_VVAL", "cyan", "after: ( <val>  // comma loops back",
              kw:  new string?[]{ _,_,_,_,_,_,_,_ },
              sym: new[]{ _,_,_,_,"q_WVAL","q_INS_LP",_,_,_ }),

            // ── DELETE ───────────────────────────────────────────────────
            S("q_DELETE", "cyan", "after: DELETE",
              kw:  new[]{ _,_,_,_,"q_DEL_FROM",_,_,_ },
              sym: new string?[]{ _,_,_,_,_,_,_,_,_ }),

            S("q_DEL_FROM", "cyan", "after: DELETE FROM",
              kw:  new string?[]{ _,_,_,_,_,_,_,_ },
              sym: new[]{ _,_,_,_,_,_,_,"q_DEL_TBL",_ }),

            S("q_DEL_TBL", "cyan", "after: DELETE FROM <tbl>",
              kw:  new[]{ _,_,_,_,_,_,"q_WHERE",_ },
              sym: new string?[]{ _,_,_,_,_,_,_,_,_ }),

            // ── UPDATE ───────────────────────────────────────────────────
            S("q_UPDATE", "cyan", "after: UPDATE",
              kw:  new string?[]{ _,_,_,_,_,_,_,_ },
              sym: new[]{ _,_,_,_,_,_,_,"q_UPD_TBL",_ }),

            S("q_UPD_TBL", "cyan", "after: UPDATE <tbl>",
              kw:  new[]{ _,_,_,_,_,_,"q_WHERE",_ },
              sym: new string?[]{ _,_,_,_,_,_,_,_,_ }),

            // ── Shared tail (merged by Myhill-Nerode) ────────────────────
            S("q_WHERE", "magenta", "after: WHERE / SET  // shared: SEL, DEL, UPD",
              kw:  new string?[]{ _,_,_,_,_,_,_,_ },
              sym: new[]{ _,_,_,_,_,_,"q_WCOL",_,_ }),

            S("q_WCOL", "magenta", "after: WHERE <col>  // shared: SEL, DEL, UPD",
              kw:  new string?[]{ _,_,_,_,_,_,_,_ },
              sym: new[]{ _,"q_WEQ",_,_,_,_,_,_,_ }),

            S("q_WEQ", "magenta", "after: WHERE <col> =  // shared: SEL, DEL, UPD",
              kw:  new string?[]{ _,_,_,_,_,_,_,_ },
              sym: new[]{ _,_,_,_,_,_,"q_WVAL",_,"q_WVAL" }),

            S("q_WVAL", "magenta", "after: = <val>  // shared: SEL, DEL, UPD, INS",
              kw:  new string?[]{ _,_,_,_,_,_,_,_ },
              sym: new[]{ _,_,SEMI,_,_,_,_,_,_ }),

            // ── Terminal ─────────────────────────────────────────────────
            S(SEMI, "green", "accept: query complete",
              new string?[8], new string?[9]),
            S(DEAD, "red",   "trap:   non-recoverable error",
              new string?[8], new string?[9]),
        };

        // ── Helpers ──────────────────────────────────────────────────────

        private static StateRow S(string name, string color, string desc,
                                  string?[] kw, string?[] sym)
            => new(name, color, desc, kw, sym);

        public static bool IsMerged(string name)
            => name is "q_WHERE" or "q_WCOL" or "q_WEQ" or "q_WVAL";

        public static bool IsAccept(string name) => name == SEMI;
        public static bool IsDead(string name) => name == DEAD;

        public static readonly string[] GroupBoundaries =
            { "q0", "q_UPD_TBL", "q_WVAL", SEMI };
    }
}