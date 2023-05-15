namespace SQLite.BTrees;

public class LeafNode : Node
{
    // Leaf node header layout
    const uint numCellsSize = sizeof(uint);
    const uint numCellsOffset = CommonNodeHeaderSize;
    public const uint HeaderSize = CommonNodeHeaderSize + numCellsSize;

    // Leaf node body layout
    const uint keySize = sizeof(uint);
    const uint keyOffset = 0;
    const uint valueSize = Row.Size;
    const uint valueOffset = keyOffset + keySize;
    public const uint CellSize = keySize + valueSize;
    public const uint SpaceForCells = Page.Size - HeaderSize;
    public const uint MaxCells = SpaceForCells / CellSize;

    public LeafNode(byte[] node) : base(node)
    {
    }

    public void Initialize()
    {
        NumCells = 0;
    }

    public uint NumCells
    {
        get => BitConverter.ToUInt32(node, (int)numCellsOffset);
        set => Array.Copy(BitConverter.GetBytes(value), 0, node, numCellsOffset, sizeof(uint));
    }

    public Memory<byte> Cell(uint cellNum)
    {
        var start = HeaderSize + cellNum * CellSize;
        var end = start + CellSize;

        return node.AsMemory()[(int)start..(int)end];
    }

    public Memory<byte> Key(uint cellNum) => Cell(cellNum)[..(int)keySize];

    public Memory<byte> Value(uint cellNum) => Cell(cellNum)[(int)keySize..];

    public static async Task InsertAsync(Cursor cursor, uint key, Row value)
    {
        var page = await cursor.Table.Pager.GetPageAsync(cursor.PageNum);
        var node = new LeafNode(page.Buffer);

        if (node.NumCells >= MaxCells)
        {
            // Node full
            DbContext.OutputService.WriteLine("Need to implement splitting a leaf node.\n");
            DbContext.EnvironmentService.Exit(1);
        }

        if (cursor.CellNum < node.NumCells)
        {
            // Make room for new cell
            for (var i = node.NumCells; i > cursor.CellNum; i--)
            {
                node.Cell(i - 1).CopyTo(node.Cell(i));
            }
        }

        node.NumCells++;
        BitConverter.GetBytes(key).CopyTo(node.Key(cursor.CellNum));
        value.Serialize(node.Value(cursor.CellNum));
    }
}
