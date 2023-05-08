using SQLite.Services;
using System.Text;

namespace SQLite.Tests.Services;

public class MockOutputService : IOutputService
{
    private readonly StringBuilder stringBuilder = new StringBuilder();
    private readonly List<string> output = new List<string> { string.Empty };

    public string[] Output => output.ToArray();

    public void Write(string? value)
    {
        stringBuilder.Append(value);
        output[^1] = stringBuilder.ToString();
    }

    public void WriteLine(string? value)
    {
        Write(value);

        stringBuilder.Clear();
        output.Add(string.Empty);
    }
}
