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
            var page = await Table.Pager.GetPageAsync(Table.RootPageNum);
            var node = new LeafNode(page.Buffer);

            var numCells = node.NumCells;
            if (numCells >= LeafNode.MaxCells)
            {
                throw new TableFullException();
            }

            var keyToInsert = RowToInsert.Id;
            var cursor = await Table.FindAsync(keyToInsert);

            if (cursor.CellNum < numCells)
            {
                var keyAtIndex = BitConverter.ToUInt32(node.Key(cursor.CellNum).Span);
                if (keyAtIndex == keyToInsert)
                {
                    return ExecuteResult.DuplicateKey;
                }
            }

            await LeafNode.InsertAsync(cursor, RowToInsert.Id, RowToInsert);

            return ExecuteResult.Success;
        }
        catch (TableFullException)
        {
            return ExecuteResult.TableFull;
        }
    }
}
