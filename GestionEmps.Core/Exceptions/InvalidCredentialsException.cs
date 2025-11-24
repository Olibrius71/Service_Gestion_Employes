namespace GestionEmps.Core.Exceptions;

public class InvalidCredentialsException : SgeException
{
    public InvalidCredentialsException() : base("Email ou mot de passe incorrect.", "INVALID_CREDENTIALS", 401)
    {
    }
}