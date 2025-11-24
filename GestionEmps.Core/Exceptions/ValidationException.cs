namespace GestionEmps.Core.Exceptions;


/// <summary>
/// Represents an exception that is thrown when one or more validation errors occur in the system.
/// </summary>
/// <remarks>
/// This exception is intended to encapsulate validation errors that happen when incoming data
/// does not meet the expected format, constraints, or business rules.
/// It provides a collection of validation errors associated with specific properties and can
/// be instantiated with either multiple errors or a single property error.
/// The default error code for this exception is "VALIDATION_ERROR", and it corresponds to the HTTP
/// status code 400 (Bad Request).
/// </remarks>

public class ValidationException : SgeException
{
    public Dictionary<string, List<string>> Errors { get; }
    public ValidationException(Dictionary<string, List<string>> errors) : base("Une ou plusieurs erreurs de validation sont survenues.", "VALIDATION_ERROR", 400)
    {
        Errors = errors;
    }
    
    public ValidationException(string propertyName, string errorMessage) : this(new Dictionary<string, List<string>> { { propertyName, new List<string> { errorMessage } } })
    {
    }

}