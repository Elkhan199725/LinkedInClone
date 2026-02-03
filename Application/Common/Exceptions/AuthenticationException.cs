namespace Application.Common.Exceptions;

public class AuthenticationException : Exception
{
    public AuthenticationException() : base("Invalid credentials.")
    {
    }

    public AuthenticationException(string message) : base(message)
    {
    }

    public AuthenticationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
