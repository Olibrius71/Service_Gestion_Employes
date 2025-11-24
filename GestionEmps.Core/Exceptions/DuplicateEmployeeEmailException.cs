namespace GestionEmps.Core.Exceptions;

public class DuplicateEmployeeEmailException : SgeException
{
    public DuplicateEmployeeEmailException(string email) : base($"L'email '{email}' est déjà utilisé par un autre employé.", "DUPLICATE_EMPLOYEE_EMAIL", 409)
    {
    }
}