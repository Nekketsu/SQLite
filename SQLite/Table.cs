using SQLite.BTrees;

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

    public async Task<Cursor> StartAsync()
    {
        var pageNum = RootPageNum;
        var cellNum = 0u;

        var rootNode = await Pager.GetPageAsync(RootPageNum);
        var numCells = new LeafNode(rootNode.Buffer).NumCells;
        var endOfTable = numCells == cellNum;

        return new Cursor(this, pageNum, cellNum, endOfTable);
    }

    // Return the position of the given key.
    // If the key is not present, return the position
    // where it should be inserted
    public async Task<Cursor> FindAsync(uint key)
    {
        var rootNode = await Pager.GetPageAsync(RootPageNum);

        if (Node.GetNodeType(rootNode.Buffer) == NodeType.Leaf)
        {
            return await LeafNode.FindAsync(this, RootPageNum, key);
        }
        else
        {
            return await InternalNode.FindAsync(this, RootPageNum, key);
        }
    }
}
