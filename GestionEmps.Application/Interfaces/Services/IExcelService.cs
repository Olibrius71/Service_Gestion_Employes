namespace SGE.Application.Interfaces.Services;

/// <summary>
/// Provides functionality for importing and exporting employee data to and from Excel files.
/// </summary>
public interface IExcelService
{
    /// <summary>
    /// Exports a collection of employee data transfer objects to an Excel file.
    /// </summary>
    /// <param name="employees">The collection of employee DTOs to export.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a byte array representing the Excel file content.
    /// </returns>
    Task<byte[]> ExportEmployeesToExcelAsync(IEnumerable<DTOs.EmployeeDto> employees, CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports employee data from an Excel file and returns a collection of employee import DTOs.
    /// Supports both create and update scenarios based on the presence of an ID.
    /// </summary>
    /// <param name="fileStream">The stream containing the Excel file data.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a collection of EmployeeImportDto objects parsed from the Excel file.
    /// </returns>
    /// <exception cref="ApplicationException">Thrown if the file format is invalid or contains invalid data.</exception>
    Task<IEnumerable<DTOs.EmployeeImportDto>> ImportEmployeesFromExcelAsync(Stream fileStream, CancellationToken cancellationToken = default);
}

