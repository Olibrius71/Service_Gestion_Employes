namespace GestionEmps.Core.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a business rule is violated within the system.
/// </summary>
/// <remarks>
/// This exception is intended to indicate issues where a specific business rule has been broken.
/// It includes a customizable message and defaults the error code to "BUSINESS_RULE_VIOLATION".
/// The HTTP status code associated with this exception is 400 (Bad Request).
/// </remarks>

public class BusinessRuleException : SgeException
{
    public BusinessRuleException(string message, string errorCode = "BUSINESS_RULE_VIOLATION") : base(message, errorCode, 400)
    {
    }
}