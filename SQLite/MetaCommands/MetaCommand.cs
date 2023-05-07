namespace SQLite.MetaCommands;

public abstract class MetaCommand
{
    public abstract void Execute();

    public static PrepareMetaCommandResult Prepare(string input, out MetaCommand metaCommand)
    {
        if (input == ".exit")
        {
            metaCommand = new ExitMetaCommand();
            return PrepareMetaCommandResult.Success;
        }

        metaCommand = null!;

        return PrepareMetaCommandResult.UnrecognizedCommand;
    }
}
