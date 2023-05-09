namespace SQLite.MetaCommands;

public abstract class MetaCommand
{
    public abstract Task ExecuteAsync();

    public static PrepareMetaCommandResult Prepare(string input, Table table, out MetaCommand metaCommand)
    {
        if (input == ".exit")
        {
            metaCommand = new ExitMetaCommand(table);
            return PrepareMetaCommandResult.Success;
        }

        metaCommand = null!;

        return PrepareMetaCommandResult.UnrecognizedCommand;
    }
}
