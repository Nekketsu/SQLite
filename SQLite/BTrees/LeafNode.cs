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
    private const uint rightSplitCount = (MaxCells + 1) / 2;
    private const uint leftSplitCount = (MaxCells + 1) - rightSplitCount;

    public LeafNode(byte[] node) : base(node)
    {
    }

    public void Initialize()
    {
        NodeType = NodeType.Leaf;
        IsNodeRoot = false;
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

    public uint Key(uint cellNum) => BitConverter.ToUInt32(Cell(cellNum)[..(int)keySize].Span);
    public void Key(uint cellNum, uint value) =>
        BitConverter.GetBytes(value).CopyTo(Cell(cellNum)[..(int)keySize]);

    public Memory<byte> Value(uint cellNum) => Cell(cellNum)[(int)keySize..];

    public static async Task InsertAsync(Cursor cursor, uint key, Row value)
    {
        var page = await cursor.Table.Pager.GetPageAsync(cursor.PageNum);
        var node = new LeafNode(page.Buffer);

        if (node.NumCells >= MaxCells)
        {
            // Node full
            await SplitAndInsert(cursor, key, value);
            return;
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
        node.Key(cursor.CellNum, key);
        value.Serialize(node.Value(cursor.CellNum));
    }

    private static async Task SplitAndInsert(Cursor cursor, uint key, Row value)
    {
        // Create a new node and move half the cells over
        // Insert the new value in one of the two nodes.
        // Update parent or create a new parent.
        var oldPage = await cursor.Table.Pager.GetPageAsync(cursor.PageNum);
        var newPageNum = cursor.Table.Pager.GetUnusedPageNum();
        var newPage = await cursor.Table.Pager.GetPageAsync(newPageNum);

        var oldNode = new LeafNode(oldPage.Buffer);
        var newNode = new LeafNode(newPage.Buffer);

        newNode.Initialize();

        // All existing keys plus new key should be divided
        // evenly between old (left) and new (right) nodes.
        // Starting from the right, move each key to correct position.
        for (var i = (int)MaxCells; i >= 0; i--)
        {
            var destionationNode = i >= leftSplitCount
                ? newNode
                : oldNode;

            var indexWithinNode = (uint)i % leftSplitCount;
            var destination = destionationNode.Cell(indexWithinNode);

            if (i == cursor.CellNum)
            {
                value.Serialize(destination);
            }
            else if (i > cursor.CellNum)
            {
                oldNode.Cell((uint)i - 1).CopyTo(destination);
            }
            else
            {
                oldNode.Cell((uint)i).CopyTo(destination);
            }
        }

        // Update cell count on both leaf nodes
        oldNode.NumCells = leftSplitCount;
        newNode.NumCells = rightSplitCount;

        if (oldNode.IsNodeRoot)
        {
            await CreateNewRoot(cursor.Table, newPageNum);
        }
        else
        {
            DbContext.OutputService.WriteLine("Need to implement updating parent after split");
            DbContext.EnvironmentService.Exit(1);
        }
    }

    private static async Task CreateNewRoot(Table table, uint rightChildPageNum)
    {
        // Handle splitting the root.
        // Old root copied to new page, becomes left child.
        // Address of right child passed in.
        // Re-initialize root page to contain the new root node.
        // New root node points to two children.
        var root = await table.Pager.GetPageAsync(table.RootPageNum);
        var rightChild = await table.Pager.GetPageAsync(rightChildPageNum);
        var leftChildPageNum = table.Pager.GetUnusedPageNum();
        var leftChild = await table.Pager.GetPageAsync(leftChildPageNum);

        // Left child has data copied from old root
        root.Buffer.AsSpan().CopyTo(leftChild.Buffer.AsSpan());

        var leftChildNode = new Node(leftChild.Buffer);
        leftChildNode.IsNodeRoot = false;

        // Root node is new internal node with one key and two children
        var rootNode = new InternalNode(root.Buffer);
        rootNode.Initialize();
        rootNode.IsNodeRoot = true;
        rootNode.NumKeys = 1;
        rootNode.Child(0, leftChildPageNum);
        var leftChildMaxKey = leftChildNode.MaxKey;
        rootNode.Key(0, leftChildMaxKey);
        rootNode.RightChild = rightChildPageNum;
    }

    public static async Task<Cursor> FindAsync(Table table, uint pageNum, uint key)
    {
        var page = await table.Pager.GetPageAsync(pageNum);
        var node = new LeafNode(page.Buffer);
        var numCells = node.NumCells;

        // Binary Search
        var minIndex = 0u;
        var onePastMaxIndex = numCells;
        while (onePastMaxIndex != minIndex)
        {
            var index = (minIndex + onePastMaxIndex) / 2;
            var keyAtIndex = node.Key(index);
            if (key == keyAtIndex)
            {
                return new Cursor(table, pageNum, index, numCells == index);
            }
            if (key < keyAtIndex)
            {
                onePastMaxIndex = index;
            }
            else
            {
                minIndex = index + 1;
            }
        }

        return new Cursor(table, pageNum, minIndex, numCells == minIndex);
    }
}
