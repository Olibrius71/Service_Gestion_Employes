using System.Text.RegularExpressions;
using OfficeOpenXml;
using SGE.Application.DTOs;
using SGE.Application.Interfaces.Services;

namespace SGE.Application.Services;

/// <summary>
/// Provides implementation for importing and exporting employee data to and from Excel files.
/// </summary>
public class ExcelService : IExcelService
{
    /// <summary>
    /// Initializes a new instance of the ExcelService class.
    /// Sets the license context for EPPlus to non-commercial use.
    /// </summary>
    public ExcelService()
    {
        // Configure EPPlus license context (required for EPPlus 5+)
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    /// <summary>
    /// Validates if the provided string is a valid email address.
    /// </summary>
    /// <param name="email">The email string to validate.</param>
    /// <returns>True if the email is valid, false otherwise.</returns>
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Pattern regex pour validation d'email
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Exports a collection of employee data to an Excel file.
    /// </summary>
    /// <param name="employees">The collection of employees to export.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A byte array containing the Excel file data.</returns>
    public async Task<byte[]> ExportEmployeesToExcelAsync(IEnumerable<EmployeeDto> employees, CancellationToken cancellationToken = default)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Employees");

        // Headers
        worksheet.Cells[1, 1].Value = "ID";
        worksheet.Cells[1, 2].Value = "Prénom";
        worksheet.Cells[1, 3].Value = "Nom";
        worksheet.Cells[1, 4].Value = "Email";
        worksheet.Cells[1, 5].Value = "PhoneNumber";
        worksheet.Cells[1, 6].Value = "Address";
        worksheet.Cells[1, 7].Value = "Position";
        worksheet.Cells[1, 8].Value = "Salaire";
        worksheet.Cells[1, 9].Value = "HireDate";
        worksheet.Cells[1, 10].Value = "DepartmentId";
        worksheet.Cells[1, 11].Value = "Département";

        // Style headers
        using (var range = worksheet.Cells[1, 1, 1, 11])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        }

        // Data
        var employeeList = employees.ToList();
        for (int i = 0; i < employeeList.Count; i++)
        {
            var employee = employeeList[i];
            int row = i + 2; // Start from row 2 (after header)

            worksheet.Cells[row, 1].Value = employee.Id;
            worksheet.Cells[row, 2].Value = employee.FirstName;
            worksheet.Cells[row, 3].Value = employee.LastName;
            worksheet.Cells[row, 4].Value = employee.Email;
            worksheet.Cells[row, 5].Value = employee.PhoneNumber;
            worksheet.Cells[row, 6].Value = employee.Address;
            worksheet.Cells[row, 7].Value = employee.Position;
            worksheet.Cells[row, 8].Value = employee.Salary;
            worksheet.Cells[row, 9].Value = employee.HireDate.ToString("yyyy-MM-dd");
            worksheet.Cells[row, 10].Value = employee.DepartmentId;
            worksheet.Cells[row, 11].Value = employee.DepartmentName;
        }

        // Auto-fit columns
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        // Return as byte array
        return await Task.FromResult(package.GetAsByteArray());
    }

    /// <summary>
    /// Imports employee data from an Excel file.
    /// Expected columns: ID (optional), FirstName, LastName, Email, PhoneNumber, Address, Position, Salary, HireDate, DepartmentId
    /// If ID is provided and exists, the employee will be updated instead of created.
    /// </summary>
    /// <param name="fileStream">The stream containing the Excel file.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A collection of EmployeeImportDto objects.</returns>
    /// <exception cref="ApplicationException">Thrown if the file format is invalid.</exception>
    public async Task<IEnumerable<EmployeeImportDto>> ImportEmployeesFromExcelAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        var employees = new List<EmployeeImportDto>();

        using var package = new ExcelPackage(fileStream);
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();

        if (worksheet == null)
            throw new ApplicationException("Le fichier Excel ne contient aucune feuille de calcul.");

        // Vérifier qu'il y a au moins une ligne d'en-tête et une ligne de données
        if (worksheet.Dimension == null || worksheet.Dimension.Rows < 2)
            throw new ApplicationException("Le fichier Excel est vide ou ne contient pas de données.");

        // Lire les en-têtes pour déterminer l'ordre des colonnes
        var headers = new Dictionary<string, int>();
        for (int col = 1; col <= worksheet.Dimension.Columns; col++)
        {
            var headerValue = worksheet.Cells[1, col].Value?.ToString()?.Trim().ToLower();
            if (!string.IsNullOrEmpty(headerValue))
            {
                headers[headerValue] = col;
            }
        }

        // Valider que les colonnes requises sont présentes
        var hasMinimumHeaders = headers.Keys.Any(h => h.Contains("nom") || h.Contains("name")) &&
                                headers.Keys.Any(h => h.Contains("email")) &&
                                headers.Keys.Any(h => h.Contains("department"));

        if (!hasMinimumHeaders)
            throw new ApplicationException("Le fichier Excel ne contient pas toutes les colonnes requises. " +
                "Colonnes attendues: Prénom, Nom, Email, PhoneNumber, Address, Position, Salaire, HireDate, DepartmentId");

        // Fonction helper pour obtenir l'index de colonne
        int GetColumnIndex(params string[] possibleNames)
        {
            foreach (var name in possibleNames)
            {
                if (headers.ContainsKey(name.ToLower()))
                    return headers[name.ToLower()];
            }
            return -1;
        }

        // Lire les données ligne par ligne
        for (int row = 2; row <= worksheet.Dimension.Rows; row++)
        {
            try
            {
                var idCol = GetColumnIndex("id");
                var firstNameCol = GetColumnIndex("prénom", "prenom", "firstname");
                var lastNameCol = GetColumnIndex("nom", "lastname");
                var emailCol = GetColumnIndex("email");
                var phoneCol = GetColumnIndex("téléphone", "telephone", "phone", "phonenumber");
                var addressCol = GetColumnIndex("adresse", "address");
                var positionCol = GetColumnIndex("position", "poste");
                var salaryCol = GetColumnIndex("salaire", "salary");
                var hireDateCol = GetColumnIndex("dateembauche", "hiredate", "date");
                var departmentIdCol = GetColumnIndex("departmentid", "départementid", "departement", "department");

                // Extraire l'ID si présent (pour update)
                int? employeeId = null;
                if (idCol > 0 && int.TryParse(worksheet.Cells[row, idCol].Value?.ToString(), out var parsedId) && parsedId > 0)
                {
                    employeeId = parsedId;
                }

                var employee = new EmployeeImportDto
                {
                    Id = employeeId,
                    FirstName = firstNameCol > 0 ? worksheet.Cells[row, firstNameCol].Value?.ToString()?.Trim() ?? "" : "",
                    LastName = lastNameCol > 0 ? worksheet.Cells[row, lastNameCol].Value?.ToString()?.Trim() ?? "" : "",
                    Email = emailCol > 0 ? worksheet.Cells[row, emailCol].Value?.ToString()?.Trim() ?? "" : "",
                    PhoneNumber = phoneCol > 0 ? worksheet.Cells[row, phoneCol].Value?.ToString()?.Trim() ?? "" : "",
                    Address = addressCol > 0 ? worksheet.Cells[row, addressCol].Value?.ToString()?.Trim() ?? "" : "",
                    Position = positionCol > 0 ? worksheet.Cells[row, positionCol].Value?.ToString()?.Trim() ?? "" : "",
                    Salary = salaryCol > 0 && decimal.TryParse(worksheet.Cells[row, salaryCol].Value?.ToString(), out var salary) ? salary : 0,
                    HireDate = hireDateCol > 0 && DateTime.TryParse(worksheet.Cells[row, hireDateCol].Value?.ToString(), out var hireDate) 
                        ? DateTime.SpecifyKind(hireDate, DateTimeKind.Utc) 
                        : DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                    DepartmentId = departmentIdCol > 0 && int.TryParse(worksheet.Cells[row, departmentIdCol].Value?.ToString(), out var deptId) ? deptId : 0
                };

                // Validation de base
                if (string.IsNullOrWhiteSpace(employee.FirstName))
                    throw new ApplicationException($"Ligne {row}: Le champ Prénom est obligatoire.");
                
                if (string.IsNullOrWhiteSpace(employee.LastName))
                    throw new ApplicationException($"Ligne {row}: Le champ Nom est obligatoire.");
                
                if (string.IsNullOrWhiteSpace(employee.Email))
                    throw new ApplicationException($"Ligne {row}: Le champ Email est obligatoire.");
                
                // Validation du format email
                if (!IsValidEmail(employee.Email))
                    throw new ApplicationException($"Ligne {row}: L'email '{employee.Email}' n'est pas valide.");
                
                if (employee.DepartmentId == 0)
                    throw new ApplicationException($"Ligne {row}: Le champ DepartmentId est obligatoire.");

                employees.Add(employee);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Erreur lors de la lecture de la ligne {row}: {ex.Message}", ex);
            }
        }

        return await Task.FromResult(employees);
    }
}

