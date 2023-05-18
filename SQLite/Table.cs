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
        var cursor = await FindAsync(0);

        var node = await Pager.GetPageAsync(cursor.PageNum);
        var numCells = new LeafNode(node.Buffer).NumCells;
        cursor.EndOfTable = numCells == 0;

        return cursor;
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
