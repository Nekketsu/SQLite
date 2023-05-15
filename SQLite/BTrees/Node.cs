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
}
