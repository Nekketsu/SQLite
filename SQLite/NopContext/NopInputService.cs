using SQLite.Services;

namespace SQLite.NopContext;

internal class NopInputService : IInputService
{
    public string? ReadLine() => null;
}
