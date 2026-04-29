// ============================================================
// Display/InfoRenderer.cs
// ============================================================
// Renders the educational/reference panels: alphabet, state
// definitions, and the full DFA transition table.
//
// Data lives in DfaData.cs — this file is pure rendering logic.
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
            SectionHeader("ALPHABET (Σ)  —  Tokens recognized by the DFA");

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

        // ── State Definitions (Q) ─────────────────────────────────────────

        public static void RenderStateDefinitions()
        {
            SectionHeader("STATE DEFINITIONS  (Q)  — DFA");
            ConsoleWriter.WriteLine();

            foreach (var row in DfaData.States)
            {
                var writer = WriterFor(row.Color);
                writer($"  {row.Name,-14}", false);
                ConsoleWriter.White($"  {row.Desc}");
            }

            ConsoleWriter.WriteLine();
        }

        // ── Transition Table (δ) ──────────────────────────────────────────

        // ── Transition Table (δ) ──────────────────────────────────────────

        public static void RenderTransitionTable()
        {
            SectionHeader("DFA TRANSITION TABLE   δ: Q × Σ → Q ");
            ConsoleWriter.Cyan("  rows = states │ columns = input tokens │ cells = δ(q, σ)");
            ConsoleWriter.Cyan("  —— = undefined (implicit q_DEAD at runtime)");
            ConsoleWriter.Cyan("══════════════════════════════════════════════════════════════");
            ConsoleWriter.WriteLine();

            RenderSubTable(
                subtitle: "TABLE 1 OF 2 — SQL Keywords",
                getLabels: () => DfaData.KeywordLabels,
                getCells: row => row.KwCells
            );

            ConsoleWriter.WriteLine();

            RenderSubTable(
                subtitle: "TABLE 2 OF 2 — Symbols & Generic Tokens",
                getLabels: () => DfaData.SymbolLabels,
                getCells: row => row.SymCells
            );

            ConsoleWriter.WriteLine();
            ConsoleWriter.Green("  ★  Accept state  F = { q_SEMI }");
            ConsoleWriter.Yellow("     Start  state  q0");
            ConsoleWriter.Magenta("  ◆  Shared states q_WHERE / q_WCOL / q_WEQ / q_WVAL");
            ConsoleWriter.WriteLine();
        }

        private static void RenderSubTable(
            string subtitle,
            Func<string[]> getLabels,
            Func<StateRow, string?[]> getCells)
        {
            ConsoleWriter.Yellow($"  ── {subtitle} ──");
            ConsoleWriter.WriteLine();

            var labels = getLabels();
            var rows = DfaData.States;

            const int StateColW = 12;
            int[] colW = new int[labels.Length];
            for (int c = 0; c < labels.Length; c++)
            {
                colW[c] = Math.Max(7, labels[c].Length);
                foreach (var row in rows)
                {
                    int len = (getCells(row)[c] ?? "——").Length;
                    if (len > colW[c]) colW[c] = len;
                }
            }

            Rule('┌', '┬', '┐', '─', StateColW, colW);

            // Header
            ConsoleWriter.Yellow($"  │ {"State",-StateColW}", nl: false);
            foreach (var (label, w) in Zip(labels, colW))
                ConsoleWriter.Yellow($" │ {label.PadRight(w)}", nl: false);
            ConsoleWriter.Yellow(" │");

            Rule('├', '┼', '┤', '─', StateColW, colW);

            // Data rows
            for (int r = 0; r < rows.Length; r++)
            {
                var row = rows[r];
                var cells = getCells(row);

                ConsoleWriter.Yellow("  │ ", nl: false);
                string stateLabel = DfaData.IsAccept(row.Name) ? "★ " + row.Name
                                  : DfaData.IsDead(row.Name) ? "✗ " + row.Name
                                  : row.Name;
                WriterFor(row.Color)($"{stateLabel,-StateColW}", false);

                for (int c = 0; c < labels.Length; c++)
                {
                    ConsoleWriter.Yellow(" │ ", nl: false);
                    string? val = cells[c];
                    string padded = (val ?? "q_DEAD").PadRight(colW[c]);

                    if (val == null) ConsoleWriter.Red(padded, nl: false);
                    else if (DfaData.IsAccept(val)) ConsoleWriter.Green(padded, nl: false);
                    else if (DfaData.IsDead(val)) ConsoleWriter.Red(padded, nl: false);
                    else if (DfaData.IsMerged(val)) ConsoleWriter.Magenta(padded, nl: false);
                    else ConsoleWriter.Cyan(padded, nl: false);
                }
                ConsoleWriter.Yellow(" │");

                if (Array.IndexOf(DfaData.GroupBoundaries, row.Name) >= 0 && r < rows.Length - 1)
                    Rule('├', '┼', '┤', '─', StateColW, colW);
            }

            Rule('└', '┴', '┘', '─', StateColW, colW);
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

        // ── Private helpers ───────────────────────────────────────────────

        private static void SectionHeader(string title)
        {
            ConsoleWriter.Cyan("══════════════════════════════════════════════════════════════");
            ConsoleWriter.Cyan($"  {title}");
            ConsoleWriter.Cyan("══════════════════════════════════════════════════════════════");
        }

        private static void Rule(char l, char m, char r, char f, int stateColW, int[] colW)
        {
            ConsoleWriter.Yellow("  " + l + new string(f, stateColW + 2), nl: false);
            foreach (int w in colW)
                ConsoleWriter.Yellow(m + new string(f, w + 2), nl: false);
            ConsoleWriter.Yellow(r.ToString());
        }

        // Maps color name 
        private static Action<string, bool> WriterFor(string color) => color switch
        {
            "green" => ConsoleWriter.Green,
            "red" => ConsoleWriter.Red,
            "magenta" => ConsoleWriter.Magenta,
            _ => ConsoleWriter.Cyan,
        };

        // Pairs two same-length sequences without LINQ overhead.
        private static IEnumerable<(T1, T2)> Zip<T1, T2>(T1[] a, T2[] b)
        {
            for (int i = 0; i < a.Length; i++) yield return (a[i], b[i]);
        }
    }
}