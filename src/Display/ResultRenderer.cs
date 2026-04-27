// ============================================================
// Display/ResultRenderer.cs
// ============================================================
// Renders the final ACCEPTED / REJECTED verdict banner.
//
// Responsibility (single):
//   Take a ValidationResult and print one clear outcome box.
// ============================================================

using DfaSqlValidator.Models;

namespace DfaSqlValidator.Display
{
    /// <summary>
    /// Renders the accept/reject verdict for a validation run.
    /// </summary>
    public static class ResultRenderer
    {
        public static void Render(ValidateResult result)
        {
            if (result.Accepted)
                RenderAccepted();
            else
                RenderRejected(result.ErrorMessage);
        }

        private static void RenderAccepted()
        {
            ConsoleWriter.Green("  ╔══════════════════════════════════════╗");
            ConsoleWriter.Green("  ║   ✓  ACCEPTED  —  Valid SQL Query    ║");
            ConsoleWriter.Green("  ╚══════════════════════════════════════╝");
            ConsoleWriter.WriteLine();
        }

        private static void RenderRejected(string errorMessage)
        {
            ConsoleWriter.Red("  ╔═════════════════════════════════════╗");
            ConsoleWriter.Red("  ║   ✗  REJECTED  —  Invalid Query     ║");
            ConsoleWriter.Red("  ╚═════════════════════════════════════╝");
            ConsoleWriter.Red($"  Reason: {errorMessage}");
            ConsoleWriter.WriteLine();
        }
    }
}
