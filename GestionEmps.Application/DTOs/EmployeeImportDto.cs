namespace SGE.Application.DTOs;

/// <summary>
/// Data Transfer Object for importing employees from Excel.
/// Includes optional ID for update scenarios.
/// </summary>
public class EmployeeImportDto
{
    /// <summary>
    /// Gets or sets the optional ID of the employee (for updates).
    /// If provided and exists, the employee will be updated instead of created.
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Gets or sets the first name of the employee.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name of the employee.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address of the employee.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the phone number of the employee.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the address of the employee.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the position or job title of the employee.
    /// </summary>
    public string Position { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the salary of the employee.
    /// </summary>
    public decimal Salary { get; set; }

    /// <summary>
    /// Gets or sets the hire date of the employee.
    /// </summary>
    public DateTime HireDate { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the department to which the employee belongs.
    /// </summary>
    public int DepartmentId { get; set; }
}

