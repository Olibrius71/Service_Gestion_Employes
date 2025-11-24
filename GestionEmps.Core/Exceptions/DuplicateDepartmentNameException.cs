namespace GestionEmps.Core.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an attempt is made to create or register a department
/// with a name that already exists within the system.
/// </summary>
/// <remarks>
/// This exception is intended to signal violations of unique constraints with respect to department names.
/// It includes a message specifying the conflicting department name, an error code of "DEPARTMENT_NAME_EXISTS",
/// and an associated HTTP status code of 409 (Conflict).
/// </remarks>

public class DuplicateDepartmentNameException : SgeException
{
    public DuplicateDepartmentNameException(string departmentName) : base($"Le nom du département '{departmentName}' existe déjà.", "DEPARTMENT_NAME_EXISTS", 409)
    {
    }
}