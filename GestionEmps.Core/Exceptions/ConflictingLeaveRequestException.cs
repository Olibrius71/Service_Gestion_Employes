namespace GestionEmps.Core.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a leave request conflicts with another existing leave request.
/// </summary>
/// <remarks>
/// This exception is utilized to indicate a situation where the requested leave period overlaps
/// with another leave request. It provides a descriptive message with the conflicting dates, and
/// includes an error code "CONFLICTING_LEAVE_REQUEST". The HTTP status code associated with
/// this exception is 409 (Conflict).
/// </remarks>
public class ConflictingLeaveRequestException : SgeException
{
    public ConflictingLeaveRequestException(DateTime startDate, DateTime endDate) : base($"Conflit de congé détecté pour la période du {startDate:dd/MM/yyyy} au {endDate:dd/MM/yyyy}", "CONFLICTING_LEAVE_REQUEST", 409)
    {
    }
}