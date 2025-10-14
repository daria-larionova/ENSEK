using ENSEK.API.DTOs;
using ENSEK.API.Models;

namespace ENSEK.API.Validators;

public interface IMeterReadingValidator
{
    Task<ValidationResult> ValidateAsync(MeterReadingCsvRow record, int lineNumber);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public int? AccountId { get; set; }
    public DateTime? DateTime { get; set; }
    public string? MeterReadValue { get; set; }
    public string? ErrorMessage { get; set; }
}
