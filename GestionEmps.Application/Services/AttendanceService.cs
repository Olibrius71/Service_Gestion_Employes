using AutoMapper;
using SGE.Application.DTOs;
using SGE.Application.Interfaces.Repositories;
using SGE.Application.Interfaces.Services;
using SGE.Core.Entities;
using SGE.Core.Exceptions;

namespace SGE.Application.Services;

/// <summary>
/// Provides functionalities related to managing employee attendances, including clocking in/out,
/// retrieving attendance records, and calculating worked hours for employees.
/// </summary>
public class AttendanceService(
    IAttendanceRepository attendanceRepository,
    IEmployeeRepository employeeRepository,
    IMapper mapper) : IAttendanceService
{
    /// <summary>
    /// Registers the clock-in time for an employee. If an attendance record for the employee on the specified day
    /// already exists without a clock-in time, it updates the record with the provided clock-in information.
    /// Otherwise, it creates a new attendance record.
    /// </summary>
    /// <param name="clockInDto">An object containing the employee's ID and clock-in time information.</param>
    /// <param name="cancellationToken">A token to observe during the asynchronous operation for cancellation.</param>
    /// <returns>A DTO containing the attendance details after the clock-in operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the employee corresponding to the specified ID is not found.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the employee has already clocked in on the same day or when there is an issue creating or updating the attendance record.
    /// </exception>
    public async Task<AttendanceDto> ClockInAsync(ClockInOutDto clockInDto, CancellationToken cancellationToken = default)
    {
        if (!await employeeRepository.ExistsAsync(clockInDto.EmployeeId, cancellationToken))
            throw new EmployeeNotFoundException(clockInDto.EmployeeId);

        var date = clockInDto.DateTime.Date;
        var time = clockInDto.DateTime.TimeOfDay;

        // Vérifier s'il existe déjà une entrée pour la date
        var attendances = await attendanceRepository.FindAsync(
            a => a.EmployeeId == clockInDto.EmployeeId && a.Date == date,
            cancellationToken);
        
        var attendance = attendances.FirstOrDefault();

        if (attendance != null)
        {
            // Vérifier si l'employé a déjà pointé
            if (attendance.ClockIn.HasValue)
                throw new AlreadyClockedInException(clockInDto.EmployeeId);

            // Mettre à jour l'entrée existante
            attendance.ClockIn = time;
            attendance.Notes += (string.IsNullOrEmpty(attendance.Notes) ? "" : "; ") + clockInDto.Notes;
            attendance.UpdatedAt = DateTime.UtcNow;
            attendance.UpdatedBy = "System";
            
            await attendanceRepository.UpdateAsync(attendance, cancellationToken);
        }
        else
        {
            // Créer une nouvelle entrée de présence
            attendance = new Attendance
            {
                EmployeeId = clockInDto.EmployeeId,
                Date = date,
                ClockIn = time,
                Notes = clockInDto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            };

            await attendanceRepository.AddAsync(attendance, cancellationToken);
        }

        // Recharger l'entité avec les relations
        var result = await attendanceRepository.GetByIdAsync(attendance.Id, cancellationToken);
        return mapper.Map<AttendanceDto>(result);
    }

    /// <summary>
    /// Registers the clock-out time for an employee. It updates the existing attendance record for the employee
    /// on the specified day by adding the clock-out information. The record must already contain a clock-in time.
    /// </summary>
    /// <param name="clockOutDto">An object containing the employee's ID and clock-out time information.</param>
    /// <param name="cancellationToken">A token to observe during the asynchronous operation for cancellation.</param>
    /// <returns>A DTO containing the updated attendance details after the clock-out operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the employee corresponding to the specified ID is not found.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no clock-in record exists for the specified day, when the employee has not clocked in prior to clocking out,
    /// or when the employee has already clocked out on the same day.
    /// </exception>
    public async Task<AttendanceDto> ClockOutAsync(ClockInOutDto clockOutDto, CancellationToken cancellationToken = default)
    {
        // Vérifier que l'employé existe
        if (!await employeeRepository.ExistsAsync(clockOutDto.EmployeeId, cancellationToken))
            throw new EmployeeNotFoundException(clockOutDto.EmployeeId);

        var date = clockOutDto.DateTime.Date;
        var time = clockOutDto.DateTime.TimeOfDay;

        // Vérifier s'il existe déjà une entrée pour la date
        var attendances = await attendanceRepository.FindAsync(
            a => a.EmployeeId == clockOutDto.EmployeeId && a.Date == date,
            cancellationToken);
        
        var attendance = attendances.FirstOrDefault();

        if (attendance == null)
            throw new NotClockedInException(clockOutDto.EmployeeId);

        if (!attendance.ClockIn.HasValue)
            throw new NotClockedInException(clockOutDto.EmployeeId);

        if (attendance.ClockOut.HasValue)
            throw new AttendanceException("L'employé a déjà pointé à la sortie aujourd'hui.", "ALREADY_CLOCKED_OUT");

        attendance.ClockOut = time;
        attendance.Notes += (string.IsNullOrEmpty(attendance.Notes) ? "" : "; ") + clockOutDto.Notes;
        attendance.UpdatedAt = DateTime.UtcNow;
        attendance.UpdatedBy = "System";

        // Calculer les heures travaillées
        CalculateWorkedHours(attendance);

        // Mettre à jour les informations de la présence
        await attendanceRepository.UpdateAsync(attendance, cancellationToken);

        // Recharger l'entité avec les relations
        var result = await attendanceRepository.GetByIdAsync(attendance.Id, cancellationToken);
        return mapper.Map<AttendanceDto>(result);
    }

    /// <summary>
    /// Creates a new attendance record for an employee based on the provided details. The worked hours
    /// and overtime hours are calculated automatically during the creation process.
    /// </summary>
    /// <param name="createAttendanceDto">An object containing the details needed to create the attendance record, including clock-in/out times and break duration.</param>
    /// <param name="cancellationToken">A token to observe during the asynchronous operation for cancellation.</param>
    /// <returns>A DTO containing the details of the newly created attendance record.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided attendance creation object is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if there is an issue during the creation of the attendance record or calculation of hours.</exception>
    public async Task<AttendanceDto> CreateAttendanceAsync(AttendanceCreateDto createAttendanceDto, CancellationToken cancellationToken = default)
    {
        // Vérifier que l'employé existe
        if (!await employeeRepository.ExistsAsync(createAttendanceDto.EmployeeId, cancellationToken))
            throw new EmployeeNotFoundException(createAttendanceDto.EmployeeId);

        // Vérifier s'il existe déjà une entrée pour cette date
        var existingAttendances = await attendanceRepository.FindAsync(
            a => a.EmployeeId == createAttendanceDto.EmployeeId && a.Date == createAttendanceDto.Date.Date,
            cancellationToken);

        if (existingAttendances.Any())
            throw new AttendanceAlreadyExistsException(createAttendanceDto.Date);

        var entity = mapper.Map<Attendance>(createAttendanceDto);
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.CreatedBy = "System";
        entity.UpdatedBy = "System";

        // Calculer les heures travaillées si les deux heures sont fournies
        CalculateWorkedHours(entity);

        await attendanceRepository.AddAsync(entity, cancellationToken);

        // Recharger l'entité avec les relations
        var result = await attendanceRepository.GetByIdAsync(entity.Id, cancellationToken);
        return mapper.Map<AttendanceDto>(result);
    }

    /// <summary>
    /// Retrieves an attendance record by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the attendance record to retrieve.</param>
    /// <param name="cancellationToken">A token to observe during the asynchronous operation for cancellation.</param>
    /// <returns>An object containing the attendance details, or null if no record is found with the specified ID.</returns>
    public async Task<AttendanceDto> GetAttendanceByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var att = await attendanceRepository.GetByIdAsync(id, cancellationToken);
        if (att == null)
            throw new AttendanceException($"Enregistrement de présence avec l'ID {id} introuvable.", "ATTENDANCE_NOT_FOUND");
        
        return mapper.Map<AttendanceDto>(att);
    }

    /// <summary>
    /// Retrieves the attendance records for a specific employee within an optional date range.
    /// If no date range is provided, it returns all attendance records for the employee.
    /// </summary>
    /// <param name="employeeId">The unique identifier of the employee for whom attendance records are being retrieved.</param>
    /// <param name="startDate">The start date of the range for filtering attendance records (optional).</param>
    /// <param name="endDate">The end date of the range for filtering attendance records (optional).</param>
    /// <param name="cancellationToken">A token to observe during the asynchronous operation for cancellation.</param>
    /// <returns>A collection of attendance records for the specified employee within the date range, if provided.</returns>
    public async Task<IEnumerable<AttendanceDto>> GetAttendancesByEmployeeAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var attendances = await attendanceRepository.GetByEmployeeAsync(employeeId, cancellationToken);

        // Filtrer par dates si fournies
        if (startDate.HasValue)
        {
            var startDateUtc = startDate.Value.Kind == DateTimeKind.Utc 
                ? startDate.Value.Date 
                : startDate.Value.ToUniversalTime().Date;
            attendances = attendances.Where(a => a.Date >= startDateUtc);
        }

        if (endDate.HasValue)
        {
            var endDateUtc = endDate.Value.Kind == DateTimeKind.Utc 
                ? endDate.Value.Date 
                : endDate.Value.ToUniversalTime().Date;
            attendances = attendances.Where(a => a.Date <= endDateUtc);
        }

        return mapper.Map<IEnumerable<AttendanceDto>>(attendances);
    }

    /// <summary>
    /// Retrieves the attendance records for all employees on the specified date.
    /// </summary>
    /// <param name="date">The date for which to retrieve attendance records.</param>
    /// <param name="cancellationToken">A token to observe during the asynchronous operation for cancellation.</param>
    /// <returns>A collection of attendance details for all employees on the specified date.</returns>
    public async Task<IEnumerable<AttendanceDto>> GetAttendancesByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var dateUtc = date.Kind == DateTimeKind.Utc 
            ? date.Date 
            : date.ToUniversalTime().Date;
        var attendances = await attendanceRepository.FindAsync(
            a => a.Date == dateUtc,
            cancellationToken);

        return mapper.Map<IEnumerable<AttendanceDto>>(attendances);
    }

    /// <summary>
    /// Retrieves the attendance record for an employee for the current date, if available.
    /// </summary>
    /// <param name="employeeId">The unique identifier of the employee whose attendance record is to be fetched.</param>
    /// <param name="cancellationToken">A token to observe during the asynchronous operation for cancellation.</param>
    /// <returns>An attendance DTO containing the details of the employee's attendance for the current date, or null if no record exists.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no employee is found with the specified ID.</exception>
    /// <exception cref="InvalidOperationException">Thrown when multiple attendance records for the employee are found for the current date.</exception>
    public async Task<AttendanceDto?> GetTodayAttendanceAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        if (!await employeeRepository.ExistsAsync(employeeId, cancellationToken))
            throw new EmployeeNotFoundException(employeeId);

        var today = DateTime.UtcNow.Date;
        var attendances = await attendanceRepository.FindAsync(
            a => a.EmployeeId == employeeId && a.Date == today,
            cancellationToken);

        var attendance = attendances.FirstOrDefault();
        return attendance == null ? null : mapper.Map<AttendanceDto>(attendance);
    }

    /// <summary>
    /// Calculates the total hours worked by an employee for a specific month and year.
    /// Aggregates attendance records for the given employee within the specified month
    /// to compute the total worked hours.
    /// </summary>
    /// <param name="employeeId">The unique identifier of the employee.</param>
    /// <param name="year">The year for which the worked hours are being calculated.</param>
    /// <param name="month">The month for which the worked hours are being calculated.</param>
    /// <param name="cancellationToken">A token to observe during the asynchronous operation for cancellation.</param>
    /// <returns>The total hours worked by the employee for the specified month and year.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no records are found for the specified employee.</exception>
    public async Task<decimal> GetMonthlyWorkedHoursAsync(int employeeId, int year, int month, CancellationToken cancellationToken = default)
    {
        if (!await employeeRepository.ExistsAsync(employeeId, cancellationToken))
            throw new EmployeeNotFoundException(employeeId);

        var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1).AddDays(-1).Date;
        endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59, DateTimeKind.Utc);

        var attendances = await attendanceRepository.FindAsync(
            a => a.EmployeeId == employeeId && a.Date >= startDate && a.Date <= endDate,
            cancellationToken);

        return attendances.Sum(a => a.WorkedHours);
    }

    /// <summary>
    /// Calculates the total worked hours and overtime hours for the specified attendance record based on the clock-in and clock-out times.
    /// If a break duration is provided, it is subtracted from the total worked time.
    /// </summary>
    /// <param name="attendance">The attendance record containing the clock-in, clock-out, and break duration information to calculate worked hours.</param>
    private void CalculateWorkedHours(Attendance attendance)
    {
        if (!attendance.ClockIn.HasValue || !attendance.ClockOut.HasValue)
            return;

        var totalWorked = attendance.ClockOut.Value - attendance.ClockIn.Value;

        // Soustraire la pause
        if (attendance.BreakDuration.HasValue)
            totalWorked -= attendance.BreakDuration.Value;

        var workedHours = (decimal)totalWorked.TotalHours;

        // Calculer les heures normales (8 heures par jour)
        const decimal normalWorkingHours = 8m;

        if (workedHours <= normalWorkingHours)
        {
            attendance.WorkedHours = Math.Max(0, workedHours);
            attendance.OvertimeHours = 0;
        }
        else
        {
            attendance.WorkedHours = normalWorkingHours;
            attendance.OvertimeHours = workedHours - normalWorkingHours;
        }
    }
}

