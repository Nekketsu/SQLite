using SQLite.Tests.Services;

namespace SQLite.Tests
{
    public class ReplTests
    {
        [Fact]
        public void InsertsAndRetrievesARow()
        {
            var script = new[]
            {
                "insert 1 user1 person1@example.com",
                "select",
                ".exit"
            };

            var result = RunScript(script);

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
        public void PrintsErrorMessageWhenTableIsFull()
        {
            var script = Enumerable.Range(1, 1401)
                .Select(i => $"insert {i} user{i} person{i}@example.com")
                .Append(".exit")
                .ToArray();

            var result = RunScript(script);

            var expected = "db > Error: Table full.";

            Assert.Equal(expected, result[^2]);
        }

        [Fact]
        public void AllowsInsertingStringsThatAreTheMaximumLength()
        {
            var longUserName = new string('a', 32);
            var longEmail = new string('a', 255);

            var script = new[]
            {
                $"insert 1 {longUserName} {longEmail}",
                "select",
                ".exit"
            };

            var result = RunScript(script);

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
        public void PrintsErrorMessageIfStringsAreTooLong()
        {
            var longUsername = new string('a', 33);
            var longEmail = new string('a', 256);

            var script = new[]
            {
                $"insert 1 {longUsername} {longEmail}",
                "select",
                ".exit"
            };

            var result = RunScript(script);

            var expected = new[]
            {
                "db > String is too long.",
                "db > Executed.",
                "db > "
            };

            Assert.Equal(expected, result);
        }

        [Fact]
        public void PrintsAnErrorMessageIfIdIsNegative()
        {
            var script = new[]
            {
                "insert -1 cstack foo@bar.com",
                "select",
                ".exit"
            };

            var result = RunScript(script);

            var expected = new[]
            {
                "db > ID must be positive.",
                "db > Executed.",
                "db > "
            };

            Assert.Equal(expected, result);
        }

        private string[] RunScript(string[] script)
        {
            var environment = new MockEnvironmentService();
            var input = new MockInputService(script);
            var output = new MockOutputService();

            var repl = new Repl(environment, input, output);
            try
            {
                repl.Run();
            }
            catch (ExitException) { }

            return output.Output;
        }
    }
}