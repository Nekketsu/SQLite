namespace SQLite.MetaCommands;

public abstract class MetaCommand
{
    public abstract Task ExecuteAsync();

    public static (PrepareMetaCommandResult, MetaCommand) Prepare(string input, Database database)
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
            var metaCommand = new PrintTreeMetaCommand(database.Table.Pager, 0, 0);
            return (PrepareMetaCommandResult.Success, metaCommand);
        }

        return (PrepareMetaCommandResult.UnrecognizedCommand, null!);
    }
}
