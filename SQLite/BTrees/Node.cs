namespace SQLite.BTrees;

public class Node
{
    const int typeSize = sizeof(byte);
    const int typeOffset = 0;
    const int isRootSize = sizeof(byte);
    const int isRootOffset = typeSize;
    const int parentPointerSize = sizeof(uint);
    const int parentPointerOffset = isRootOffset + isRootSize;
    public const int CommonNodeHeaderSize = typeSize + isRootSize + parentPointerSize;

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

    public bool IsNodeRoot
    {
        get => node[isRootOffset] == 1;
        set => node[isRootOffset] = (byte)(value ? 1 : 0);
    }
}
