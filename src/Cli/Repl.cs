// ============================================================
// Cli/Repl.cs
// ============================================================
// The Read-Eval-Print Loop — the application's entry point
// for user interaction.
// ============================================================

using DfaSqlValidator.Display;

namespace DfaSqlValidator.Cli
{
    /// <summary>
    /// Interactive console loop.
    /// </summary>
    public sealed class Repl
    {
        private readonly CommandHandler _handler;

        public Repl(CommandHandler handler)
        {
            _handler = handler;
        }

        /// <summary>
        /// Start the REPL.  Blocks until the user exits.
        /// </summary>
        public void Run()
        {
            // Startup sequence
            InfoRenderer.RenderBanner();
            InfoRenderer.RenderAlphabet();
            ConsoleWriter.Cyan("  Type 'help' for commands and examples.");
            ConsoleWriter.Cyan("  Type 'table' to see the full DFA transition table.");
            ConsoleWriter.Cyan("  Type 'exit' or 'quit' to leave.");
            ConsoleWriter.WriteLine();

            // Main loop
            while (true)
            {
                _handler.PrintPrompt();

                string raw = Console.ReadLine()!;
                if (raw == null) break;  // EOF (Ctrl+Z / Ctrl+D)

                bool keepRunning = _handler.Handle(raw);
                if (!keepRunning) break;
            }
        }
    }
}
