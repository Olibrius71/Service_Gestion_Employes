namespace GestionEmps.Core.Exceptions;


/// <summary>
/// Represents an exception that is thrown when an invalid status transition occurs for a leave request.
/// </summary>
/// <remarks>
/// This exception is used to indicate that a transition from one leave status to another is not allowed.
/// It includes a detailed error message with the current and attempted statuses, an error code
/// "INVALID_STATUS_TRANSITION", and is associated with the HTTP status code 400 (Bad Request).
/// </remarks>
public class InvalidLeaveStatusTransitionException : SgeException
{
    public InvalidLeaveStatusTransitionException(string currentStatus, string newStatus) : base($"Transition de statut invalide de '{currentStatus}' vers '{newStatus}'", "INVALID_STATUS_TRANSITION", 400)
    {
    }
}