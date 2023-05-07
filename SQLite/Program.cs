using SQLite;
using SQLite.MetaCommands;
using SQLite.Statements;

var table = new Table();

while (true)
{
    PrintPrompt();
    var input = ReadInput();

    if (input.StartsWith('.'))
    {
        switch (MetaCommand.Prepare(input, out var metaCommand))
        {
            case PrepareMetaCommandResult.Success:
                break;
            case PrepareMetaCommandResult.UnrecognizedCommand:
                Console.WriteLine($"Unrecognized command {input}");
                continue;
        }

        metaCommand.Execute();
    }

    switch (Statement.Prepare(input, table, out var statement))
    {
        case PrepareStatementResult.Success:
            break;
        case PrepareStatementResult.SyntaxError:
            Console.WriteLine("Syntax error. Could not parse statement.");
            continue;
        case PrepareStatementResult.UnrecognizedStatement:
            Console.WriteLine($"Unrecognized keyword at start of {input}");
            continue;
    }

    switch (statement.Execute())
    {
        case ExecuteResult.Success:
            Console.WriteLine("Executed.");
            break;
        case ExecuteResult.TableFull:
            Console.WriteLine("Error: Table full.");
            break;
    }
}

void PrintPrompt()
{
    Console.Write("db > ");
}

string ReadInput()
{
    var input = Console.ReadLine();

    if (input is null)
    {
        Console.WriteLine("Error reading input");
        Environment.Exit(1);
    }

    return input;
}
