namespace GestionEmps.Core.Exceptions;


/// <summary>
/// Represents an exception that is thrown when an issue related to attendance operations occurs.
/// </summary>
/// <remarks>
/// This exception is designed to handle errors specific to attendance functionality within the system.
/// It includes a customizable message and defaults the error code to "ATTENDANCE_ERROR".
/// The HTTP status code associated with this exception is 400 (Bad Request).
/// </remarks>
public class AttendanceException : SgeException
{
    public AttendanceException(string message, string errorCode = "ATTENDANCE_ERROR") : base(message, errorCode, 400)
    {
    }
}