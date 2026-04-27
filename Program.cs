// ============================================================
// Program.cs — Entry point
// ============================================================
// Constructs the object graph and starts the REPL.
//
// Dependency graph:
//   Repl → CommandHandler → SqlValidator → Tokenizer + DfaEngine
// ============================================================

using DfaSqlValidator.Cli;

var validator = new SqlValidator();
var handler = new CommandHandler(validator);
var repl = new Repl(handler);

repl.Run();
