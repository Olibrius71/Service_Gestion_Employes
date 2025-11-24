namespace GestionEmps.Core.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an employee with a specified ID cannot be found.
/// </summary>
/// <remarks>
/// This exception is intended to indicate scenarios where a requested employee resource does not exist in the system.
/// It includes an ID reference for the missing employee, and the error code is set to "EMPLOYEE_NOT_FOUND".
/// The HTTP status code associated with this exception is 404 (Not Found).
/// </remarks>

public class EmployeeNotFoundException : SgeException
{
    public EmployeeNotFoundException(int employeeId) : base($"Employé avec l'ID {employeeId} introuvable.", "EMPLOYEE_NOT_FOUND", 404)
    {
    }
    
    public EmployeeNotFoundException(string email) : base($"Employé avec l'email '{email}' introuvable.", "EMPLOYEE_NOT_FOUND", 404)
    {
    }
}