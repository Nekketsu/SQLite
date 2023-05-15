namespace SQLite.MetaCommands;

public abstract class MetaCommand
{
    public abstract Task ExecuteAsync();

    public static async Task<(PrepareMetaCommandResult, MetaCommand)> PrepareAsync(string input, Database database)
    {
        if (input == ".exit")
        {
            var metaCommand = new ExitMetaCommand(database);
            return (PrepareMetaCommandResult.Success, metaCommand);
        }
        else if (input == ".constants")
        {
            var metaCommand = new ConstantsMetaCommand();
            return (PrepareMetaCommandResult.Success, metaCommand);
        }
        else if (input == ".btree")
        {
            DbContext.OutputService.WriteLine("Tree:");
            var page = await database.Table.Pager.GetPageAsync(0);
            var metaCommand = new PrintLeafNodeMetaCommand(page.Buffer);
            return (PrepareMetaCommandResult.Success, metaCommand);
        }

        return (PrepareMetaCommandResult.UnrecognizedCommand, null!);
    }
}
