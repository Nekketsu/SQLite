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
        var table = Table.Open(filename);

        while (true)
        {
            PrintPrompt();
            var input = ReadInput();

            if (input.StartsWith('.'))
            {
                switch (MetaCommand.Prepare(input, table, out var metaCommand))
                {
                    case PrepareMetaCommandResult.Success:
                        break;
                    case PrepareMetaCommandResult.UnrecognizedCommand:
                        DbContext.OutputService.WriteLine($"Unrecognized command {input}");
                        continue;
                }

                await metaCommand.ExecuteAsync();
            }

            switch (Statement.Prepare(input, table, out var statement))
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