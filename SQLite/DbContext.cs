using SQLite.NopContext;
using SQLite.Services;

namespace SQLite;

public class DbContext
{
    public static IInputService InputService => context.inputService;
    public static IOutputService OutputService => context.outputService;
    public static IEnvironmentService EnvironmentService => context.environmentService;

    private readonly IInputService inputService;
    private readonly IOutputService outputService;
    private readonly IEnvironmentService environmentService;

    public DbContext(IInputService inputService, IOutputService outputService, IEnvironmentService environmentService)
    {
        this.inputService = inputService;
        this.outputService = outputService;
        this.environmentService = environmentService;
    }

    private static DbContext context = new NopDbContext();

    public static void SetContext(DbContext context)
    {
        DbContext.context = context;
    }
}
