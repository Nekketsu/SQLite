using SQLite.MetaCommands;
using SQLite.Services;
using SQLite.Statements;

namespace SQLite;

public class Repl
{
    private readonly IEnvironmentService environmentService;
    private readonly IInputService inputService;
    private readonly IOutputService outputService;

    public Repl(IEnvironmentService environmentService, IInputService inputService, IOutputService outputService)
    {
        this.environmentService = environmentService;
        this.inputService = inputService;
        this.outputService = outputService;
    }

    public void Run()
    {
        var table = new Table();

        while (true)
        {
            PrintPrompt();
            var input = ReadInput();

            if (input.StartsWith('.'))
            {
                switch (MetaCommand.Prepare(environmentService, input, out var metaCommand))
                {
                    case PrepareMetaCommandResult.Success:
                        break;
                    case PrepareMetaCommandResult.UnrecognizedCommand:
                        outputService.WriteLine($"Unrecognized command {input}");
                        continue;
                }

                metaCommand.Execute();
            }

            switch (Statement.Prepare(input, table, outputService, out var statement))
            {
                case PrepareStatementResult.Success:
                    break;
                case PrepareStatementResult.NegativeId:
                    outputService.WriteLine("ID must be positive.");
                    continue;
                case PrepareStatementResult.StringTooLong:
                    outputService.WriteLine("String is too long.");
                    continue;
                case PrepareStatementResult.SyntaxError:
                    outputService.WriteLine("Syntax error. Could not parse statement.");
                    continue;
                case PrepareStatementResult.UnrecognizedStatement:
                    outputService.WriteLine($"Unrecognized keyword at start of {input}");
                    continue;
            }

            switch (statement.Execute())
            {
                case ExecuteResult.Success:
                    outputService.WriteLine("Executed.");
                    break;
                case ExecuteResult.TableFull:
                    outputService.WriteLine("Error: Table full.");
                    break;
            }
        }

        void PrintPrompt()
        {
            outputService.Write("db > ");
        }

        string ReadInput()
        {
            var input = this.inputService.ReadLine();

            if (input is null)
            {
                outputService.WriteLine("Error reading input");
                Environment.Exit(1);
            }

            return input;
        }
    }
}