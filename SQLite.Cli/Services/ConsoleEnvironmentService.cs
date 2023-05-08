using SQLite.Services;

namespace SQLite.Cli.Services;

public class ConsoleEnvironmentService : IEnvironmentService
{
    public void Exit(int exitCode) => Environment.Exit(exitCode);
}
