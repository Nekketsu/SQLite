using SQLite.BTrees;

namespace SQLite.MetaCommands;

public class ConstantsMetaCommand : MetaCommand
{
    public override Task ExecuteAsync()
    {
        DbContext.OutputService.WriteLine("Constants:");
        PrintConstants();

        return Task.CompletedTask;
    }

    private void PrintConstants()
    {
        DbContext.OutputService.WriteLine($"ROW_SIZE: {Row.Size}");
        DbContext.OutputService.WriteLine($"COMMON_NODE_HEADER_SIZE: {Node.CommonNodeHeaderSize}");
        DbContext.OutputService.WriteLine($"LEAF_NODE_HEADER_SIZE: {LeafNode.HeaderSize}");
        DbContext.OutputService.WriteLine($"LEAF_NODE_CELL_SIZE: {LeafNode.CellSize}");
        DbContext.OutputService.WriteLine($"LEAF_NODE_SPACE_FOR_CELLS: {LeafNode.SpaceForCells}");
        DbContext.OutputService.WriteLine($"LEAF_NODE_MAX_CELLS: {LeafNode.MaxCells}");
    }
}
