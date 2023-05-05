namespace SQLite.Statements;

public class InsertStatement : Statement
{
    public override void Execute()
    {
        Console.WriteLine("This is where we would do an insert.");
    }
}
