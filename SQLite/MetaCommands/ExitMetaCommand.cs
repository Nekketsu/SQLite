namespace SQLite.MetaCommands;

public class ExitMetaCommand : MetaCommand
{
    public override void Execute()
    {
        Environment.Exit(0);
    }
}
