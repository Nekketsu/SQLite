namespace SQLite.Statements;

public class SelectStatement : Statement
{
    public override void Execute()
    {
        Console.WriteLine("This is where we would do a select.");
    }
}
