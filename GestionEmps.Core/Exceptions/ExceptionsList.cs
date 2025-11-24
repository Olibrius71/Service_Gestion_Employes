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
    public BusinessRuleException(string message, string errorCode = "BUSINESS_RULE_VIOLATION")
        : base(message, errorCode, 400)
    {
    }
}

/// <summary>
/// Represents an exception that is thrown when an employee with a specified ID cannot be found.
/// </summary>
/// <remarks>
/// This exception is intended to indicate scenarios where a requested employee resource does not exist in the system.
/// The error code is set to "EMPLOYEE_NOT_FOUND". The HTTP status code is 404 (Not Found).
/// </remarks>
public class EmployeeNotFoundException : SgeException
{
    public EmployeeNotFoundException(int employeeId)
        : base($"Employé avec l'ID {employeeId} introuvable.", "EMPLOYEE_NOT_FOUND", 404)
    {
    }
}

/// <summary>
/// Represents an exception that is thrown when a specific department cannot be found.
/// </summary>
/// <remarks>
/// This exception indicates that a department ID does not correspond to an existing department.
/// </remarks>
public class DepartmentNotFoundException : SgeException
{
    public DepartmentNotFoundException(int departmentId)
        : base($"Département avec l'ID {departmentId} introuvable.", "DEPARTMENT_NOT_FOUND", 404)
    {
    }
}

/// <summary>
/// Represents an exception that is thrown when a leave request cannot be found in the system.
/// </summary>
public class LeaveRequestNotFoundException : SgeException
{
    public LeaveRequestNotFoundException(int leaveRequestId)
        : base($"Demande de congé avec l'ID {leaveRequestId} introuvable.", "LEAVE_REQUEST_NOT_FOUND", 404)
    {
    }
}

/// <summary>
/// Represents an exception that is thrown when there are insufficient leave days available.
/// </summary>
public class InsufficientLeaveDaysException : SgeException
{
    public InsufficientLeaveDaysException(int requiredDays, int availableDays)
        : base($"Jours de congé insuffisants. Demandé: {requiredDays}, Disponible: {availableDays}",
              "INSUFFICIENT_LEAVE_DAYS", 400)
    {
    }
}

/// <summary>
/// Represents an exception that is thrown when a leave request conflicts with another.
/// </summary>
public class ConflictingLeaveRequestException : SgeException
{
    public ConflictingLeaveRequestException(DateTime startDate, DateTime endDate)
        : base($"Conflit de congé détecté pour la période du {startDate:dd/MM/yyyy} au {endDate:dd/MM/yyyy}",
              "CONFLICTING_LEAVE_REQUEST", 409)
    {
    }
}

/// <summary>
/// Represents an exception thrown when an invalid status transition occurs.
/// </summary>
public class InvalidLeaveStatusTransitionException : SgeException
{
    public InvalidLeaveStatusTransitionException(string currentStatus, string newStatus)
        : base($"Transition de statut invalide de '{currentStatus}' vers '{newStatus}'",
              "INVALID_STATUS_TRANSITION", 400)
    {
    }
}

/// <summary>
/// Represents an exception for validation errors.
/// </summary>
public class ValidationException : SgeException
{
    public Dictionary<string, List<string>> Errors { get; }

    public ValidationException(Dictionary<string, List<string>> errors)
        : base("Une ou plusieurs erreurs de validation sont survenues.", "VALIDATION_ERROR", 400)
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string errorMessage)
        : this(new Dictionary<string, List<string>> { { propertyName, new List<string> { errorMessage } } })
    {
    }
}

/// <summary>
/// Represents an exception thrown for general attendance-related errors.
/// </summary>
public class AttendanceException : SgeException
{
    public AttendanceException(string message, string errorCode = "ATTENDANCE_ERROR")
        : base(message, errorCode, 400)
    {
    }
}

/// <summary>
/// Represents an exception thrown when an employee is already clocked in.
/// </summary>
public class AlreadyClockedInException : SgeException
{
    public AlreadyClockedInException(int employeeId)
        : base($"L'employé {employeeId} est déjà pointé.", "ALREADY_CLOCKED_IN", 409)
    {
    }
}

/// <summary>
/// Represents an exception thrown when an employee has not clocked in.
/// </summary>
public class NotClockedInException : SgeException
{
    public NotClockedInException(int employeeId)
        : base($"L'employé {employeeId} n'a pas pointé à l'arrivée.", "NOT_CLOCKED_IN", 400)
    {
    }
}

/// <summary>
/// Represents an exception thrown when attempting to create a department with a name that already exists.
/// </summary>
public class DuplicateDepartmentNameException : SgeException
{
    public DuplicateDepartmentNameException(string departmentName)
        : base($"Le nom du département '{departmentName}' existe déjà.", "DEPARTMENT_NAME_EXISTS", 409)
    {
    }
}

/// <summary>
/// Represents an exception thrown when invalid data is provided for an employee.
/// </summary>
public class InvalidEmployeeDataException : SgeException
{
    public InvalidEmployeeDataException(string message)
        : base(message, "INVALID_EMPLOYEE_DATA", 400)
    {
    }
}
