using AutoMapper;
using SGE.Application.DTOs;
using SGE.Application.Interfaces.Repositories;
using SGE.Application.Interfaces.Services;
using SGE.Core.Entities;
using SGE.Core.Enums;
using SGE.Core.Exceptions;

namespace SGE.Application.Services;

public class LeaveRequestService(
    IEmployeeRepository employeeRepository,
    ILeaveRequestRepository leaveRequestRepository,
    IMapper mapper)
    : ILeaveRequestService
{
    /// <summary>
    /// Creates a new leave request in the system asynchronously.
    /// </summary>
    /// <param name="dto">
    /// The data required to create a new leave request, including employee ID, leave type, start date, end date, and reason.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// The details of the created leave request wrapped in a LeaveRequestDto.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the employee with the specified ID is not found.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the date range is invalid (end date before start date).
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if there is a conflicting leave request for the specified employee within the date range.
    /// </exception>
    public async Task<LeaveRequestDto> CreateAsync(LeaveRequestCreateDto dto, CancellationToken cancellationToken = default)
    {
        // Vérifier que l'employé existe
        if (!await employeeRepository.ExistsAsync(dto.EmployeeId, cancellationToken))
            throw new EmployeeNotFoundException(dto.EmployeeId);

        // Valider les dates
        if (dto.EndDate < dto.StartDate)
            throw new ValidationException("EndDate", "La date de fin doit être supérieure à la date de début.");

        if (dto.StartDate.Date < DateTime.UtcNow.Date)
            throw new ValidationException("StartDate", "La date de début doit être supérieure ou égale à la date du jour.");

        // Convertir les dates en UTC pour la vérification des conflits
        var startDateUtc = dto.StartDate.Kind == DateTimeKind.Utc 
            ? dto.StartDate 
            : dto.StartDate.Kind == DateTimeKind.Local 
                ? dto.StartDate.ToUniversalTime() 
                : DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc);
        
        var endDateUtc = dto.EndDate.Kind == DateTimeKind.Utc 
            ? dto.EndDate 
            : dto.EndDate.Kind == DateTimeKind.Local 
                ? dto.EndDate.ToUniversalTime() 
                : DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc);

        // Vérifier les conflits de congés
        var hasConflict = await HasConflictingLeaveAsync(dto.EmployeeId, startDateUtc, endDateUtc, null, cancellationToken);
        if (hasConflict)
            throw new ConflictingLeaveRequestException(dto.StartDate, dto.EndDate);

        var entity = mapper.Map<LeaveRequest>(dto);
        entity.DaysRequested = CalculateBusinessDays(dto.StartDate, dto.EndDate);
        entity.Status = LeaveStatus.Pending;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.CreatedBy = "System";
        entity.UpdatedBy = "System";

        await leaveRequestRepository.AddAsync(entity, cancellationToken);

        // Recharger l'entité avec les relations
        var result = await leaveRequestRepository.GetByIdAsync(entity.Id, cancellationToken);
        return mapper.Map<LeaveRequestDto>(result);
    }

    /// <summary>
    /// Retrieves the details of a leave request by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the leave request to be retrieved.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// The details of the leave request wrapped in a LeaveRequestDto, or null if no leave request with the specified ID exists.
    /// </returns>
    public async Task<LeaveRequestDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var leaveRequest = await leaveRequestRepository.GetByIdAsync(id, cancellationToken);
        if (leaveRequest == null)
            throw new LeaveRequestNotFoundException(id);
        
        return mapper.Map<LeaveRequestDto>(leaveRequest);
    }

    /// <summary>
    /// Retrieves the leave requests associated with a specific employee asynchronously.
    /// </summary>
    /// <param name="employeeId">
    /// The unique identifier of the employee whose leave requests are to be retrieved.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A collection of leave request details wrapped in LeaveRequestDto objects.
    /// </returns>
    public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var leaveRequests = await leaveRequestRepository.GetByEmployeeAsync(employeeId, cancellationToken);
        return mapper.Map<IEnumerable<LeaveRequestDto>>(leaveRequests);
    }

    /// <summary>
    /// Retrieves a collection of leave requests based on the specified status asynchronously.
    /// </summary>
    /// <param name="status">
    /// The status of the leave requests to filter by, such as Pending, Approved, Rejected, or Cancelled.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A collection of leave requests matching the specified status, wrapped in LeaveRequestDto objects.
    /// </returns>
    public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByStatusAsync(LeaveStatus status, CancellationToken cancellationToken = default)
    {
        var leaveRequests = await leaveRequestRepository.FindAsync(lr => lr.Status == status, cancellationToken);
        return mapper.Map<IEnumerable<LeaveRequestDto>>(leaveRequests);
    }

    /// <summary>
    /// Retrieves all leave requests with a status of pending asynchronously.
    /// </summary>
    /// <returns>
    /// A collection of leave requests that are currently pending, wrapped in LeaveRequestDto objects.
    /// </returns>
    public async Task<IEnumerable<LeaveRequestDto>> GetPendingLeaveRequestsAsync()
    {
        return await GetLeaveRequestsByStatusAsync(LeaveStatus.Pending);
    }

    /// <summary>
    /// Updates the status of an existing leave request asynchronously.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the leave request to be updated.
    /// </param>
    /// <param name="dto">
    /// An object containing the updated status and optional manager comments for the leave request.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A boolean value indicating whether the operation was successful.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the leave request with the specified ID is not found.
    /// </exception>
    public async Task<bool> UpdateStatusAsync(int id, LeaveRequestUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var leaveRequest = await leaveRequestRepository.GetByIdAsync(id, cancellationToken);
        if (leaveRequest == null)
            throw new LeaveRequestNotFoundException(id);

        leaveRequest.Status = dto.Status;
        leaveRequest.ManagerComments = dto.ManagerComments;
        leaveRequest.ReviewedAt = DateTime.UtcNow;
        leaveRequest.UpdatedAt = DateTime.UtcNow;
        leaveRequest.UpdatedBy = "System";

        await leaveRequestRepository.UpdateAsync(leaveRequest, cancellationToken);
        return true;
    }

    /// <summary>
    /// Retrieves the remaining leave days for a specific employee in a given year asynchronously.
    /// </summary>
    /// <param name="employeeId">
    /// The unique identifier of the employee for whom the remaining leave days are being retrieved.
    /// </param>
    /// <param name="year">
    /// The year for which the remaining leave days are being calculated.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// The total number of remaining leave days for the specified employee and year.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the employee with the specified ID is not found.
    /// </exception>
    public async Task<int> GetRemainingLeaveDaysAsync(int employeeId, int year, CancellationToken cancellationToken = default)
    {
        if (!await employeeRepository.ExistsAsync(employeeId, cancellationToken))
            throw new EmployeeNotFoundException(employeeId);

        // Nombre de jours de congé par défaut (à ajuster selon vos besoins)
        const int defaultAnnualLeaveDays = 25;

        var startDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        // Récupérer les congés approuvés pour l'année
        var leaveRequests = await leaveRequestRepository.FindAsync(
            lr => lr.EmployeeId == employeeId 
                  && lr.Status == LeaveStatus.Approved 
                  && lr.StartDate >= startDate 
                  && lr.EndDate <= endDate,
            cancellationToken);

        var usedDays = leaveRequests.Sum(lr => lr.DaysRequested);
        return Math.Max(0, defaultAnnualLeaveDays - usedDays);
    }

    /// <summary>
    /// Checks if there are any conflicting leave requests for an employee within the specified date range.
    /// </summary>
    /// <param name="employeeId">
    /// The ID of the employee for whom the check is being performed.
    /// </param>
    /// <param name="startDate">
    /// The start date of the leave period to verify for conflicts.
    /// </param>
    /// <param name="endDate">
    /// The end date of the leave period to verify for conflicts.
    /// </param>
    /// <param name="excludeRequestId">
    /// An optional leave request ID to exclude from the conflict check, typically used when updating an existing leave request.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests during the operation.
    /// </param>
    /// <returns>
    /// A boolean value indicating whether any conflicting leave requests exist.
    /// </returns>
    public async Task<bool> HasConflictingLeaveAsync(int employeeId, DateTime startDate, DateTime endDate, int? excludeRequestId = null, CancellationToken cancellationToken = default)
    {
        // Convertir les dates en UTC pour la comparaison avec la base de données
        var startDateUtc = startDate.Kind == DateTimeKind.Utc 
            ? startDate 
            : startDate.Kind == DateTimeKind.Local 
                ? startDate.ToUniversalTime() 
                : DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        
        var endDateUtc = endDate.Kind == DateTimeKind.Utc 
            ? endDate 
            : endDate.Kind == DateTimeKind.Local 
                ? endDate.ToUniversalTime() 
                : DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        var leaveRequests = await leaveRequestRepository.FindAsync(
            lr => lr.EmployeeId == employeeId 
                  && (lr.Status == LeaveStatus.Pending || lr.Status == LeaveStatus.Approved)
                  && lr.StartDate <= endDateUtc 
                  && lr.EndDate >= startDateUtc,
            cancellationToken);

        if (excludeRequestId.HasValue)
            leaveRequests = leaveRequests.Where(lr => lr.Id != excludeRequestId.Value);

        return leaveRequests.Any();
    }

    /// <summary>
    /// Calculates the number of business days between two dates (excluding weekends).
    /// </summary>
    /// <param name="startDate">The start date of the period.</param>
    /// <param name="endDate">The end date of the period.</param>
    /// <returns>The number of business days between the two dates.</returns>
    private int CalculateBusinessDays(DateTime startDate, DateTime endDate)
    {
        int businessDays = 0;
        DateTime current = startDate;

        while (current <= endDate)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                businessDays++;

            current = current.AddDays(1);
        }

        return businessDays;
    }
}

