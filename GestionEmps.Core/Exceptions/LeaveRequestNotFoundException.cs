namespace GestionEmps.Core.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a leave request cannot be found in the system.
/// </summary>
/// <remarks>
/// This exception is designed to signal that a leave request, identified by its unique ID, was not found.
/// It includes a detailed error message, assigns an error code ("LEAVE_REQUEST_NOT_FOUND"),
/// and sets the HTTP status code to 404 (Not Found).
/// </remarks>

public class LeaveRequestNotFoundException : SgeException
{
    public LeaveRequestNotFoundException(int leaveRequestId) : base($"Demande de congé avec l'ID {leaveRequestId} introuvable.", "LEAVE_REQUEST_NOT_FOUND", 404)
    {
    }
}