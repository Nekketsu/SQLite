namespace SQLite;

public class Table
{
    public const int MaxPages = 100;
    public const int RowsPerPage = Page.Size / Row.Size;
    public int MaxRows = RowsPerPage * MaxPages;

    public Pager Pager { get; }
    public int NumRows { get; set; }

    private Table(string fileName)
    {
        Pager = Pager.Open(fileName);
        NumRows = (int)(Pager.FileLength / Row.Size);
    }

    public static Table Open(string fileName) => new Table(fileName);

    public async Task CloseAsync()
    {
        await Pager.FlushAsync(NumRows);
        Pager.Close();
    }

    public Cursor Start() => new Cursor(this);

    public Cursor End() => new Cursor(this, NumRows);
}
