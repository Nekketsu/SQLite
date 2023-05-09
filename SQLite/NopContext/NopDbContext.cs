namespace SQLite.NopContext;

internal class NopDbContext : DbContext
{
    public NopDbContext() : base(new NopInputService(), new NopOutputService(), new NopEnvironmentService())
    {
    }
}
