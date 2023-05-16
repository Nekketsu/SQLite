using SQLite.BTrees;

namespace SQLite.MetaCommands;

public class PrintTreeMetaCommand : MetaCommand
{
    private Pager pager;
    private uint pageNum;
    private uint indentationLevel;

    public PrintTreeMetaCommand(Pager pager, uint pageNum, uint indentationLevel)
    {
        this.pager = pager;
        this.pageNum = pageNum;
        this.indentationLevel = indentationLevel;
    }

    public override async Task ExecuteAsync()
    {
        await PrintTreeAsync(pager, pageNum, indentationLevel);
    }

    private void Indent(uint level)
    {
        for (var i = 0u; i < level; i++)
        {
            DbContext.OutputService.Write("  ");
        }
    }

    private async Task PrintTreeAsync(Pager pager, uint pageNum, uint indentationLevel)
    {
        var node = await pager.GetPageAsync(pageNum);

        switch (Node.GetNodeType(node.Buffer))
        {
            case NodeType.Leaf:
                {
                    var leafNode = new LeafNode(node.Buffer);
                    var numKeys = leafNode.NumCells;
                    Indent(indentationLevel);
                    DbContext.OutputService.WriteLine($"- leaf (size {numKeys})");
                    for (var i = 0u; i < numKeys; i++)
                    {
                        Indent(indentationLevel + 1);
                        DbContext.OutputService.WriteLine($"- {leafNode.Key(i)}");
                    }
                    break;
                }
            case NodeType.Internal:
                {
                    var internalNode = new InternalNode(node.Buffer);
                    var numKeys = internalNode.NumKeys;
                    Indent(indentationLevel);
                    DbContext.OutputService.WriteLine($"- internal (size {numKeys})");
                    for (var i = 0u; i < numKeys; i++)
                    {
                        var child = internalNode.Child(i);
                        await PrintTreeAsync(pager, child, indentationLevel + 1);

                        Indent(indentationLevel + 1);
                        DbContext.OutputService.WriteLine($"- key {internalNode.Key(i)}");
                    }
                    await PrintTreeAsync(pager, internalNode.RightChild, indentationLevel + 1);
                    break;
                }
        }
    }
}
