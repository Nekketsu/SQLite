using SQLite.BTrees;

namespace SQLite.MetaCommands;

public class PrintLeafNodeMetaCommand : MetaCommand
{
    public byte[] Node { get; }

    public PrintLeafNodeMetaCommand(byte[] node)
    {
        Node = node;
    }

    public override Task ExecuteAsync()
    {
        var leafNode = new LeafNode(Node);

        var numCells = leafNode.NumCells;
        DbContext.OutputService.WriteLine($"leaf (size {numCells})");
        for (var i = 0u; i < numCells; i++)
        {
            var key = BitConverter.ToUInt32(leafNode.Key(i).Span);
            DbContext.OutputService.WriteLine($"  - {i} : {key}");
        }

        return Task.CompletedTask;
    }
}
