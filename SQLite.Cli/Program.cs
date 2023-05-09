using SQLite;
using SQLite.Cli.Services;

var context = new DbContext(
    new ConsoleInputService(),
    new ConsoleOutputService(),
    new ConsoleEnvironmentService()
);

DbContext.SetContext(context);

if (!args.Any())
{
    Console.WriteLine("Must supply a database filename.\n");
    DbContext.EnvironmentService.Exit(1);
}

var filename = args.First();

var repl = new Repl(context);

await repl.RunAsync(filename);