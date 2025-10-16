using Microsoft.AspNetCore.Mvc;
using SGE.Application.DTOs;
using SGE.Application.Interfaces.Services;

namespace SGE.API.Controllers;

/// <summary>
/// The EmployeesController class handles HTTP requests related to employee operations
/// such as retrieving, creating, updating, and deleting employees.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EmployeesController(IEmployeeService employeeService) : ControllerBase
{
    /// <summary>
    /// Retrieves all employees or a paginated list.
    /// </summary>
    /// <param name="pageIndex">Optional page index for pagination (1-based).</param>
    /// <param name="pageSize">Optional page size for pagination.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An <see cref="ActionResult"/> containing an enumerable collection of <see cref="EmployeeDto"/>.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll([FromQuery] int? pageIndex, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        if (pageIndex.HasValue && pageSize.HasValue)
        {
            var pagedList = await employeeService.GetPagedAsync(pageIndex.Value, pageSize.Value, cancellationToken);
            return Ok(pagedList);
        }

        var list = await employeeService.GetAllAsync(cancellationToken);
        return Ok(list);
    }

    /// <summary>
    /// Retrieves a specific employee by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the employee to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An <see cref="ActionResult"/> containing the <see cref="EmployeeDto"/> of the specified employee if found, otherwise a NotFound result.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var employee = await employeeService.GetByIdAsync(id, cancellationToken);
        if (employee == null) return NotFound();
        return Ok(employee);
    }

    /// <summary>
    /// Retrieves an employee by their email address.
    /// </summary>
    /// <param name="email">The email address of the employee to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// An asynchronous task that returns an action result containing the employee's data transfer object (EmployeeDto) if found, otherwise a NotFound result.
    /// </returns>
    [HttpGet("by-email/{email}")]
    public async Task<ActionResult<EmployeeDto>> GetByEmail(string email, CancellationToken cancellationToken)
    {
        var employee = await employeeService.GetByEmailAsync(email, cancellationToken);
        if (employee == null) return NotFound();
        return Ok(employee);
    }

    /// <summary>
    /// Retrieves employees associated with a specific department.
    /// </summary>
    /// <param name="departmentId">The identifier of the department to retrieve employees for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// An asynchronous task that returns an action result containing an enumerable collection of EmployeeDto objects.
    /// </returns>
    [HttpGet("by-department/{departmentId:int}")]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetByDepartment(int departmentId, CancellationToken cancellationToken)
    {
        var employees = await employeeService.GetByDepartmentAsync(departmentId, cancellationToken);
        return Ok(employees);
    }

    /// <summary>
    /// Creates a new employee based on the provided data transfer object.
    /// </summary>
    /// <param name="dto">The data transfer object containing the details of the employee to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An <see cref="ActionResult"/> containing the created <see cref="EmployeeDto"/> with its unique identifier and other details.</returns>
    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create(EmployeeCreateDto dto, CancellationToken cancellationToken)
    {
        var created = await employeeService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing employee with the given identifier and updated details.
    /// </summary>
    /// <param name="id">The unique identifier of the employee to update.</param>
    /// <param name="dto">The data transfer object containing the updated details of the employee.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the update operation. Returns <see cref="NoContentResult"/> if successful, or <see cref="NotFoundResult"/> if the employee is not found.</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, EmployeeUpdateDto dto, CancellationToken cancellationToken)
    {
        var ok = await employeeService.UpdateAsync(id, dto, cancellationToken);
        if (!ok) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Deletes a specific employee by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the employee to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns NoContent if successful, otherwise NotFound if the employee does not exist.</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var ok = await employeeService.DeleteAsync(id, cancellationToken);
        if (!ok) return NotFound();
        return NoContent();
    }
}

