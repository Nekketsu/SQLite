namespace SQLite;

public class Pager
{
    FileStream fileDescriptor;
    public long FileLength { get; }
    Page?[] pages = new Page?[Table.MaxPages];

    private Pager(string fileName)
    {
        var fileInfo = new FileInfo(fileName);

        fileInfo.Directory?.Create();
        fileDescriptor = fileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite);
        FileLength = fileInfo.Length;

        for (var i = 0; i < Table.MaxPages; i++)
        {
            pages[i] = null;
        }
    }

    public static Pager Open(string fileName) => new Pager(fileName);

    public async Task<Page> GetPageAsync(int pageNum)
    {
        if (pageNum > Table.MaxPages)
        {
            DbContext.OutputService.WriteLine($"Tried to fetch page number out of bounds. {pageNum} > {Table.MaxPages}");
        }

        if (pages[pageNum] is null)
        {
            // Cache miss. Allocate memory and load from file.
            var page = new Page();
            var numPages = FileLength / Page.Size;

            // We might save a partial page at the end of the file
            if (FileLength % Page.Size != 0)
            {
                numPages++;
            }

            if (pageNum <= numPages)
            {
                fileDescriptor.Seek(pageNum * Page.Size, SeekOrigin.Begin);
                try
                {
                    var bytesRead = await fileDescriptor.ReadAsync(page.Buffer.AsMemory());
                }
                catch (Exception e)
                {
                    DbContext.OutputService.WriteLine($"Error reading file: {e.Message}");
                    DbContext.EnvironmentService.Exit(1);
                }
            }

            pages[pageNum] = page;
        }

        return pages[pageNum]!;
    }

    public async Task FlushAsync(int numRows)
    {
        var numFullPages = numRows / Table.RowsPerPage;

        for (int i = 0; i < numFullPages; i++)
        {
            if (pages[i] is null)
            {
                continue;
            }
            await FlushAsync(i, Page.Size);
            pages[i] = null;
        }

        // There may be a partial page to write to the end of the file
        // This should not be needed after we switch to a B-tree
        var numAdditionalRows = numRows % Table.RowsPerPage;
        if (numAdditionalRows > 0)
        {
            var pageNum = numFullPages;
            if (pages[pageNum] is not null)
            {
                await FlushAsync(pageNum, numAdditionalRows * Row.Size);
                pages[pageNum] = null;
            }
        }
    }

    private async Task FlushAsync(int pageNum, int size)
    {
        if (pages[pageNum] is null)
        {
            DbContext.OutputService.WriteLine("Tried to flush null page");
            DbContext.EnvironmentService.Exit(1);
        }

        try
        {
            var offset = fileDescriptor.Seek(pageNum * Page.Size, SeekOrigin.Begin);
        }
        catch (Exception e)
        {
            DbContext.OutputService.WriteLine($"Error seeking: {e}");
            DbContext.EnvironmentService.Exit(1);
        }

        await fileDescriptor.WriteAsync(pages[pageNum]!.Buffer, 0, size);
    }

    public void Close()
    {
        try
        {
            fileDescriptor.Close();
        }
        catch
        {
            DbContext.OutputService.WriteLine("Error closing db file.");
            DbContext.EnvironmentService.Exit(1);
        }
    }
}
