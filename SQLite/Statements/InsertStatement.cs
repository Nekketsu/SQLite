using SQLite.BTrees;
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
            var node = await Table.Pager.GetPageAsync(Table.RootPageNum);
            if (new LeafNode(node.Buffer).NumCells >= LeafNode.MaxCells)
            {
                throw new TableFullException();
            }

            var cursor = await Table.EndAsync();

            await LeafNode.InsertAsync(cursor, RowToInsert.Id, RowToInsert);

            return ExecuteResult.Success;
        }
        catch (TableFullException)
        {
            return ExecuteResult.TableFull;
        }
    }
}
