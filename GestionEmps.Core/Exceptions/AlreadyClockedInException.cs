namespace GestionEmps.Core.Exceptions;


/// <summary>
/// Represents an exception that is thrown when an attempt is made to clock in an employee who is already clocked in.
/// </summary>
/// <remarks>
/// This exception is used to indicate that an employee already exists in the system as currently clocked in,
/// preventing duplicate clock-in actions for the same employee.
/// It provides a detailed message including the employee ID, an error code "ALREADY_CLOCKED_IN",
/// and an HTTP status code of 409 (Conflict).
/// </remarks>
public class AlreadyClockedInException : SgeException
{
    public AlreadyClockedInException(int employeeId) : base($"L'employé {employeeId} est déjà pointé.", "ALREADY_CLOCKED_IN", 409)
    {
    }
}