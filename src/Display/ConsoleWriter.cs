// ============================================================
// Display/ConsoleWriter.cs
// ============================================================
// Thin wrapper over Console that provides color-safe write
// helpers used by every other display component
// ============================================================

namespace DfaSqlValidator.Display
{
    /// <summary>
    /// Color-safe console write primitives.
    /// All writes reset the color afterwards.
    /// </summary>
    public static class ConsoleWriter
    {
        public static void Write(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }

        public static void WriteLine(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void WriteLine() => Console.WriteLine();

        // ── Named color shortcuts ─────────────────────────────────────────
        public static void Cyan(string s, bool nl = true) => Emit(s, ConsoleColor.Cyan, nl);
        public static void Green(string s, bool nl = true) => Emit(s, ConsoleColor.Green, nl);
        public static void Red(string s, bool nl = true) => Emit(s, ConsoleColor.Red, nl);
        public static void Yellow(string s, bool nl = true) => Emit(s, ConsoleColor.Yellow, nl);
        public static void White(string s, bool nl = true) => Emit(s, ConsoleColor.White, nl);
        public static void Gray(string s, bool nl = true) => Emit(s, ConsoleColor.Gray, nl);
        public static void Magenta(string s, bool nl = true) => Emit(s, ConsoleColor.Magenta, nl);

        private static void Emit(string s, ConsoleColor c, bool nl)
        {
            Console.ForegroundColor = c;
            if (nl) Console.WriteLine(s);
            else Console.Write(s);
            Console.ResetColor();
        }
    }
}
