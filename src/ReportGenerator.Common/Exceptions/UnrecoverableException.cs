namespace ReportGenerator.Common.Exceptions;

/// <summary>
/// A custom exception handling an unrecoverable error that occurred that should not be retried.
/// </summary>
public class UnrecoverableException : Exception
{
    public UnrecoverableException() { }

    public UnrecoverableException(string message)
        : base(message) { }

    public UnrecoverableException(string message, Exception inner)
        : base(message, inner) { }
}
