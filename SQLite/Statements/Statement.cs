namespace SQLite.Statements;

public abstract class Statement
{
    public abstract ExecuteResult Execute();

    public static PrepareStatementResult Prepare(string input, Table table, out Statement statement)
    {
        statement = null!;

        if (input.StartsWith("insert"))
        {
            var inputSplit = input.Split();
            if (inputSplit.Length < 4
                || inputSplit[0] != "insert"
                || !int.TryParse(inputSplit[1], out var id))
            {
                return PrepareStatementResult.SyntaxError;
            }

            var username = inputSplit[2];
            var email = inputSplit[3];

            var rowToInsert = new Row(id, username, email);

            statement = new InsertStatement(table, rowToInsert);
            return PrepareStatementResult.Success;
        }
        if (input == "select")
        {
            statement = new SelectStatement(table);
            return PrepareStatementResult.Success;
        }

        return PrepareStatementResult.UnrecognizedStatement;
    }
}
