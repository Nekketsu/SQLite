using SQLite.Services;

namespace SQLite.Cli.Services;

public class ConsoleOutputService : IOutputService
{
    public void Write(string? value) => Console.Write(value);

    public void WriteLine(string? value) => Console.WriteLine(value);
}
