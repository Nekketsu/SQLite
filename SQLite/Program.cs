while (true)
{
    PrintPrompt();
    var input = ReadInput();

    if (input == ".exit")
    {
        Environment.Exit(0);
    }
    else
    {
        Console.WriteLine($"Unrecognized command {input}");
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