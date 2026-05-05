namespace eCommerce.Application.Exceptions;

public class UnautherizedException : Exception
{

    public UnautherizedException(string message = "Unautherized")
        : base(message)
    {
    }

    public UnautherizedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
