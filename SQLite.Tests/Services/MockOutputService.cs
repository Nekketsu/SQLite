using SQLite.Services;
using System.Text;

namespace SQLite.Tests.Services;

public class MockOutputService : IOutputService
{
    private readonly StringBuilder stringBuilder = new StringBuilder();
    private readonly List<string> output = new List<string>();

    public string[] Output => output.ToArray();

    public void Write(string? value)
    {
        var shouldAddLine = stringBuilder.Length == 0;

        stringBuilder.Append(value);
        if (shouldAddLine)
        {
            output.Add(stringBuilder.ToString());
        }
        else
        {
            output[^1] = stringBuilder.ToString();
        }
    }

    public void WriteLine(string? value)
    {
        Write(value);

        stringBuilder.Clear();
    }
}
