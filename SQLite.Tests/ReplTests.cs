using SQLite.Tests.Services;

namespace SQLite.Tests
{
    public class ReplTests
    {
        const string filename = "test.db";

        [Fact]
        public async Task InsertsAndRetrievesARow()
        {
            var path = nameof(InsertsAndRetrievesARow);
            CleanUp(path);

            var script = new[]
            {
                "insert 1 user1 person1@example.com",
                "select",
                ".exit"
            };

            var result = await RunScriptAsync(script, path);

            var expected = new[]
            {
                "db > Executed.",
                "db > (1, user1, person1@example.com)",
                "Executed.",
                "db > "
            };

            Assert.Equal(expected, result);
        }

        //[Fact]
        //public async Task PrintsErrorMessageWhenTableIsFull()
        //{
        //    var path = nameof(PrintsErrorMessageWhenTableIsFull);
        //    CleanUp(path);

        //    var script = Enumerable.Range(1, 1401)
        //        .Select(i => $"insert {i} user{i} person{i}@example.com")
        //        .Append(".exit")
        //        .ToArray();

        //    var result = await RunScriptAsync(script, path);

        //    var expected = "db > Error: Table full.";

        //    Assert.Equal(expected, result[^2]);
        //}

        [Fact]
        public async Task AllowsInsertingStringsThatAreTheMaximumLength()
        {
            var path = nameof(AllowsInsertingStringsThatAreTheMaximumLength);
            CleanUp(path);

            var longUserName = new string('a', 32);
            var longEmail = new string('a', 255);

            var script = new[]
            {
                $"insert 1 {longUserName} {longEmail}",
                "select",
                ".exit"
            };

            var result = await RunScriptAsync(script, path);

            var expected = new[]
            {
                "db > Executed.",
                $"db > (1, {longUserName}, {longEmail})",
                "Executed.",
                "db > "
            };

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task PrintsErrorMessageIfStringsAreTooLong()
        {
            var path = nameof(PrintsErrorMessageIfStringsAreTooLong);
            CleanUp(path);

            var longUsername = new string('a', 33);
            var longEmail = new string('a', 256);

            var script = new[]
            {
                $"insert 1 {longUsername} {longEmail}",
                "select",
                ".exit"
            };

            var result = await RunScriptAsync(script, path);

            var expected = new[]
            {
                "db > String is too long.",
                "db > Executed.",
                "db > "
            };

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task PrintsAnErrorMessageIfIdIsNegative()
        {
            var path = nameof(PrintsAnErrorMessageIfIdIsNegative);
            CleanUp(path);

            var script = new[]
            {
                "insert -1 cstack foo@bar.com",
                "select",
                ".exit"
            };

            var result = await RunScriptAsync(script, path);

            var expected = new[]
            {
                "db > ID must be positive.",
                "db > Executed.",
                "db > "
            };

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task KeepsDataAfterClosingConnection()
        {
            var path = nameof(KeepsDataAfterClosingConnection);
            CleanUp(path);

            var script1 = new[]
            {
                "insert 1 user1 person1@example.com",
                ".exit"
            };

            var result1 = await RunScriptAsync(script1, path);

            var expected1 = new[]
            {
                "db > Executed.",
                "db > ",
            };

            Assert.Equal(expected1, result1);


            var script2 = new[]
            {
                "select",
                ".exit"
            };

            var result2 = await RunScriptAsync(script2, path);

            var expected2 = new[]
            {
                "db > (1, user1, person1@example.com)",
                "Executed.",
                "db > "
            };

            Assert.Equal(expected2, result2);

        }

        [Fact]
        public async void PrintsConstants()
        {
            var path = nameof(PrintsConstants);
            CleanUp(path);

            var script = new[]
            {
                ".constants",
                ".exit"
            };

            var result = await RunScriptAsync(script, path);

            var expected = new[]
            {
                "db > Constants:",
                "ROW_SIZE: 293",
                "COMMON_NODE_HEADER_SIZE: 6",
                "LEAF_NODE_HEADER_SIZE: 10",
                "LEAF_NODE_CELL_SIZE: 297",
                "LEAF_NODE_SPACE_FOR_CELLS: 4086",
                "LEAF_NODE_MAX_CELLS: 13",
                "db > "
            };

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task AllowsPrintingOutTheStructureOfAOneNodeBTree()
        {
            var path = nameof(AllowsPrintingOutTheStructureOfAOneNodeBTree);
            CleanUp(path);

            var script = new[] { 3, 1, 2 }
            .Select(i => $"insert {i} user{i} person{i}@example.com")
            .Append(".btree")
            .Append(".exit")
            .ToArray();

            var result = await RunScriptAsync(script, path);

            var expected = new[]
            {
                "db > Executed.",
                "db > Executed.",
                "db > Executed.",
                "db > Tree:",
                "- leaf (size 3)",
                "  - 1",
                "  - 2",
                "  - 3",
                "db > "
            };

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task PrintsAnErrorMessageIfThereIsADuplicateId()
        {
            var path = nameof(PrintsAnErrorMessageIfThereIsADuplicateId);
            CleanUp(path);

            var script = new[]
            {
                "insert 1 user1 person1@example.com",
                "insert 1 user1 person1@example.com",
                "select",
                ".exit"
            };

            var result = await RunScriptAsync(script, path);

            var expected = new[]
            {
                "db > Executed.",
                "db > Error: Duplicate key.",
                "db > (1, user1, person1@example.com)",
                "Executed.",
                "db > "
            };

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task AllowsPrintingOutTheStructureOfA3LeafNode()
        {
            var path = nameof(AllowsPrintingOutTheStructureOfA3LeafNode);
            CleanUp(path);

            var script = Enumerable.Range(1, 14)
                .Select(i => $"insert {i} user{i} person{i}@example.com")
                .Append(".btree")
                .Append("insert 15 user15 person15@example.com")
                .Append(".exit")
                .ToArray();

            var result = await RunScriptAsync(script, path);

            var expected = new[]
            {
                "db > Tree:",
                "- internal (size 1)",
                "  - leaf (size 7)",
                "    - 1",
                "    - 2",
                "    - 3",
                "    - 4",
                "    - 5",
                "    - 6",
                "    - 7",
                "  - key 7",
                "  - leaf (size 7)",
                "    - 8",
                "    - 9",
                "    - 10",
                "    - 11",
                "    - 12",
                "    - 13",
                "    - 14",
                "db > Need to implement searching an internal node",
            };

            Assert.Equal(expected, result[14..]);
        }

        private async Task<string[]> RunScriptAsync(string[] script, string path)
        {
            var outputService = new MockOutputService();

            var context = new DbContext(
                new MockInputService(script),
                outputService,
                new MockEnvironmentService()
            );

            var repl = new Repl(context);
            try
            {
                var fullPath = Path.Combine(path, filename);
                await repl.RunAsync(fullPath);
            }
            catch (ExitException) { }

            return outputService.Output;
        }

        private void CleanUp(string path)
        {
            var fullPath = Path.Combine(path, filename);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}