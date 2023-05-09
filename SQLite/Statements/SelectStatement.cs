namespace SQLite.Statements;

public class SelectStatement : Statement
{
    public Table Table { get; }

    public SelectStatement(Table table)
    {
        Table = table;
    }

    public override async Task<ExecuteResult> ExecuteAsync()
    {
        for (var i = 0; i < Table.NumRows; i++)
        {
            var row = await Table.SelectAsync(i);
            PrintRow(row);
        }

        return ExecuteResult.Success;
    }

    private void PrintRow(Row row)
    {
        DbContext.OutputService.WriteLine(row.ToString());
    }
}
