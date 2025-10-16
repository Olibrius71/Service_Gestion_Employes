namespace SGE.Application.DTOs;

public class EmployeeCreateDto
{
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
    /// Gets or sets the date the employee was hired.
    /// </summary>
    public DateTime HireDate { get; set; }

    /// <summary>
    /// Gets or sets the department ID.
    /// </summary>
    public int DepartmentId { get; set; }
}

