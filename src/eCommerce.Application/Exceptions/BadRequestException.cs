namespace eCommerce.Application.Exceptions;

public class BadRequestException : Exception
{
    public List<string> Errors {get; set;}
    public BadRequestException(string message)
        : base(message)
    {
        Errors = new List<string>
        {
            message
        };
    }

    public BadRequestException(string message, List<string> errors)
        : base(message)
    {
        Errors = errors;
    }
}
