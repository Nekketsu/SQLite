using SQLite.BTrees;

namespace SQLite
{
    public class Cursor
    {
        public Table Table { get; }
        public uint PageNum { get; }
        public uint CellNum { get; private set; }

        public bool EndOfTable { get; private set; } // Indicates a position one past the last element

        private Cursor(Table table, uint pageNum, uint cellNum, bool endOfTable)
        {
            Table = table;
            PageNum = pageNum;
            CellNum = cellNum;

            EndOfTable = endOfTable;
        }


        public static async Task<Cursor> StartAsync(Table table)
        {
            var pageNum = table.RootPageNum;
            var cellNum = 0u;

            var rootNode = await table.Pager.GetPageAsync(table.RootPageNum);
            var numCells = new LeafNode(rootNode.Buffer).NumCells;
            var endOfTable = numCells == cellNum;

            return new Cursor(table, pageNum, cellNum, endOfTable);
        }

        public static async Task<Cursor> EndAsync(Table table)
        {
            var pageNum = table.RootPageNum;

            var rootNode = await table.Pager.GetPageAsync(table.RootPageNum);
            var numCells = new LeafNode(rootNode.Buffer).NumCells;
            var cellNum = numCells;
            var endOfTable = true;

            return new Cursor(table, pageNum, cellNum, endOfTable);
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
