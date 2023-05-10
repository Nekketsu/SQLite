namespace SQLite
{
    public class Cursor
    {
        private Table table;
        private int rowNum;
        public bool EndOfTable { get; private set; } // Indicates a position one past the last element

        public Cursor(Table table, int rowNum = 0)
        {
            this.table = table;
            this.rowNum = rowNum;
            EndOfTable = table.NumRows == 0;
        }

        public async Task<Memory<byte>> GetValueAsync()
        {
            var pageNum = rowNum / Table.RowsPerPage;
            var page = await table.Pager.GetPageAsync(pageNum);
            var rowOffset = rowNum % Table.RowsPerPage;
            var byteOffset = rowOffset * Row.Size;

            return page.Buffer.AsMemory(byteOffset);
        }

        public void Advance()
        {
            rowNum++;
            if (rowNum >= table.NumRows)
            {
                EndOfTable = true;
            }
        }
    }
}
