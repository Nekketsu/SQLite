namespace SQLite.BTrees;

public class Node
{
    const uint typeSize = sizeof(byte);
    const uint typeOffset = 0;
    const uint isRootSize = sizeof(byte);
    const uint isRootOffset = typeSize;
    const uint parentPointerSize = sizeof(uint);
    const uint parentPointerOffset = isRootOffset + isRootSize;
    public const uint CommonNodeHeaderSize = typeSize + isRootSize + parentPointerSize;

    protected byte[] node { get; }

    public Node(byte[] node)
    {
        this.node = node;
    }

    public NodeType NodeType
    {
        get => GetNodeType(node);
        set => node[typeOffset] = (byte)value;
    }

    public static NodeType GetNodeType(byte[] node) =>
        (NodeType)node[typeOffset];

    public uint MaxKey
    {
        get
        {
            switch (GetNodeType(node))
            {
                case NodeType.Internal:
                    var internalNode = new InternalNode(node);
                    return internalNode.Key(internalNode.NumKeys - 1);
                case NodeType.Leaf:
                    var leafNode = new LeafNode(node);
                    return leafNode.Key(leafNode.NumCells - 1);
            }

            return 0;
        }
    }

    public uint Parent
    {
        get => BitConverter.ToUInt32(node, (int)parentPointerOffset);
        set => Array.Copy(BitConverter.GetBytes(value), 0, node, parentPointerOffset, sizeof(uint));
    }

    public bool IsNodeRoot
    {
        get => node[isRootOffset] == 1;
        set => node[isRootOffset] = (byte)(value ? 1 : 0);
    }
}
