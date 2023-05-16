using SQLite.MetaCommands;
using SQLite.Statements;

namespace SQLite;

public class Repl
{
    public Repl(DbContext context)
    {
        DbContext.SetContext(context);
    }

    public async Task RunAsync(string filename)
    {
        var database = await Database.OpenAsync(filename);

        while (true)
        {
            PrintPrompt();
            var input = ReadInput();

            if (input.StartsWith('.'))
            {
                var (result, metaCommand) = MetaCommand.Prepare(input, database);
                switch (result)
                {
                    case PrepareMetaCommandResult.Success:
                        await metaCommand.ExecuteAsync();
                        continue;
                    case PrepareMetaCommandResult.UnrecognizedCommand:
                        DbContext.OutputService.WriteLine($"Unrecognized command {input}");
                        continue;
                }
            }

            switch (Statement.Prepare(input, database, out var statement))
            {
                case PrepareStatementResult.Success:
                    break;
                case PrepareStatementResult.NegativeId:
                    DbContext.OutputService.WriteLine("ID must be positive.");
                    continue;
                case PrepareStatementResult.StringTooLong:
                    DbContext.OutputService.WriteLine("String is too long.");
                    continue;
                case PrepareStatementResult.SyntaxError:
                    DbContext.OutputService.WriteLine("Syntax error. Could not parse statement.");
                    continue;
                case PrepareStatementResult.UnrecognizedStatement:
                    DbContext.OutputService.WriteLine($"Unrecognized keyword at start of {input}");
                    continue;
            }

            switch (await statement.ExecuteAsync())
            {
                case ExecuteResult.Success:
                    DbContext.OutputService.WriteLine("Executed.");
                    break;
                case ExecuteResult.DuplicateKey:
                    DbContext.OutputService.WriteLine("Error: Duplicate key.");
                    break;
                case ExecuteResult.TableFull:
                    DbContext.OutputService.WriteLine("Error: Table full.");
                    break;
            }
        }

        void PrintPrompt()
        {
            DbContext.OutputService.Write("db > ");
        }

        string ReadInput()
        {
            var input = DbContext.InputService.ReadLine();

            if (input is null)
            {
                DbContext.OutputService.WriteLine("Error reading input");
                DbContext.EnvironmentService.Exit(1);
            }

            return input!;
        }
    }
}