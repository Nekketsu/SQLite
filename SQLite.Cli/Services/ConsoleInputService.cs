using SQLite.Services;

namespace SQLite.Cli.Services;

public class ConsoleInputService : IInputService
{
    public string? ReadLine() => Console.ReadLine();
}
