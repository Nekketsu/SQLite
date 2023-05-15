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
        var cursor = await Table.StartAsync();
        while (!cursor.EndOfTable)
        {
            var row = Row.Deserialize(await cursor.GetValueAsync());
            PrintRow(row);
            await cursor.AdvanceAsync();
        }

        return ExecuteResult.Success;
    }

    private void PrintRow(Row row)
    {
        DbContext.OutputService.WriteLine(row.ToString());
    }
}
