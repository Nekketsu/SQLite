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

    public override ExecuteResult Execute()
    {
        if (!Table.Insert(RowToInsert))
        {
            return ExecuteResult.TableFull;
        }

        return ExecuteResult.Success;
    }
}
