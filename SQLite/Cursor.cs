using SQLite.BTrees;

namespace SQLite
{
    public class Cursor
    {
        public Table Table { get; }
        public uint PageNum { get; }
        public uint CellNum { get; private set; }
        public bool EndOfTable { get; private set; } // Indicates a position one past the last element

        public Cursor(Table table, uint pageNum, uint cellNum, bool endOfTable)
        {
            Table = table;
            PageNum = pageNum;
            CellNum = cellNum;

            EndOfTable = endOfTable;
        }

        public async Task<Memory<byte>> GetValueAsync()
        {
            var page = await Table.Pager.GetPageAsync(PageNum);

            return new LeafNode(page.Buffer).Value(CellNum);
        }

        public async Task AdvanceAsync()
        {
            var node = await Table.Pager.GetPageAsync(PageNum);
            CellNum++;

            if (CellNum >= new LeafNode(node.Buffer).NumCells)
            {
                EndOfTable = true;
            }
        }
    }
}
