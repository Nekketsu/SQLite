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

            var expected = "db > Error: Table full.";

            Assert.Equal(expected, result[^2]);
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
            var path = nameof(PrintsErrorMessageWhenTableIsFull);
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