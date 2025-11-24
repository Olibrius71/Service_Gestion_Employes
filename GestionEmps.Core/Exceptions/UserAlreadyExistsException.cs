namespace GestionEmps.Core.Exceptions;

public class UserAlreadyExistsException : SgeException
{
    public UserAlreadyExistsException(string identifier, string type = "email") : base($"Un utilisateur avec ce {type} '{identifier}' existe déjà.", "USER_ALREADY_EXISTS", 409)
    {
    }
}