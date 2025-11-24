namespace GestionEmps.Core.Exceptions;

public abstract class SgeException : Exception
{
    // 💡 Custom Properties to store the error details
    public string ErrorCode { get; }
    public int StatusCode { get; }

    // 🔨 Main Constructor
    public SgeException(string message, string errorCode, int statusCode)
        : base(message) // Call the base Exception class constructor
    {
        this.ErrorCode = errorCode;
        this.StatusCode = statusCode;
    }

    // 🔨 Optional: Overload to allow throwing without a specific error code/status code
    // This could default to a generic internal server error.
    public SgeException(string message) : this(message, "INTERNAL_ERROR", 500)
    {
    }
}