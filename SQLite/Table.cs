using SQLite.Exceptions;

namespace SQLite;

public class Table
{
    public const int MaxPages = 100;
    public const int RowsPerPage = Page.Size / Row.Size;
    const int maxRows = RowsPerPage * MaxPages;

    public Pager Pager { get; }
    public int NumRows { get; private set; }

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

    public async Task InsertAsync(Row row)
    {
        if (NumRows >= maxRows)
        {
            throw new TableFullException();
        }

        var rowSlot = await RowSlotAsync(NumRows);
        row.Serialize(rowSlot);

        NumRows++;
    }

    public async Task<Row> SelectAsync(int rowNum)
    {
        var rowSlot = await RowSlotAsync(rowNum);
        var row = Row.Deserialize(rowSlot);

        return row;
    }

    private async Task<Memory<byte>> RowSlotAsync(int rowNum)
    {
        var pageNum = rowNum / RowsPerPage;
        var page = await Pager.GetPageAsync(pageNum);
        var rowOffset = rowNum % RowsPerPage;
        var byteOffset = rowOffset * Row.Size;

        return page.Buffer.AsMemory(byteOffset);
    }
}
