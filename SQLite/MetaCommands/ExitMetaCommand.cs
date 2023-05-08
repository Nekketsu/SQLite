using SQLite.Services;

namespace SQLite.MetaCommands;

public class ExitMetaCommand : MetaCommand
{
    private readonly IEnvironmentService environment;

    public ExitMetaCommand(IEnvironmentService environment)
    {
        this.environment = environment;
    }

    public override void Execute()
    {
        environment.Exit(0);
    }
}
