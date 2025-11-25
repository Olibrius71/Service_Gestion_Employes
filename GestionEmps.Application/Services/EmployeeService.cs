using AutoMapper;
using SGE.Application.DTOs;
using SGE.Application.Interfaces.Repositories;
using SGE.Application.Interfaces.Services;
using SGE.Core.Entities;
using SGE.Core.Exceptions;

namespace SGE.Application.Services;

/// <summary>
/// Provides services to manage employee-related operations.
/// Implements the <see cref="IEmployeeService"/> interface and uses the
/// <see cref="IEmployeeRepository"/> and <see cref="IDepartmentRepository"/> for data access.
/// </summary>
public class EmployeeService(IEmployeeRepository employeeRepository, IDepartmentRepository departmentRepository, IMapper mapper) : IEmployeeService
{
    /// <summary>
    /// Retrieves all employee records asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A collection of employee data transfer objects.</returns>
    public async Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await employeeRepository.GetAllWithDepartmentAsync(cancellationToken);
        return mapper.Map<IEnumerable<EmployeeDto>>(list);
    }

    /// <summary>
    /// Retrieves an employee record by its identifier asynchronously, including department information.
    /// </summary>
    /// <param name="id">The unique identifier of the employee.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An employee data transfer object if found; otherwise, null.</returns>
    public async Task<EmployeeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetWithDepartmentAsync(id, cancellationToken);
        if (employee == null)
            throw new EmployeeNotFoundException(id);
        
        return mapper.Map<EmployeeDto>(employee);
    }

    /// <summary>
    /// Retrieves a paginated list of employees.
    /// </summary>
    /// <param name="pageIndex">The page index (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A paginated collection of employee data transfer objects.</returns>
    public async Task<IEnumerable<EmployeeDto>> GetPagedAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var list = await employeeRepository.GetPagedAsync(pageIndex, pageSize, cancellationToken);
        return mapper.Map<IEnumerable<EmployeeDto>>(list);
    }

    /// <summary>
    /// Creates a new employee record asynchronously.
    /// </summary>
    /// <param name="dto">The data transfer object containing the details of the employee to be created.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The created employee data transfer object.</returns>
    /// <exception cref="ApplicationException">Thrown if the email already exists or department does not exist.</exception>
    public async Task<EmployeeDto> CreateAsync(EmployeeCreateDto dto, CancellationToken cancellationToken = default)
    {
        // Vérifier si l'email existe déjà
        var existingEmail = await employeeRepository.GetByEmailAsync(dto.Email, cancellationToken);
        if (existingEmail != null)
            throw new DuplicateEmployeeEmailException(dto.Email);

        // Vérifier si le département existe
        var departmentExists = await departmentRepository.ExistsAsync(dto.DepartmentId, cancellationToken);
        if (!departmentExists)
            throw new DepartmentNotFoundException(dto.DepartmentId);

        var entity = mapper.Map<Employee>(dto);
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.CreatedBy = "System";
        entity.UpdatedBy = "System";
        await employeeRepository.AddAsync(entity, cancellationToken);

        return mapper.Map<EmployeeDto>(entity);
    }

    /// <summary>
    /// Updates an existing employee record asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the employee to update.</param>
    /// <param name="dto">The data transfer object containing updated information for the employee.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A boolean value indicating whether the update was successful.</returns>
    /// <exception cref="ApplicationException">Thrown if the email already exists or department does not exist.</exception>
    public async Task<bool> UpdateAsync(int id, EmployeeUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await employeeRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            throw new EmployeeNotFoundException(id);

        // Si l'email est modifié, vérifier qu'il n'existe pas déjà
        if (!string.IsNullOrEmpty(dto.Email) && dto.Email != entity.Email)
        {
            var existingEmail = await employeeRepository.GetByEmailAsync(dto.Email, cancellationToken);
            if (existingEmail != null)
                throw new DuplicateEmployeeEmailException(dto.Email);
        }

        // Si le département est modifié, vérifier qu'il existe
        if (dto.DepartmentId.HasValue && dto.DepartmentId.Value != entity.DepartmentId)
        {
            var departmentExists = await departmentRepository.ExistsAsync(dto.DepartmentId.Value, cancellationToken);
            if (!departmentExists)
                throw new DepartmentNotFoundException(dto.DepartmentId.Value);
        }
        
        if (!string.IsNullOrEmpty(dto.FirstName))
            entity.FirstName = dto.FirstName;
        
        if (!string.IsNullOrEmpty(dto.LastName))
            entity.LastName = dto.LastName;
        
        if (!string.IsNullOrEmpty(dto.Email))
            entity.Email = dto.Email;
        
        if (!string.IsNullOrEmpty(dto.PhoneNumber))
            entity.PhoneNumber = dto.PhoneNumber;
        
        if (!string.IsNullOrEmpty(dto.Address))
            entity.Address = dto.Address;
        
        if (!string.IsNullOrEmpty(dto.Position))
            entity.Position = dto.Position;
        
        if (dto.Salary.HasValue)
            entity.Salary = dto.Salary.Value;
        
        if (dto.DepartmentId.HasValue)
            entity.DepartmentId = dto.DepartmentId.Value;
        
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = "System";
        
        // Sauvegarder les changements (l'entité est déjà trackée par EF Core)
        await employeeRepository.UpdateAsync(entity, cancellationToken);

        return true;
    }

    /// <summary>
    /// Deletes an employee record by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the employee to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A boolean indicating whether the deletion was successful.</returns>
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await employeeRepository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            throw new EmployeeNotFoundException(id);

        await employeeRepository.DeleteAsync(entity.Id, cancellationToken);
        return true;
    }

    /// <summary>
    /// Asynchronously retrieves an employee by their email address and maps it to an EmployeeDto.
    /// </summary>
    /// <param name="email">The email address of the employee to retrieve.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the EmployeeDto if found; otherwise, null.</returns>
    public async Task<EmployeeDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emp = await employeeRepository.GetByEmailAsync(email, cancellationToken);
        return emp == null ? null : mapper.Map<EmployeeDto>(emp);
    }

    /// <summary>
    /// Asynchronously retrieves employees belonging to a specific department and maps them to a collection of EmployeeDto.
    /// </summary>
    /// <param name="departmentId">The unique identifier of the department whose employees should be retrieved.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of EmployeeDto objects associated with the specified department.</returns>
    public async Task<IEnumerable<EmployeeDto>> GetByDepartmentAsync(int departmentId, CancellationToken cancellationToken = default)
    {
        var list = await employeeRepository.GetByDepartmentAsync(departmentId, cancellationToken);
        return mapper.Map<IEnumerable<EmployeeDto>>(list);
    }
}

