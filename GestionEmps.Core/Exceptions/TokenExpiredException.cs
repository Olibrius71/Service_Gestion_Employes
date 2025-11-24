namespace GestionEmps.Core.Exceptions;

public class TokenExpiredException : SgeException
{
    public TokenExpiredException() : base("Le token a expiré.", "TOKEN_EXPIRED", 401)
    {
    }
}