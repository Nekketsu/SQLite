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
            var rootNode = await pager.GetPageAsync(0);
            new LeafNode(rootNode.Buffer).Initialize();
        }

        return new Database(pager, table);
    }

    public async Task CloseAsync()
    {
        await pager.FlushAsync();
        pager.Close();
    }
}
