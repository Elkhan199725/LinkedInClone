namespace Application.Common.Exceptions;

public class RegistrationException : Exception
{
    public IEnumerable<string> Errors { get; }

    public RegistrationException(IEnumerable<string> errors)
        : base("Registration failed.")
    {
        Errors = errors;
    }

    public RegistrationException(string message) : base(message)
    {
        Errors = [message];
    }

    public RegistrationException(string message, Exception innerException)
        : base(message, innerException)
    {
        Errors = [message];
    }
}
