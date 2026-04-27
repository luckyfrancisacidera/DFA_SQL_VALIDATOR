// ============================================================
// Cli/CommandHandler.cs
// ============================================================
// Parses raw console input and dispatches to the correct
// handler — either a built-in command or the SQL validator.
// ============================================================

using DfaSqlValidator.Display;

namespace DfaSqlValidator.Cli
{
    /// <summary>
    /// Dispatches user input to built-in commands or the SQL validator.
    /// Returns false when the application should exit.
    /// </summary>
    public sealed class CommandHandler
    {
        private readonly SqlValidator _validator;
        private bool _debugMode = false;

        public CommandHandler(SqlValidator validator)
        {
            _validator = validator;
        }

        /// <summary>
        /// Handle one line of user input.
        /// </summary>
        /// <returns>True to keep the REPL running; false to exit.</returns>
        public bool Handle(string raw)
        {
            string input = raw.Trim();
            if (input.Length == 0) return true;

            string lower = input.ToLower();

            return lower switch
            {
                "exit" or "quit" => Exit(),
                "clear" => Clear(),
                "help" => ShowHelp(),
                "alphabet" => ShowAlphabet(),
                "table" => ShowTable(),
                "states" => ShowStates(),
                "debug on" => SetDebug(true),
                "debug off" => SetDebug(false),
                _ => Validate(input)
            };
        }

        // ── Command implementations ───────────────────────────────────────

        private static bool Exit()
        {
            ConsoleWriter.Cyan("\n  Goodbye! — Automata simulation ended.");
            return false;
        }

        private static bool Clear()
        {
            InfoRenderer.RenderBanner();
            return true;
        }

        private static bool ShowHelp()
        {
            HelpRenderer.Render();
            return true;
        }

        private static bool ShowAlphabet()
        {
            InfoRenderer.RenderAlphabet();
            return true;
        }

        private static bool ShowTable()
        {
            InfoRenderer.RenderTransitionTable();
            return true;
        }

        private static bool ShowStates()
        {
            InfoRenderer.RenderStateDefinitions();
            return true;
        }

        private bool SetDebug(bool on)
        {
            _debugMode = on;
            if (on)
                ConsoleWriter.Yellow("  DEBUG MODE ENABLED — token types and all trace labels shown.");
            else
                ConsoleWriter.Yellow("  Debug mode disabled.");
            ConsoleWriter.WriteLine();
            return true;
        }

        private bool Validate(string input)
        {
            var result = _validator.Validate(input);

            if (_debugMode)
                TraceRenderer.RenderTokens(result.Tokens);

            TraceRenderer.Render(result.Trace, _debugMode);
            ResultRenderer.Render(result);
            return true;
        }

        // ── Prompt ────────────────────────────────────────────────────────

        public void PrintPrompt() => InfoRenderer.RenderPrompt(_debugMode);
    }
}
