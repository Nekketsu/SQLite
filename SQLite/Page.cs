namespace SQLite;

public class Page
{
    public const int Size = 4096;
    public byte[] Buffer { get; } = new byte[Size];
}
