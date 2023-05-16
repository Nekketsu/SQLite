namespace SQLite.BTrees;

public class InternalNode : Node
{
    // Internal node header layout
    const uint numKeySize = sizeof(uint);
    const uint numKeysOffset = CommonNodeHeaderSize;
    const uint rightChildSize = sizeof(uint);
    const uint rightChildOffset = numKeysOffset + numKeySize;
    const uint headerSize = CommonNodeHeaderSize + numKeySize + rightChildSize;

    // Internal node body layout
    const uint keySize = sizeof(uint);
    const uint childSize = sizeof(uint);
    const uint cellSize = childSize + keySize;

    public InternalNode(byte[] node) : base(node) { }

    public uint NumKeys
    {
        get => BitConverter.ToUInt32(node, (int)numKeysOffset);
        set => Array.Copy(BitConverter.GetBytes(value), 0, node, numKeysOffset, sizeof(uint));
    }

    public uint RightChild
    {
        get => BitConverter.ToUInt32(node, (int)rightChildOffset);
        set => Array.Copy(BitConverter.GetBytes(value), 0, node, rightChildOffset, sizeof(uint));
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
}
