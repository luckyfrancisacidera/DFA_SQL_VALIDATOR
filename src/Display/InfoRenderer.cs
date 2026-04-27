// ============================================================
// Display/InfoRenderer.cs
// ============================================================
// Renders the educational/reference panels: alphabet, state
// definitions, and the full DFA transition table.
//
// Uses the 27-state DFA with the
//   q_WHERE  ← q_SEL_WHERE + q_DEL_WHERE + q_UPD_SET
//   q_WCOL   ← q_SEL_WCOL  + q_DEL_WCOL  + q_UPD_SCOL
//   q_WEQ    ← q_SEL_WEQ   + q_DEL_WEQ   + q_UPD_SEQ
//   q_WVAL   ← q_SEL_WVAL  + q_DEL_WVAL  + q_UPD_SVAL + q_INS_RP
// ============================================================

namespace DfaSqlValidator.Display
{
    public static class InfoRenderer
    {
        // ── Banner ────────────────────────────────────────────────────────

        public static void RenderBanner()
        {
            Console.Clear();
            ConsoleWriter.Cyan("╔══════════════════════════════════════════════════════════════╗");
            ConsoleWriter.Cyan("║         DFA-BASED SQL VALIDATOR  —  Automata Theory          ║");
            ConsoleWriter.Cyan("║                 Educational Simulation v1.0                  ║");
            ConsoleWriter.Cyan("╚══════════════════════════════════════════════════════════════╝");
            ConsoleWriter.WriteLine();
        }

        // ── Alphabet (Σ) ──────────────────────────────────────────────────

        public static void RenderAlphabet()
        {
            ConsoleWriter.Cyan("══════════════════════════════════════════════════════════════");
            ConsoleWriter.Cyan("  ALPHABET (Σ)  —  Tokens recognized by the DFA");
            ConsoleWriter.Cyan("══════════════════════════════════════════════════════════════");

            ConsoleWriter.Yellow("  SQL Keywords :");
            ConsoleWriter.White("    SELECT  FROM  WHERE  INSERT  INTO  VALUES");
            ConsoleWriter.White("    DELETE  UPDATE  SET");
            ConsoleWriter.WriteLine();

            ConsoleWriter.Yellow("  Symbols      :");
            ConsoleWriter.White("    *  (STAR)       =  (EQUALS)     ;  (SEMICOLON)");
            ConsoleWriter.White("    (  (LPAREN)     )  (RPAREN)     ,  (COMMA)");
            ConsoleWriter.WriteLine();

            ConsoleWriter.Yellow("  Generic Tokens:");
            ConsoleWriter.White("    COLUMN  — any identifier used as a column name");
            ConsoleWriter.White("    TABLE   — any identifier used as a table name");
            ConsoleWriter.White("    VALUE   — numeric literal, quoted string, or identifier");
            ConsoleWriter.WriteLine();

            ConsoleWriter.Yellow("  Special      :");
            ConsoleWriter.White("    UNKNOWN — unrecognized token (causes rejection)");
            ConsoleWriter.WriteLine();
        }

        // ── State Definitions (Q) — minimized 27-state DFA ───────────────

        public static void RenderStateDefinitions()
        {
            ConsoleWriter.Cyan("══════════════════════════════════════════════════════════════");
            ConsoleWriter.Cyan("  STATE DEFINITIONS  (Q)  —  minimized DFA  [27 states]");
            ConsoleWriter.Cyan("══════════════════════════════════════════════════════════════");

            var defs = new[]
            {
                // ── Start ────────────────────────────────────────────────
                ("q0",          "Initial / start state"),

                // ── SELECT path ──────────────────────────────────────────
                ("q_SELECT",    "Seen: SELECT"),
                ("q_SEL_COL",   "Seen: SELECT <column|*>"),
                ("q_SEL_FROM",  "Seen: SELECT … FROM"),
                ("q_SEL_TBL",   "Seen: SELECT … FROM <table>          ← pre-accept (WHERE optional)"),

                // ── INSERT path ──────────────────────────────────────────
                ("q_INSERT",    "Seen: INSERT"),
                ("q_INS_INTO",  "Seen: INSERT INTO"),
                ("q_INS_TBL",   "Seen: INSERT INTO <table>"),
                ("q_INS_VAL",   "Seen: … VALUES"),
                ("q_INS_LP",    "Seen: … VALUES ("),
                ("q_INS_VVAL",  "Seen: … ( <value>                    ← loops back on COMMA"),

                // ── DELETE path ──────────────────────────────────────────
                ("q_DELETE",    "Seen: DELETE"),
                ("q_DEL_FROM",  "Seen: DELETE FROM"),
                ("q_DEL_TBL",   "Seen: DELETE FROM <table>"),

                // ── UPDATE path ──────────────────────────────────────────
                ("q_UPDATE",    "Seen: UPDATE"),
                ("q_UPD_TBL",   "Seen: UPDATE <table>"),

                // ── Shared tail (merged by minimization) ─────────────────
                ("q_WHERE",     "Seen: … WHERE  |  … SET              ← merged: SEL/DEL/UPD"),
                ("q_WCOL",      "Seen: … WHERE <column>               ← merged: SEL/DEL/UPD"),
                ("q_WEQ",       "Seen: … WHERE <column> =             ← merged: SEL/DEL/UPD"),
                ("q_WVAL",      "Seen: … = <value>  |  … ) pre-acc.  ← merged: SEL/DEL/UPD/INS"),

                // ── Terminal ─────────────────────────────────────────────
                ("q_SEMI",      "★ ACCEPT — valid query terminated with ;"),
                ("q_DEAD",      "✗ TRAP   — unrecognized token, non-recoverable"),
            };

            ConsoleWriter.White("  Note: q_WHERE/q_WCOL/q_WEQ/q_WVAL are shared across SELECT,");
            ConsoleWriter.White("        DELETE, and UPDATE paths (Myhill-Nerode equivalence).");
            ConsoleWriter.WriteLine();

            foreach (var (state, desc) in defs)
            {
                bool isMerged = state is "q_WHERE" or "q_WCOL" or "q_WEQ" or "q_WVAL";

                if (state == "q_SEMI")
                    ConsoleWriter.Green($"  {state,-14}  {desc}");
                else if (state == "q_DEAD")
                    ConsoleWriter.Red($"  {state,-14}  {desc}");
                else if (isMerged)
                {
                    ConsoleWriter.Magenta($"  {state,-14}", nl: false);
                    ConsoleWriter.White($"  {desc}");
                }
                else
                {
                    ConsoleWriter.Cyan($"  {state,-14}", nl: false);
                    ConsoleWriter.White($"  {desc}");
                }
            }
            ConsoleWriter.WriteLine();
        }

        // ── Transition Table (δ) — 27-state DFA ────────────────
        //
        // Rows  = states in Q (ordered by path)
        // Cols  = token types in Σ (ordered by category)
        // Cells = δ(state, token)  — "——" when undefined (implicit q_DEAD)

        public static void RenderTransitionTable()
        {
            // ── 1. Column order ───────────────────────────────────────────

            // We represent each input column as a plain string label
            // so we can include both real TokenTypes and the sentinel "—"
            var inputLabels = new[]
            {
                "SELECT","INSERT","DELETE","UPDATE",
                "FROM","INTO","WHERE/SET","VALUES",
                "STAR","=",";"  ,
                "(",")",",",
                "COLUMN","TABLE","VALUE",
            };

            // ── 2. Row definitions (state name + its transition row) ──────
            //
            // Each row is (displayName, color, string?[] cells)
            // where cells[i] maps to inputLabels[i].
            // null → "——" (no transition).

            const string SEMI = "q_SEMI";
            const string DEAD = "q_DEAD";
            const string _ = null;    // undefined → ——

            var rows = new (string Name, string Color, string[] Cells)[]
            {
                // Name          Color     SELECT      INSERT      DELETE      UPDATE      FROM        INTO       WHERE/SET   VALUES      STAR       =           ;           (           )           ,           COLUMN       TABLE       VALUE
                ("q0",          "cyan",   new[]{ "q_SELECT","q_INSERT","q_DELETE","q_UPDATE",_,         _,         _,          _,          _,         _,          _,          _,          _,          _,          _,           _,          _ }),
                // ── SELECT ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
                ("q_SELECT",    "cyan",   new[]{ _,        _,         _,          _,         _,         _,         _,          _,          "q_SEL_COL",_,         _,          _,          _,          _,          "q_SEL_COL", _,          _ }),
                ("q_SEL_COL",   "cyan",   new[]{ _,        _,         _,          _,         "q_SEL_FROM",_,        _,          _,          _,         _,          _,          _,          _,          _,          _,           _,          _ }),
                ("q_SEL_FROM",  "cyan",   new[]{ _,        _,         _,          _,         _,         _,         _,          _,          _,         _,          _,          _,          _,          _,          _,           "q_SEL_TBL",_ }),
                ("q_SEL_TBL",   "cyan",   new[]{ _,        _,         _,          _,         _,         _,         "q_WHERE",  _,          _,         _,          SEMI,       _,          _,          _,          _,           _,          _ }),
                // ── INSERT ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
                ("q_INSERT",    "cyan",   new[]{ _,        _,         _,          _,         _,         "q_INS_INTO",_,         _,          _,         _,          _,          _,          _,          _,          _,           _,          _ }),
                ("q_INS_INTO",  "cyan",   new[]{ _,        _,         _,          _,         _,         _,         _,          _,          _,         _,          _,          _,          _,          _,          _,           "q_INS_TBL",_ }),
                ("q_INS_TBL",   "cyan",   new[]{ _,        _,         _,          _,         _,         _,         _,          "q_INS_VAL",_,         _,          _,          _,          _,          _,          _,           _,          _ }),
                ("q_INS_VAL",   "cyan",   new[]{ _,        _,         _,          _,         _,         _,         _,          _,          _,         _,          _,          "q_INS_LP", _,          _,          _,           _,          _ }),
                ("q_INS_LP",    "cyan",   new[]{ _,        _,         _,          _,         _,         _,         _,          _,          _,         _,          _,          _,          _,          _,          "q_INS_VVAL","_",        "q_INS_VVAL" }),
                ("q_INS_VVAL",  "cyan",   new[]{ _,        _,         _,          _,         _,         _,         _,          _,          _,         _,          _,          _,          "q_WVAL",   "q_INS_LP", _,           _,          _ }),
                // ── DELETE ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
                ("q_DELETE",    "cyan",   new[]{ _,        _,         _,          _,         "q_DEL_FROM",_,        _,          _,          _,         _,          _,          _,          _,          _,          _,           _,          _ }),
                ("q_DEL_FROM",  "cyan",   new[]{ _,        _,         _,          _,         _,         _,         _,          _,          _,         _,          _,          _,          _,          _,          _,           "q_DEL_TBL",_ }),
                ("q_DEL_TBL",   "cyan",   new[]{ _,        _,         _,          _,         _,         _,         "q_WHERE",  _,          _,         _,          _,          _,          _,          _,          _,           _,          _ }),
                // ── UPDATE ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
                ("q_UPDATE",    "cyan",   new[]{ _,        _,         _,          _,         _,         _,         _,          _,          _,         _,          _,          _,          _,          _,          _,           "q_UPD_TBL",_ }),
                ("q_UPD_TBL",   "cyan",   new[]{ _,        _,         _,          _,         _,         _,         "q_WHERE",  _,          _,         _,          _,          _,          _,          _,          _,           _,          _ }),
                // ── Shared tail (merged) ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
                ("q_WHERE",     "magenta",new[]{ _,        _,         _,          _,         _,         _,         _,          _,          _,         _,          _,          _,          _,          _,          "q_WCOL",    _,          _ }),
                ("q_WCOL",      "magenta",new[]{ _,        _,         _,          _,         _,         _,         _,          _,          _,         "q_WEQ",    _,          _,          _,          _,          _,           _,          _ }),
                ("q_WEQ",       "magenta",new[]{ _,        _,         _,          _,         _,         _,         _,          _,          _,         _,          _,          _,          _,          _,          "q_WVAL",    _,          "q_WVAL" }),
                ("q_WVAL",      "magenta",new[]{ _,        _,         _,          _,         _,         _,         _,          _,          _,         _,          SEMI,       _,          _,          _,          _,           _,          _ }),
                // ── Terminal ────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
                (SEMI,          "green",  new string[17]),
                (DEAD,          "red",    new string[17]),
            };

            // Fix q_INS_LP COLUMN cell (used "_" as placeholder above — correct it)
            // row index 9 = q_INS_LP, col index 15 = COLUMN
            rows[9].Cells[15] = "q_INS_VVAL";

            // ── 3. Measure column widths ──────────────────────────────────

            const int StateColW = 12;
            int[] colW = new int[inputLabels.Length];
            for (int c = 0; c < inputLabels.Length; c++)
            {
                colW[c] = Math.Max(7, inputLabels[c].Length);
                foreach (var row in rows)
                {
                    string cell = row.Cells[c] ?? "——";
                    if (cell.Length > colW[c]) colW[c] = cell.Length;
                }
            }

            // ── 4. Border helpers ─────────────────────────────────────────

            void Rule(char l, char m, char r, char f)
            {
                ConsoleWriter.Yellow("  " + l + new string(f, StateColW + 2), nl: false);
                for (int c = 0; c < inputLabels.Length; c++)
                    ConsoleWriter.Yellow(m + new string(f, colW[c] + 2), nl: false);
                ConsoleWriter.Yellow(r.ToString());
            }

            void CellColor(string value, string rowColor)
            {
                string text = (value ?? "——").PadRight(colW[Array.IndexOf(inputLabels, value) < 0 ? 0 : 0]);
                // color by destination
                if (value == null)
                    ConsoleWriter.Gray("——", nl: false);
                else if (value == SEMI)
                    ConsoleWriter.Green(value, nl: false);
                else if (value == DEAD)
                    ConsoleWriter.Red(value, nl: false);
                else if (value.StartsWith("q_WHERE") || value.StartsWith("q_WCOL") ||
                         value.StartsWith("q_WEQ") || value.StartsWith("q_WVAL"))
                    ConsoleWriter.Magenta(value, nl: false);
                else
                    ConsoleWriter.Cyan(value, nl: false);
            }

            // ── 5. Render ─────────────────────────────────────────────────

            ConsoleWriter.Cyan("══════════════════════════════════════════════════════════════");
            ConsoleWriter.Cyan("  DFA TRANSITION TABLE   δ: Q × Σ → Q   [minimized, 27 states]");
            ConsoleWriter.Cyan("  rows = states │ columns = input tokens │ cells = δ(q, σ)");
            ConsoleWriter.Cyan("  —— = undefined (implicit q_DEAD at runtime)");
            ConsoleWriter.Magenta("  Magenta rows = shared tail states (merged by Myhill-Nerode)");
            ConsoleWriter.Cyan("══════════════════════════════════════════════════════════════");
            ConsoleWriter.WriteLine();

            Rule('┌', '┬', '┐', '─');

            // Header
            ConsoleWriter.Yellow($"  │ {"State",-StateColW}", nl: false);
            for (int c = 0; c < inputLabels.Length; c++)
                ConsoleWriter.Yellow($" │ {inputLabels[c].PadRight(colW[c])}", nl: false);
            ConsoleWriter.Yellow(" │");

            Rule('├', '┼', '┤', '─');

            // Data rows
            string[] groupBoundaries = { "q0", "q_UPD_TBL", "q_WVAL", "q_SEMI" };

            for (int r = 0; r < rows.Length; r++)
            {
                var (name, color, cells) = rows[r];

                // State label
                ConsoleWriter.Yellow("  │ ", nl: false);
                string label = name == SEMI ? "★ " + name : name == DEAD ? "✗ " + name : name;
                Action<string, bool> stateWriter = color switch
                {
                    "green" => ConsoleWriter.Green,
                    "red" => ConsoleWriter.Red,
                    "magenta" => ConsoleWriter.Magenta,
                    _ => ConsoleWriter.Cyan,
                };
                stateWriter($"{label,-StateColW}", false);

                // Cells
                for (int c = 0; c < inputLabels.Length; c++)
                {
                    ConsoleWriter.Yellow(" │ ", nl: false);
                    string val = cells[c];
                    string padded = (val ?? "——").PadRight(colW[c]);

                    if (val == null)
                        ConsoleWriter.Gray(padded, nl: false);
                    else if (val == SEMI)
                        ConsoleWriter.Green(padded, nl: false);
                    else if (val == DEAD)
                        ConsoleWriter.Red(padded, nl: false);
                    else if (val is "q_WHERE" or "q_WCOL" or "q_WEQ" or "q_WVAL")
                        ConsoleWriter.Magenta(padded, nl: false);
                    else
                        ConsoleWriter.Cyan(padded, nl: false);
                }
                ConsoleWriter.Yellow(" │");

                // Group separator
                if (Array.IndexOf(groupBoundaries, name) >= 0 && r < rows.Length - 1)
                    Rule('├', '┼', '┤', '─');
            }

            Rule('└', '┴', '┘', '─');

            ConsoleWriter.WriteLine();
            ConsoleWriter.Green("  ★  Accept state  F = { q_SEMI }");
            ConsoleWriter.Yellow("     Start  state  q0");
            ConsoleWriter.Magenta("  ◆  Shared states q_WHERE / q_WCOL / q_WEQ / q_WVAL");
            ConsoleWriter.WriteLine();
        }

        // ── Prompt ────────────────────────────────────────────────────────

        public static void RenderPrompt(bool debugMode)
        {
            ConsoleWriter.Write("[", ConsoleColor.DarkGray);
            if (debugMode)
            {
                ConsoleWriter.Write("DEBUG", ConsoleColor.Magenta);
                ConsoleWriter.Write("|", ConsoleColor.DarkGray);
            }
            ConsoleWriter.Write("DFA-SQL", ConsoleColor.Cyan);
            ConsoleWriter.Write("] ", ConsoleColor.DarkGray);
            ConsoleWriter.Write("> ", ConsoleColor.White);
        }
    }
}