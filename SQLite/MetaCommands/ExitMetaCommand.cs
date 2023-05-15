namespace SQLite.MetaCommands;

public class ExitMetaCommand : MetaCommand
{
    private readonly Database database;

    public ExitMetaCommand(Database database)
    {
        this.database = database;
    }

    public override async Task ExecuteAsync()
    {
        await database.CloseAsync();
        DbContext.EnvironmentService.Exit(0);
    }
}
