namespace SQLite;

public class Table
{
    const int maxPages = 100;
    const int rowsPerPage = Page.Size / Row.Size;
    const int maxRows = rowsPerPage * maxPages;

    public Page[] Pages = new Page[maxPages];
    public int NumRows { get; private set; }

    public bool Insert(Row row)
    {
        if (NumRows >= maxRows)
        {
            return false;
        }

        var rowSlot = RowSlot(NumRows);
        row.Serialize(rowSlot);

        NumRows++;

        return true;
    }

    public Row Select(int rowNum)
    {
        var rowSlot = RowSlot(rowNum);
        var row = Row.Deserialize(rowSlot);

        return row;
    }

    private Span<byte> RowSlot(int rowNum)
    {
        var pageNum = rowNum / rowsPerPage;
        var page = Pages[pageNum];
        if (page is null)
        {
            page = Pages[pageNum] = new Page();
        }
        var rowOffset = rowNum % rowsPerPage;
        var byteOffset = rowOffset * Row.Size;

        return page.AsSpan(byteOffset);
    }
}
