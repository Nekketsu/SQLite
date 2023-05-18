using SQLite.BTrees;

namespace SQLite
{
    public class Cursor
    {
        public Table Table { get; }
        public uint PageNum { get; private set; }
        public uint CellNum { get; private set; }
        public bool EndOfTable { get; set; } // Indicates a position one past the last element

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
            var leafNode = new LeafNode(node.Buffer);
            CellNum++;

            if (CellNum >= leafNode.NumCells)
            {
                // Advance to next leaf node
                uint nextPageNum = leafNode.NextLeaf;
                if (nextPageNum == 0)
                {
                    // This was rightmost leaf
                    EndOfTable = true;
                }
                else
                {
                    PageNum = nextPageNum;
                    CellNum = 0;
                }
            }
        }
    }
}
