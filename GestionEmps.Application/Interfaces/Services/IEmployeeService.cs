using SGE.Application.DTOs;

namespace SGE.Application.Interfaces.Services;

/// <summary>
/// Defines the service contract for managing employees.
/// </summary>
public interface IEmployeeService
{
    /// <summary>
    /// Asynchronously retrieves all employees.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an enumerable collection of <see cref="EmployeeDto"/>.</returns>
    Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves an employee by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the employee to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the <see cref="EmployeeDto"/> for the specified identifier, or null if not found.</returns>
    Task<EmployeeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a paginated list of employees.
    /// </summary>
    /// <param name="pageIndex">The page index (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an enumerable collection of <see cref="EmployeeDto"/>.</returns>
    Task<IEnumerable<EmployeeDto>> GetPagedAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously creates a new employee.
    /// </summary>
    /// <param name="dto">The data transfer object containing the details of the employee to create.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the newly created <see cref="EmployeeDto"/>.</returns>
    Task<EmployeeDto> CreateAsync(EmployeeCreateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously updates an existing employee with the specified identifier and update details.
    /// </summary>
    /// <param name="id">The unique identifier of the employee to update.</param>
    /// <param name="dto">The data transfer object containing the updated details of the employee.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a boolean value indicating whether the update was successful.</returns>
    Task<bool> UpdateAsync(int id, EmployeeUpdateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously deletes an employee by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the employee to delete.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result indicates whether the deletion was successful.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an employee by their email address asynchronously.
    /// </summary>
    /// <param name="email">The email address of the employee to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the employee data transfer object (EmployeeDto) if found, otherwise null.
    /// </returns>
    Task<EmployeeDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves employees by their department identifier asynchronously.
    /// </summary>
    /// <param name="departmentId">The unique identifier of the department to filter employees by.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an enumerable collection of employee data transfer objects (EmployeeDto).
    /// </returns>
    Task<IEnumerable<EmployeeDto>> GetByDepartmentAsync(int departmentId, CancellationToken cancellationToken = default);
}

