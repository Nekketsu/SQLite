using SQLite.Services;

namespace SQLite.NopContext;

internal class NopOutputService : IOutputService
{
    public void Write(string? value) { }

    public void WriteLine(string? value) { }
}
