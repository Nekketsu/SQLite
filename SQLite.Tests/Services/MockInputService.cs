using SQLite.Services;

namespace SQLite.Tests.Services;

public class MockInputService : IInputService
{
    public string[] Script { get; }

    private readonly IEnumerator<string> enumerator;

    public MockInputService(string[] script)
    {
        Script = script;
        enumerator = script.AsEnumerable().GetEnumerator();
    }

    public string? ReadLine()
    {
        if (!enumerator.MoveNext())
        {
            return null;
        }

        return enumerator.Current;
    }
}
