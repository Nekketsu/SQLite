namespace SQLite.Statements;

public class SelectStatement : Statement
{
    public Table Table { get; }

    public SelectStatement(Table table)
    {
        Table = table;
    }

    public override ExecuteResult Execute()
    {
        for (var i = 0; i < Table.NumRows; i++)
        {
            var row = Table.Select(i);
            PrintRow(row);
        }

        return ExecuteResult.Success;
    }

    private void PrintRow(Row row)
    {
        Console.WriteLine(row);
    }
}
