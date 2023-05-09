using SQLite.Services;

namespace SQLite.NopContext;

internal class NopEnvironmentService : IEnvironmentService
{
    public void Exit(int exitCode) { }
}
