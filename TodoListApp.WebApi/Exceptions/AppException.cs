namespace TodoListApp.WebApi.Exceptions;

/// <summary>
/// Base application exception for consistent error handling.
/// </summary>
public abstract class AppException : Exception
{
    protected AppException()
    {
    }

    protected AppException(string message)
        : base(message)
    {
    }

    protected AppException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
