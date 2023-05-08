using SQLite;
using SQLite.Cli.Services;

var environment = new ConsoleEnvironmentService();
var input = new ConsoleInputService();
var output = new ConsoleOutputService();

var repl = new Repl(environment, input, output);

repl.Run();