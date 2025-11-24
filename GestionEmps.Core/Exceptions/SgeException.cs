namespace GestionEmps.Core.Exceptions;

/// <summary>
/// Base class for all SGE exceptions.
/// </summary>
public class SgeException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }

    public SgeException(string message, string errorCode, int statusCode) : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}