namespace SQLite;

public class Page
{
    public const int Size = 4096;
    byte[] page = new byte[Size];

    public Span<byte> AsSpan(int start) => page.AsSpan(start);
}
