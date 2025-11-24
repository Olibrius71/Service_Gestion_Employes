namespace GestionEmps.Core.Exceptions;

/// <summary>
/// Represents an exception that is thrown when invalid data is provided for an employee within the system.
/// </summary>
/// <remarks>
/// This exception is specifically used to indicate issues where employee-related data is incorrect or does not satisfy the required validation rules.
/// It provides a custom error code "INVALID_EMPLOYEE_DATA" and is associated with a 400 (Bad Request) HTTP status code.
/// </remarks>
public class InvalidEmployeeDataException : SgeException
{
    public InvalidEmployeeDataException(string message) : base(message, "INVALID_EMPLOYEE_DATA", 400)
    {
    }

}