// ============================================================
// Display/HelpRenderer.cs
// ============================================================
// Renders the help screen shown when the user types "help".
// ============================================================

namespace DfaSqlValidator.Display
{
    /// <summary>
    /// Renders the help and usage documentation panel.
    /// </summary>
    public static class HelpRenderer
    {
        public static void Render()
        {
            ConsoleWriter.Cyan("══════════════════════════════════════════════════════════════");
            ConsoleWriter.Cyan("  HELP — DFA SQL Validator");
            ConsoleWriter.Cyan("══════════════════════════════════════════════════════════════");

            ConsoleWriter.Yellow("  COMMANDS:");
            ConsoleWriter.White("    help          — Show this help screen");
            ConsoleWriter.White("    clear         — Clear the console");
            ConsoleWriter.White("    alphabet      — Show the DFA alphabet (Σ)");
            ConsoleWriter.White("    table         — Show full DFA transition table");
            ConsoleWriter.White("    states        — Show state definitions (Q)");
            ConsoleWriter.White("    debug on      — Enable debug mode");
            ConsoleWriter.White("    debug off     — Disable debug mode");
            ConsoleWriter.White("    exit / quit   — Exit the program");
            ConsoleWriter.WriteLine();

            ConsoleWriter.Yellow("  SUPPORTED SQL PATTERNS:");
            ConsoleWriter.Green("    SELECT <col|*> FROM <table> ;");
            ConsoleWriter.Green("    SELECT <col|*> FROM <table> WHERE <col> = <val> ;");
            ConsoleWriter.Green("    INSERT INTO <table> VALUES ( <val> ) ;");
            ConsoleWriter.Green("    INSERT INTO <table> VALUES ( <val>, <val>, ... ) ;");
            ConsoleWriter.Green("    DELETE FROM <table> WHERE <col> = <val> ;");
            ConsoleWriter.Green("    UPDATE <table> SET <col> = <val> ;");
            ConsoleWriter.WriteLine();

            ConsoleWriter.Yellow("  EXAMPLE QUERIES:");
            ConsoleWriter.White("    SELECT name FROM users ;");
            ConsoleWriter.White("    SELECT * FROM products WHERE price = 100 ;");
            ConsoleWriter.White("    INSERT INTO orders VALUES ( 1 ) ;");
            ConsoleWriter.White("    INSERT INTO orders VALUES ( 1, 2, 3 ) ;");
            ConsoleWriter.White("    DELETE FROM customers WHERE id = 5 ;");
            ConsoleWriter.White("    UPDATE employees SET salary = 50000 ;");
            ConsoleWriter.WriteLine();

            ConsoleWriter.Yellow("  INPUT FORMAT:");
            ConsoleWriter.White("    • Separate all tokens with spaces (including ; ( ) = , *)");
            ConsoleWriter.White("    • Keywords are case-insensitive");
            ConsoleWriter.White("    • Identifiers and values can be any word/number");
            ConsoleWriter.White("    • Every valid query must end with ;");
            ConsoleWriter.WriteLine();

            ConsoleWriter.Yellow("  DEBUG MODE  (toggle with 'debug on' / 'debug off'):");
            ConsoleWriter.White("    • Shows token type labels alongside lexemes in the trace");
            ConsoleWriter.White("    • Shows full tokenization result before the trace");
            ConsoleWriter.WriteLine();

            ConsoleWriter.Yellow("  COLOR LEGEND:");
            ConsoleWriter.Green("    Green   → ACCEPTED / accept state");
            ConsoleWriter.Red("    Red     → REJECTED / dead state / error");
            ConsoleWriter.Yellow("    Yellow  → Current state / warnings");
            ConsoleWriter.Cyan("    Cyan    → System messages / state names");
            ConsoleWriter.White("    White   → Normal output");
            ConsoleWriter.WriteLine();
        }
    }
}
