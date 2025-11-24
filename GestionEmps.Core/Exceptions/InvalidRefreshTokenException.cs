namespace GestionEmps.Core.Exceptions;

public class InvalidRefreshTokenException : SgeException
{
    public InvalidRefreshTokenException() : base("Token de rafraîchissement invalide.", "INVALID_REFRESH_TOKEN", 401)
    {
    }
}