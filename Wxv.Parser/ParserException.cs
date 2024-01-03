namespace Wxv.Parser;

public class ParserException : Exception
{
    public ParserResult ParserResult { get; private set; }

    public ParserException(ParserResult result)
        : base(BuildMessage(result, "Invalid Data"))
    {
        ParserResult = result;
    }

    public ParserException(ParserResult result, string message, params object[] args)
        : base(BuildMessage(result, message, args))
    {
        ParserResult = result;
    }

    public ParserException(ParserResult result, Exception innerException, string message, params object[] args)
        : base(BuildMessage(result, message, args), innerException)
    {
        ParserResult = result;
    }

    private static string BuildMessage(ParserResult result, string message, params object[] args)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));

        return string.Format("{2} : Ln {0}, Col {1}", 
            result.StartPosition.Row, 
            result.StartPosition.Col, 
            string.Format(message, args));
    }
}