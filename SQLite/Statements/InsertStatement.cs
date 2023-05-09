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
            await Table.InsertAsync(RowToInsert);

            return ExecuteResult.Success;
        }
        catch (TableFullException)
        {
            return ExecuteResult.TableFull;
        }
    }
}
