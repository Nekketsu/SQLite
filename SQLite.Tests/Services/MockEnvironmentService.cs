using SQLite.Services;

namespace SQLite.Tests.Services;

public class MockEnvironmentService : IEnvironmentService
{
    public void Exit(int exitCode)
    {
        throw new ExitException();
    }
}
