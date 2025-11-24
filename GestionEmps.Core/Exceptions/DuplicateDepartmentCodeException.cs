namespace GestionEmps.Core.Exceptions;

public class DuplicateDepartmentCodeException :SgeException
{
    public DuplicateDepartmentCodeException(string code) : base($"Le code de département '{code}' existe déjà.", "DEPARTMENT_CODE_EXISTS", 409)
    {
    }
}