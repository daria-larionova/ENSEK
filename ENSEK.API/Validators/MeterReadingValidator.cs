using ENSEK.API.Data;
using ENSEK.API.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ENSEK.API.Validators;

public class MeterReadingValidator : IMeterReadingValidator
{
    private readonly ApplicationDbContext _context;

    public MeterReadingValidator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ValidationResult> ValidateAsync(MeterReadingCsvRow record, int lineNumber)
    {
        if (!int.TryParse(record.AccountId, out int accountId))
            return new ValidationResult { ErrorMessage = $"Line {lineNumber}: AccountId must be a valid integer" };

        if (!await _context.Accounts.AnyAsync(a => a.AccountId == accountId))
            return new ValidationResult { ErrorMessage = $"Line {lineNumber}: AccountId {accountId} does not exist in the system" };

        if (!DateTime.TryParseExact(record.MeterReadingDateTime,
            new[] { "dd/MM/yyyy HH:mm", "dd/MM/yyyy H:mm", "d/M/yyyy HH:mm", "d/M/yyyy H:mm" },
            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
            return new ValidationResult { ErrorMessage = $"Line {lineNumber}: MeterReadingDateTime must be in format: dd/MM/yyyy HH:mm" };

        if (await _context.MeterReadings.AnyAsync(mr => mr.AccountId == accountId && mr.MeterReadingDateTime == dateTime))
            return new ValidationResult { ErrorMessage = $"Line {lineNumber}: Duplicate reading for AccountId {accountId} at {dateTime:dd/MM/yyyy HH:mm}" };

        var latestDate = await _context.MeterReadings
            .Where(mr => mr.AccountId == accountId)
            .MaxAsync(mr => (DateTime?)mr.MeterReadingDateTime);

        if (latestDate.HasValue && dateTime < latestDate.Value)
            return new ValidationResult { ErrorMessage = $"Line {lineNumber}: Reading date {dateTime:dd/MM/yyyy HH:mm} is older than existing reading {latestDate.Value:dd/MM/yyyy HH:mm} for AccountId {accountId}" };

        if (string.IsNullOrWhiteSpace(record.MeterReadValue))
            return new ValidationResult { ErrorMessage = $"Line {lineNumber}: MeterReadValue is required" };

        var trimmed = record.MeterReadValue.Trim();
        if (trimmed.StartsWith("-") || !System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^\d{1,5}$"))
            return new ValidationResult { ErrorMessage = $"Line {lineNumber}: MeterReadValue must be numeric and up to 5 digits" };

        return new ValidationResult
        {
            IsValid = true,
            AccountId = accountId,
            DateTime = dateTime,
            MeterReadValue = trimmed.PadLeft(5, '0')
        };
    }
}
