using SQLite.Services;

namespace SQLite.Statements;

public class SelectStatement : Statement
{
    private readonly IOutputService output;

    public Table Table { get; }

    public SelectStatement(Table table, IOutputService output)
    {
        Table = table;
        this.output = output;
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
        output.WriteLine(row.ToString());
    }
}
