namespace SQLite.MetaCommands;

public class ExitMetaCommand : MetaCommand
{
    private readonly Table table;

    public ExitMetaCommand(Table table)
    {
        this.table = table;
    }

    public override async Task ExecuteAsync()
    {
        await table.CloseAsync();
        DbContext.EnvironmentService.Exit(0);
    }
}
