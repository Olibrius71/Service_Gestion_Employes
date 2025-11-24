namespace GestionEmps.Core.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a specific department  cannot be found.
/// </summary>
/// <remarks>
/// This exception is intended to indicate that an operation involving a department has failed
/// because the specified department ID does not correspond to an existing department.
/// It includes a descriptive message, an error code set to "DEPARTMENT_NOT_FOUND",
/// and uses the HTTP status code 404 (Not Found).
/// </remarks>
public class DepartmentNotFoundException : SgeException
{
    public DepartmentNotFoundException(int departmentId) : base($"Département avec l'ID {departmentId} introuvable.", "DEPARTMENT_NOT_FOUND", 404)
    {
    }
}