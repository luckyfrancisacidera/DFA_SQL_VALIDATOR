// ============================================================
// Display/TraceRenderer.cs
// ============================================================
// Renders a List<TraceStep> as a numbered, step-by-step
// transition log — one line per transition.
// ============================================================

using System;
using System.Collections.Generic;
using DfaSqlValidator.Models;

namespace DfaSqlValidator.Display
{
    public static class TraceRenderer
    {
        public static void Render(List<TraceStep> trace, bool debugMode)
        {
            ConsoleWriter.Cyan("──────────────── STEP BY STEP - DFA TRACE ────────────────");

            if (trace.Count == 0)
            {
                ConsoleWriter.Yellow("  (no transitions — input was empty)");
                ConsoleWriter.WriteLine();
                return;
            }

            // Measure widths for alignment
            int stepW = trace.Count.ToString().Length;   // digits in highest step number
            int fromW = 0;
            int tokenW = 0;

            foreach (var s in trace)
            {
                if (s.From.ToString().Length > fromW) fromW = s.From.ToString().Length;
                string tok = debugMode ? $"{s.Input}:'{s.Lexeme}'" : s.Lexeme;
                if (tok.Length > tokenW) tokenW = tok.Length;
            }

            // Column header
            string stepHdr = "Step".PadRight(4 + stepW);
            string fromHdr = "From state".PadRight(fromW);
            string tokenHdr = "Token".PadRight(tokenW);
            string toHdr = "Next state";

            ConsoleWriter.Yellow($"  {"".PadRight(4 + stepW)}   {fromHdr}   {"".PadRight(tokenW + 6)}   {toHdr}");
            ConsoleWriter.Yellow($"  {"".PadLeft(4 + stepW, '─')}   {"".PadLeft(fromW, '─')}   {"".PadLeft(tokenW + 6, '─')}   {"".PadLeft(12, '─')}");

            // One row per step
            for (int i = 0; i < trace.Count; i++)
            {
                var step = trace[i];

                string stepLabel = $"Step {(i + 1).ToString().PadLeft(stepW)}";
                string fromStr = step.From.ToString().PadRight(fromW);
                string tokStr = debugMode
                    ? $"{step.Input}:'{step.Lexeme}'"
                    : step.Lexeme;
                string arrow = $"──[{tokStr.PadRight(tokenW)}]──▶";

                // "Step N │"
                ConsoleWriter.White($"  {stepLabel} ", nl: false);
                ConsoleWriter.Yellow("│ ", nl: false);

                // From state
                ConsoleWriter.Cyan(fromStr, nl: false);
                ConsoleWriter.White("   ", nl: false);

                // Arrow + token
                ConsoleWriter.White($"──[", nl: false);
                if (debugMode)
                    ConsoleWriter.Magenta($"{step.Input}", nl: false);
                else
                    ConsoleWriter.Magenta(step.Lexeme, nl: false);

                if (debugMode)
                    ConsoleWriter.Gray($":'{step.Lexeme}'", nl: false);

                // Pad so "──▶" always lines up
                int padNeeded = tokenW - (debugMode
                    ? $"{step.Input}:'{step.Lexeme}'".Length
                    : step.Lexeme.Length);
                ConsoleWriter.White(new string(' ', Math.Max(0, padNeeded)), nl: false);
                ConsoleWriter.White("]──▶  ", nl: false);

                // Next state (color-coded)
                if (step.IsError)
                    ConsoleWriter.Red(step.To.ToString());
                else if (step.To == DfaState.q_SEMI)
                    ConsoleWriter.Green(step.To.ToString());
                else
                    ConsoleWriter.Cyan(step.To.ToString());
            }

            ConsoleWriter.WriteLine();
        }

        /// <summary>Debug-mode tokenization dump.</summary>
        public static void RenderTokens(List<Token> tokens)
        {
            ConsoleWriter.Cyan("──────────────── Tokenization Result ─────────────────");
            Console.Write("  ");
            foreach (var t in tokens)
            {
                ConsoleWriter.Magenta(t.Type.ToString(), nl: false);
                ConsoleWriter.Gray($"('{t.Lexeme}') ", nl: false);
            }
            Console.ResetColor();
            Console.WriteLine();
            ConsoleWriter.WriteLine();
        }
    }
}