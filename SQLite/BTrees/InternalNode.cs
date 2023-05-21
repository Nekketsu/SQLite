namespace SQLite.BTrees;

public class InternalNode : Node
{
    // Internal node header layout
    const uint numKeysSize = sizeof(uint);
    const uint numKeysOffset = CommonNodeHeaderSize;
    const uint rightChildSize = sizeof(uint);
    const uint rightChildOffset = numKeysOffset + numKeysSize;
    const uint headerSize = CommonNodeHeaderSize + numKeysSize + rightChildSize;
    // Keep this small for testing
    const uint maxCells = 3;

    // Internal node body layout
    const uint keySize = sizeof(uint);
    const uint childSize = sizeof(uint);
    const uint cellSize = childSize + keySize;

    public InternalNode(byte[] node) : base(node) { }

    public uint NumKeys
    {
        get => BitConverter.ToUInt32(node, (int)numKeysOffset);
        set => Array.Copy(BitConverter.GetBytes(value), 0, node, numKeysOffset, numKeysSize);
    }

    public uint RightChild
    {
        get => BitConverter.ToUInt32(node, (int)rightChildOffset);
        set => Array.Copy(BitConverter.GetBytes(value), 0, node, rightChildOffset, rightChildSize);
    }

    public void Initialize()
    {
        NodeType = NodeType.Internal;
        IsNodeRoot = false;
        NumKeys = 0;
    }

    private Memory<byte> GetRightChild =>
        node[(int)rightChildOffset..(int)(rightChildOffset + rightChildSize)];

    public Memory<byte> Cell(uint cellNum)
    {
        var start = headerSize + cellNum * cellSize;
        var end = start + cellSize;

        return node.AsMemory()[(int)start..(int)end];
    }

    private Memory<byte> GetChild(uint childNum)
    {
        var numKeys = NumKeys;

        if (childNum > numKeys)
        {
            DbContext.OutputService.WriteLine($"Tried to access child_num {childNum} > num_keys {numKeys}");
            DbContext.EnvironmentService.Exit(1);
            return null;
        }
        else if (childNum == NumKeys)
        {
            return GetRightChild;
        }
        else
        {
            return Cell(childNum);
        }
    }

    public void UpdateKey(uint oldKey, uint newKey)
    {
        var oldChildIndex = FindChild(oldKey);
        Key(oldChildIndex, newKey);
    }

    public uint Child(uint childNum) => BitConverter.ToUInt32(GetChild(childNum).Span);

    public void Child(uint childNum, uint value)
    {
        var child = GetChild(childNum);
        BitConverter.GetBytes(value).AsSpan().CopyTo(child.Span);
    }

    public uint Key(uint keyNum) =>
        BitConverter.ToUInt32(Cell(keyNum)[(int)childSize..(int)(childSize + keySize)].Span);

    public void Key(uint keyNum, uint value) =>
        BitConverter.GetBytes(value).CopyTo(Cell(keyNum)[(int)childSize..(int)(childSize + keySize)]);

    public static async Task<Cursor> FindAsync(Table table, uint pageNum, uint key)
    {
        var node = await table.Pager.GetPageAsync(pageNum);
        var internalNode = new InternalNode(node.Buffer);

        var childIndex = internalNode.FindChild(key);
        var childNum = internalNode.Child(childIndex);

        var child = await table.Pager.GetPageAsync(childNum);
        switch (new Node(child.Buffer).NodeType)
        {
            case NodeType.Leaf:
                return await LeafNode.FindAsync(table, childNum, key);
            case NodeType.Internal:
                return await FindAsync(table, childNum, key);
        }

        return null!;
    }

    // Return the index of the child which should contain
    // the given key
    public uint FindChild(uint key)
    {
        // Binary search
        var minIndex = 0u;
        var maxIndex = NumKeys; // there is one more child than key

        while (minIndex != maxIndex)
        {
            var index = (minIndex + maxIndex) / 2;
            var keyToRight = Key(index);
            if (keyToRight >= key)
            {
                maxIndex = index;
            }
            else
            {
                minIndex = index + 1;
            }
        }

        return minIndex;
    }

    // Add a new child/key pair to parent that corresponds to child
    public static async Task InsertAsync(Table table, uint parentPageNum, uint childPageNum)
    {
        var parentPage = await table.Pager.GetPageAsync(parentPageNum);
        var childPage = await table.Pager.GetPageAsync(childPageNum);

        var parent = new InternalNode(parentPage.Buffer);
        var child = new Node(childPage.Buffer);

        var childMaxKey = child.MaxKey;
        var index = parent.FindChild(childMaxKey);

        var originalNumKeys = parent.NumKeys;
        parent.NumKeys = originalNumKeys + 1;

        if (originalNumKeys >= maxCells)
        {
            DbContext.OutputService.WriteLine("Need to implement splitting internal node");
            DbContext.EnvironmentService.Exit(1);
        }

        var rightChildPageNum = parent.RightChild;
        var rightChildPage = await table.Pager.GetPageAsync(rightChildPageNum);
        var rightChild = new Node(rightChildPage.Buffer);

        if (childMaxKey > rightChild.MaxKey)
        {
            // Replace right child
            parent.Child(originalNumKeys, rightChildPageNum);
            parent.Key(originalNumKeys, rightChild.MaxKey);
            parent.RightChild = childPageNum;
        }
        else
        {
            // Make room for the new cell
            for (var i = originalNumKeys; i > index; i--)
            {
                var destination = parent.Cell(i);
                var source = parent.Cell(i - 1);
                source.CopyTo(destination);
            }

            parent.Child(index, childPageNum);
            parent.Key(index, childMaxKey);
        }
    }
}
