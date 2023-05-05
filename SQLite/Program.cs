using SQLite;
using SQLite.MetaCommands;
using SQLite.Statements;

while (true)
{
    PrintPrompt();
    var input = ReadInput();

    if (input.StartsWith('.'))
    {
        switch (PrepareMetaCommand(input, out var metaCommand))
        {
            case MetaCommandResult.Success:
                break;
            case MetaCommandResult.UnrecognizedCommand:
                Console.WriteLine($"Unrecognized command {input}");
                continue;
        }

        metaCommand.Execute();
    }

    switch (PrepareStatement(input, out var statement))
    {
        case PrepareResult.Success:
            break;
        case PrepareResult.UnrecognizedStatement:
            Console.WriteLine($"Unrecognized keyword at start of {input}");
            continue;
    }

    statement.Execute();
    Console.WriteLine("Executed.");
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

MetaCommandResult PrepareMetaCommand(string input, out MetaCommand metaCommand)
{
    if (input == ".exit")
    {
        metaCommand = new ExitMetaCommand();
        return MetaCommandResult.Success;
    }

    metaCommand = null!;

    return MetaCommandResult.UnrecognizedCommand;
}

PrepareResult PrepareStatement(string input, out Statement statement)
{
    if (input.StartsWith("insert"))
    {
        statement = new InsertStatement();
        return PrepareResult.Success;
    }
    if (input == "select")
    {
        statement = new SelectStatement();
        return PrepareResult.Success;
    }

    statement = null!;

    return PrepareResult.UnrecognizedStatement;
}