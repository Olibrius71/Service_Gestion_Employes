namespace GestionEmps.Core.Exceptions;

/// <summary>
/// Represents an exception that is thrown when there are insufficient leave days available for a request.
/// </summary>
/// <remarks>
/// This exception is used to indicate that the number of leave days requested exceeds the number of leave days available for an employee.
/// It includes details about the required and available leave days and uses the error code "INSUFFICIENT_LEAVE_DAYS".
/// The associated HTTP status code is 400 (Bad Request).
/// </remarks>
public class InsufficientLeaveDaysException : SgeException
{
    public InsufficientLeaveDaysException(int requiredDays, int availableDays) : base($"Jours de congé insuffisants. Demandé: {requiredDays}, Disponible: {availableDays}", "INSUFFICIENT_LEAVE_DAYS", 400)
    {
    }
}