namespace SQLite;

public class Pager
{
    FileStream fileDescriptor;
    public uint FileLength { get; }
    Page?[] pages = new Page?[Table.MaxPages];
    public uint NumPages { get; private set; }

    private Pager(string fileName)
    {
        var fileInfo = new FileInfo(fileName);

        fileInfo.Directory?.Create();
        fileDescriptor = fileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite);
        FileLength = (uint)fileInfo.Length;
        NumPages = FileLength / Page.Size;

        for (var i = 0; i < Table.MaxPages; i++)
        {
            pages[i] = null;
        }
    }

    public static Pager Open(string fileName) => new Pager(fileName);

    public async Task<Page> GetPageAsync(uint pageNum)
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

            if (pageNum >= NumPages)
            {
                NumPages = pageNum + 1;
            }
        }

        return pages[pageNum]!;
    }

    // Until we start recycling free pages, new pages will always
    // go onto the end of the database file
    public uint GetUnusedPageNum()
    {
        return NumPages;
    }

    public async Task FlushAsync()
    {
        for (int i = 0; i < NumPages; i++)
        {
            if (pages[i] is null)
            {
                continue;
            }
            await FlushAsync(i);
            pages[i] = null;
        }
    }

    private async Task FlushAsync(int pageNum)
    {
        if (pages[pageNum] is null)
        {
            DbContext.OutputService.WriteLine("Tried to flush null page");
            DbContext.EnvironmentService.Exit(1);
        }

        try
        {
            fileDescriptor.Seek(pageNum * Page.Size, SeekOrigin.Begin);
        }
        catch (Exception e)
        {
            DbContext.OutputService.WriteLine($"Error seeking: {e}");
            DbContext.EnvironmentService.Exit(1);
        }

        await fileDescriptor.WriteAsync(pages[pageNum]!.Buffer, 0, Page.Size);
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
