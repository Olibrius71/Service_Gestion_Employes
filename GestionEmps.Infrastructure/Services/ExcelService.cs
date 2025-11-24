using GestionEmps.Application.Interfaces.InfrastructureServices;

namespace GestionEmps.Infrastructure.Services;

public class ExcelService : IExcelService
{
    public bool IsFileExcel(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return false;

        string[] allowedExtensions = [".xlsx", ".xls"];
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        return allowedExtensions.Contains(fileExtension);
    }
}