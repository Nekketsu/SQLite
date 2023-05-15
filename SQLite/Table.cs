namespace SQLite;

public class Table
{
    public const int MaxPages = 100;

    public Pager Pager { get; }
    public uint RootPageNum { get; set; }

    public Table(Pager pager)
    {
        Pager = pager;
        RootPageNum = 0;
    }

    public async Task<Cursor> StartAsync() => await Cursor.StartAsync(this);

    public async Task<Cursor> EndAsync() => await Cursor.EndAsync(this);
}
