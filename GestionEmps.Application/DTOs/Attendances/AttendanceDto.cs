namespace SGE.Application.DTOs;

public class AttendanceDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the attendance record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the employee associated
    /// with the attendance record.
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// Gets or sets the name of the employee associated with the
    /// attendance record.
    /// </summary>
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date of the attendance record.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the clock-in time for the attendance record.
    /// </summary>
    public TimeSpan? ClockIn { get; set; }

    /// <summary>
    /// Gets or sets the clock-out time for the attendance record.
    /// </summary>
    public TimeSpan? ClockOut { get; set; }

    /// <summary>
    /// Gets or sets the duration of the break taken during the workday.
    /// </summary>
    public TimeSpan? BreakDuration { get; set; }

    /// <summary>
    /// Gets or sets the total number of hours worked by the employee
    /// during the recorded date.
    /// </summary>
    public decimal WorkedHours { get; set; }
}