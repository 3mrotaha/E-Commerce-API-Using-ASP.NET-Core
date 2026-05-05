namespace eCommerce.Application.Exceptions;

public class NullJwtSecretKeyException : Exception
{

    public NullJwtSecretKeyException(string message) : base(message)
    {
    }

    public NullJwtSecretKeyException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
