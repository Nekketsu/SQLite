namespace SQLite.Services;

public interface IOutputService
{
    void Write(string? value);
    void WriteLine(string? value);
}
