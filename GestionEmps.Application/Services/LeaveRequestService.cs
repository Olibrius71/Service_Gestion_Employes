using AutoMapper;
using GestionEmps.Application.DTOs;
using GestionEmps.Application.Interfaces.Repositories;
using GestionEmps.Application.Interfaces.Services;
using GestionEmps.Core.Entities;
using GestionEmps.Core.Enums;
using GestionEmps.Core.Exceptions;

namespace GestionEmps.Application.Services;

public class LeaveRequestService(IEmployeeRepository employeeRepository, ILeaveRequestRepository leaveRequestRepository, IMapper mapper) : ILeaveRequestService
{    
     /// <summary>
     /// Creates a new leave request asynchronously.
     /// </summary>
     /// <param name="dto">
     /// The data transfer object containing the details of the leave request to be created.
     /// </param>
     /// <param name="cancellationToken">
     /// A token to monitor for cancellation requests.
     /// </param>
     /// <returns>
     /// The details of the newly created leave request, wrapped in a LeaveRequestDto.
     /// </returns>
     /// <exception cref="KeyNotFoundException">
     /// Thrown if the referenced employee does not exist.
     /// </exception>
     /// <exception cref="InvalidOperationException">
     /// Thrown if the leave request has invalid dates, such as an end date earlier than the start date,
     /// a start date in the past, or conflicting with an existing approved leave request.
     /// </exception>
     public async Task<LeaveRequestDto> CreateAsync(LeaveRequestCreateDto dto, CancellationToken cancellationToken = default)
     {
          var employee = await employeeRepository.GetByIdAsync(dto.EmployeeId, cancellationToken);
          if (employee is null)
               throw new EmployeeNotFoundException(dto.EmployeeId);
          
          if (dto.EndDate < dto.StartDate)
               throw new ValidationException("EndDate", "La date de fin doit être supérieure à la date de début.");
          
          if (dto.StartDate < DateTime.Today)
               throw new ValidationException("StartDate", "La date de début doit être supérieure ou égale à la date de jour.");
          
          var daysRequested = CalculateBusinessDays(dto.StartDate, dto.EndDate);
          var hasConflict = await HasConflictingLeaveAsync(dto.EmployeeId, dto.StartDate, dto.EndDate, cancellationToken: cancellationToken);
          
          if (hasConflict)
               throw new ConflictingLeaveRequestException(dto.StartDate, dto.EndDate);
          
          var entity = mapper.Map<LeaveRequest>(dto);
          entity.DaysRequested = daysRequested;
          
          await leaveRequestRepository.AddAsync(entity, cancellationToken);
          
          return mapper.Map<LeaveRequestDto>(entity);
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
     /// <exception cref="NotImplementedException">
     /// Thrown if the method is not implemented.
     /// </exception>
     public async Task<LeaveRequestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
     {
          var entity = await leaveRequestRepository.GetByIdAsync(id, cancellationToken);
          if (entity == null)
          {
               throw new LeaveRequestNotFoundException(id);
          }
          
          return mapper.Map<LeaveRequestDto>(entity);
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
     /// <exception cref="NotImplementedException">
     /// Thrown if the method is not implemented.
     /// </exception>
     public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByEmployeeAsync(int employeeId, CancellationToken cancellationToken = default)
     {
          // Vérifier que l'employé existe
          if (!await employeeRepository.ExistsAsync(employeeId, cancellationToken))
          {
               throw new EmployeeNotFoundException(employeeId);
          }

          var requests = await leaveRequestRepository.GetByEmployeeAsync(employeeId, cancellationToken);
          return requests.Select(r => mapper.Map<LeaveRequestDto>(r));
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
     /// <exception cref="NotImplementedException">
     /// Thrown if the method is not implemented.
     /// </exception>
     public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByStatusAsync(LeaveStatus status, CancellationToken cancellationToken = default)
     {
          var allRequests = await leaveRequestRepository.GetAllAsync(cancellationToken);

          var filtered = allRequests.Where(r => r.Status == status);

          return filtered.Select(r => mapper.Map<LeaveRequestDto>(r));
     }
     
     /// <summary>
     /// Retrieves all leave requests with a status of pending asynchronously.
     /// </summary>
     /// <returns>
     /// A collection of leave requests that are currently pending, wrapped in LeaveRequestDto objects.
     /// </returns>
     /// <exception cref="NotImplementedException">
     /// Thrown if the method is not implemented.
     /// </exception>
     public async Task<IEnumerable<LeaveRequestDto>> GetPendingLeaveRequestsAsync()
     {
          var allRequests = await leaveRequestRepository.GetAllAsync();

          var pending = allRequests.Where(r => r.Status == LeaveStatus.Pending);

          return pending.Select(r => mapper.Map<LeaveRequestDto>(r));
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
     /// <exception cref="NotImplementedException">
     /// Thrown if the method is not implemented.
     /// </exception>
     public async Task<bool> UpdateStatusAsync(int id, LeaveRequestUpdateDto dto, CancellationToken cancellationToken = default)
     {

          var request = await leaveRequestRepository.GetByIdAsync(id, cancellationToken);
          if (request == null)
               throw new LeaveRequestNotFoundException(id);
          
          if (request.Status == LeaveStatus.Approved ||
              request.Status == LeaveStatus.Rejected ||
              request.Status == LeaveStatus.Cancelled) 
               throw new InvalidLeaveStatusTransitionException(request.Status.ToString(), dto.Status.ToString());
          
          request.Status = dto.Status;
          request.ManagerComments = dto.ManagerComments;
          request.UpdatedAt = DateTime.UtcNow;

          await leaveRequestRepository.UpdateAsync(request, cancellationToken);
          
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
     /// <exception cref="NotImplementedException">
     /// Thrown if the method is not implemented.
     /// </exception>
     public async Task<int> GetRemainingLeaveDaysAsync(int employeeId, int year, CancellationToken cancellationToken = default)
     {
          if (!await employeeRepository.ExistsAsync(employeeId, cancellationToken))
          {
               throw new EmployeeNotFoundException(employeeId);
          }
          
          const int AnnualLeaveAllowance = 22;
          
          var approvedLeaves = await leaveRequestRepository.GetByEmployeeAsync(employeeId, cancellationToken);
          var usedDays = approvedLeaves
               .Where(l => l.Status == LeaveStatus.Approved && l.StartDate.Year == year)
               .Sum(l => l.DaysRequested);

          return AnnualLeaveAllowance - usedDays;
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
     /// <exception cref="NotImplementedException">
     /// Thrown if the method is not implemented.
     /// </exception>
     
     public async Task<bool> HasConflictingLeaveAsync(int employeeId, DateTime startDate, DateTime endDate, int? excludeRequestId = null, CancellationToken cancellationToken = default)
     {
          if (!await employeeRepository.ExistsAsync(employeeId, cancellationToken))
          {
               throw new EmployeeNotFoundException(employeeId);
          }
          
          var requests = await leaveRequestRepository.GetByEmployeeAsync(employeeId, cancellationToken);

          return requests.Any(r =>
               r.Status != LeaveStatus.Rejected &&
               r.Status != LeaveStatus.Cancelled &&
               (excludeRequestId == null || r.Id != excludeRequestId) &&
               r.StartDate <= endDate &&
               r.EndDate >= startDate
          );
     }
     
     private int CalculateBusinessDays(DateTime startDate, DateTime endDate)
     {
          
          if (endDate < startDate)
               throw new ArgumentException("End date must be greater than or equal to start date.");
          
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