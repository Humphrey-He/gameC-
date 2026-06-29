namespace Course07_ResultAndExceptions.Exceptions;

public sealed class PlayerStorageException : Exception
{
    public PlayerStorageException(string message)
        : base(message)
    {
    }

    public PlayerStorageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
