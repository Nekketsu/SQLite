using SQLite.Exceptions;

namespace SQLite.Statements;

public class InsertStatement : Statement
{
    public Table Table { get; }
    public Row RowToInsert { get; }

    public InsertStatement(Table table, Row rowToInsert)
    {
        Table = table;
        RowToInsert = rowToInsert;
    }

    public override async Task<ExecuteResult> ExecuteAsync()
    {
        try
        {
            if (Table.NumRows >= Table.MaxRows)
            {
                throw new TableFullException();
            }

            var cursor = Table.End();
            var row = await cursor.GetValueAsync();
            RowToInsert.Serialize(await cursor.GetValueAsync());

            Table.NumRows++;

            return ExecuteResult.Success;
        }
        catch (TableFullException)
        {
            return ExecuteResult.TableFull;
        }
    }
}
