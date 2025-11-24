namespace GestionEmps.Core.Exceptions;

public class UserRegistrationException : SgeException
{
    public UserRegistrationException(IEnumerable<string> errors) : base($"Erreur lors de la création de l'utilisateur: {string.Join("; ", errors)}", "USER_REGISTRATION_FAILED", 400)
    {
    }
}