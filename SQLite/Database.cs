using SQLite.BTrees;

namespace SQLite;

public class Database
{
    private Pager pager;

    public Table Table { get; }

    private Database(Pager pager, Table table)
    {
        this.pager = pager;
        Table = table;
    }

    public static async Task<Database> OpenAsync(string filename)
    {
        var pager = Pager.Open(filename);
        var table = new Table(pager);

        if (pager.NumPages == 0)
        {
            // New database file. Initialize page 0 as leaf node.
            var page = await pager.GetPageAsync(0);
            var rootNode = new LeafNode(page.Buffer);
            rootNode.Initialize();
            rootNode.IsNodeRoot = true;

        }

        return new Database(pager, table);
    }

    public async Task CloseAsync()
    {
        await pager.FlushAsync();
        pager.Close();
    }
}
