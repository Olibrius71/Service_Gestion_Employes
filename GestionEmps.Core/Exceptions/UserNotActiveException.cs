namespace GestionEmps.Core.Exceptions;

public class UserNotActiveException : SgeException
{
    public UserNotActiveException() : base("Compte utilisateur désactivé.", "USER_NOT_ACTIVE", 403)
    {
    }
}