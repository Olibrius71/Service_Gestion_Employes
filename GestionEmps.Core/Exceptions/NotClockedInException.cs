namespace GestionEmps.Core.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an employee has not clocked in as required.
/// </summary>
/// <remarks>
/// This exception is intended to indicate a scenario where an operation cannot be performed
/// because the employee is not recognized as having clocked in.
/// It includes details about the employee ID, a specific error code ("NOT_CLOCKED_IN"),
/// and associates the exception with the HTTP status code 400 (Bad Request).
/// </remarks>
public class NotClockedInException : SgeException
{
    public NotClockedInException(int employeeId) : base($"L'employé {employeeId} n'a pas pointé à l'arrivée.", "NOT_CLOCKED_IN", 400)
    {
    }
}