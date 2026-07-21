namespace Apigee.Client.Exceptions;

/// <summary>Base type for every exception raised by this library.</summary>
public abstract class ApigeeException : Exception
{
    /// <summary>Initializes a new instance with the specified error message.</summary>
    protected ApigeeException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance with the specified error message and inner exception.</summary>
    protected ApigeeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
