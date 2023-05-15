using SQLite.Exceptions;

namespace SQLite.Statements;

public abstract class Statement
{
    public abstract Task<ExecuteResult> ExecuteAsync();

    public static PrepareStatementResult Prepare(string input, Database database, out Statement statement)
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

            try
            {
                if (id < 0)
                {
                    throw new NegativeIdException();
                }

                var rowToInsert = new Row((uint)id, username, email);

                statement = new InsertStatement(database.Table, rowToInsert);
                return PrepareStatementResult.Success;
            }
            catch (NegativeIdException)
            {
                return PrepareStatementResult.NegativeId;
            }
            catch (StringTooLongException)
            {
                return PrepareStatementResult.StringTooLong;
            }
        }
        if (input == "select")
        {
            statement = new SelectStatement(database.Table);
            return PrepareStatementResult.Success;
        }

        return PrepareStatementResult.UnrecognizedStatement;
    }
}
