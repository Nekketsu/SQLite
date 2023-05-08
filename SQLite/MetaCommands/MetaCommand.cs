﻿using SQLite.Services;

namespace SQLite.MetaCommands;

public abstract class MetaCommand
{
    public abstract void Execute();

    public static PrepareMetaCommandResult Prepare(IEnvironmentService environment, string input, out MetaCommand metaCommand)
    {
        if (input == ".exit")
        {
            metaCommand = new ExitMetaCommand(environment);
            return PrepareMetaCommandResult.Success;
        }

        metaCommand = null!;

        return PrepareMetaCommandResult.UnrecognizedCommand;
    }
}
