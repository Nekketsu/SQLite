namespace SQLite.Statements;

public enum PrepareStatementResult
{
    Success,
    NegativeId,
    StringTooLong,
    UnrecognizedStatement,
    SyntaxError
}
