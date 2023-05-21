using SQLite.Tests.Services;
using System.Text;

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

        [Fact]
        public async Task PrintsErrorMessageWhenTableIsFull()
        {
            var path = nameof(PrintsErrorMessageWhenTableIsFull);
            CleanUp(path);

            var script = Enumerable.Range(1, 1401)
                .Select(i => $"insert {i} user{i} person{i}@example.com")
                .Append(".exit")
                .ToArray();

            var result = await RunScriptAsync(script, path);

            var expected = new[]
            {
                "db > Executed.",
                "db > Need to implement splitting internal node"
            };

            Assert.Equal(expected, result[^2..]);
        }

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
                "LEAF_NODE_HEADER_SIZE: 14",
                "LEAF_NODE_CELL_SIZE: 297",
                "LEAF_NODE_SPACE_FOR_CELLS: 4082",
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
                "db > Executed.",
                "db > "
            };

            Assert.Equal(expected, result[14..]);
        }

        [Fact]
        public async Task PrintsAllRowsInAMultiLevelTree()
        {
            var path = nameof(PrintsAllRowsInAMultiLevelTree);
            CleanUp(path);

            var script = Enumerable.Range(1, 15)
                .Select(i => $"insert {i} user{i} person{i}@example.com")
                .Append("select")
                .Append(".exit")
                .ToArray();

            var result = await RunScriptAsync(script, path);

            var expected = new[]
            {
                "db > (1, user1, person1@example.com)",
                "(2, user2, person2@example.com)",
                "(3, user3, person3@example.com)",
                "(4, user4, person4@example.com)",
                "(5, user5, person5@example.com)",
                "(6, user6, person6@example.com)",
                "(7, user7, person7@example.com)",
                "(8, user8, person8@example.com)",
                "(9, user9, person9@example.com)",
                "(10, user10, person10@example.com)",
                "(11, user11, person11@example.com)",
                "(12, user12, person12@example.com)",
                "(13, user13, person13@example.com)",
                "(14, user14, person14@example.com)",
                "(15, user15, person15@example.com)",
                "Executed.",
                "db > ",
            };

            Assert.Equal(expected, result[15..]);
        }

        [Fact]
        public async Task PrintingOutTheStructureOfA4LeafNodeBTree()
        {
            var path = nameof(PrintingOutTheStructureOfA4LeafNodeBTree);
            CleanUp(path);

            var script = new[]
            {
                "insert 18 user18 person18@example.com",
                "insert 7 user7 person7@example.com",
                "insert 10 user10 person10@example.com",
                "insert 29 user29 person29@example.com",
                "insert 23 user23 person23@example.com",
                "insert 4 user4 person4@example.com",
                "insert 14 user14 person14@example.com",
                "insert 30 user30 person30@example.com",
                "insert 15 user15 person15@example.com",
                "insert 26 user26 person26@example.com",
                "insert 22 user22 person22@example.com",
                "insert 19 user19 person19@example.com",
                "insert 2 user2 person2@example.com",
                "insert 1 user1 person1@example.com",
                "insert 21 user21 person21@example.com",
                "insert 11 user11 person11@example.com",
                "insert 6 user6 person6@example.com",
                "insert 20 user20 person20@example.com",
                "insert 5 user5 person5@example.com",
                "insert 8 user8 person8@example.com",
                "insert 9 user9 person9@example.com",
                "insert 3 user3 person3@example.com",
                "insert 12 user12 person12@example.com",
                "insert 27 user27 person27@example.com",
                "insert 17 user17 person17@example.com",
                "insert 16 user16 person16@example.com",
                "insert 13 user13 person13@example.com",
                "insert 24 user24 person24@example.com",
                "insert 25 user25 person25@example.com",
                "insert 28 user28 person28@example.com",
                ".btree",
                ".exit"
            };

            var result = await RunScriptAsync(script, path);

            var expected = new[]
            {
                "- internal (size 3)",
                "  - leaf (size 7)",
                "    - 1",
                "    - 2",
                "    - 3",
                "    - 4",
                "    - 5",
                "    - 6",
                "    - 7",
                "  - key 7",
                "  - leaf (size 8)",
                "    - 8",
                "    - 9",
                "    - 10",
                "    - 11",
                "    - 12",
                "    - 13",
                "    - 14",
                "    - 15",
                "  - key 15",
                "  - leaf (size 7)",
                "    - 16",
                "    - 17",
                "    - 18",
                "    - 19",
                "    - 20",
                "    - 21",
                "    - 22",
                "  - key 22",
                "  - leaf (size 8)",
                "    - 23",
                "    - 24",
                "    - 25",
                "    - 26",
                "    - 27",
                "    - 28",
                "    - 29",
                "    - 30",
                "db > ",
            };

            Assert.Equal(expected, result[31..]);
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