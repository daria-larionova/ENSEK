using ENSEK.API.Data;
using ENSEK.API.DTOs;
using ENSEK.API.Models;
using ENSEK.API.Validators;
using Microsoft.EntityFrameworkCore;

namespace ENSEK.API.Services;

public class MeterReadingService : IMeterReadingService
{
    private readonly ApplicationDbContext _context;
    private readonly ICsvParserService _csvParser;
    private readonly IMeterReadingValidator _validator;
    private readonly ILogger<MeterReadingService> _logger;

    public MeterReadingService(
        ApplicationDbContext context,
        ICsvParserService csvParser,
        IMeterReadingValidator validator,
        ILogger<MeterReadingService> logger)
    {
        _context = context;
        _csvParser = csvParser;
        _validator = validator;
        _logger = logger;
    }

    public async Task<MeterReadingUploadResult> ProcessMeterReadingsAsync(Stream csvStream)
    {
        var result = new MeterReadingUploadResult();

        try
        {
            var records = await _csvParser.ParseCsvAsync(csvStream);
            var validReadings = await ProcessRecords(records, result);

            if (validReadings.Any())
            {
                // TODO: Add a transaction to the database, but InMemoryDatabase doesn't support transactions
                await _context.MeterReadings.AddRangeAsync(validReadings);
                await _context.SaveChangesAsync();
                result.SuccessfulReadings = validReadings.Count;
            }

            _logger.LogInformation("Processed {TotalRecords} meter readings: {SuccessCount} successful, {FailureCount} failed",
                records.Count, result.SuccessfulReadings, result.FailedReadings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing meter readings CSV file");
            result.Errors.Add($"Error processing CSV file: {ex.Message}");
        }

        return result;
    }

    private async Task<List<MeterReading>> ProcessRecords(List<MeterReadingCsvRow> records, MeterReadingUploadResult result)
    {
        var validReadings = new List<MeterReading>();

        for (int i = 0; i < records.Count; i++)
        {
            var record = records[i];
            var lineNumber = i + 2; // +2 because: index is 0-based (first record = 0) and +1 for header row
            
            try
            {
                var validationResult = await _validator.ValidateAsync(record, lineNumber);
                
                if (validationResult.IsValid)
                {
                    validReadings.Add(new MeterReading
                    {
                        AccountId = validationResult.AccountId!.Value,
                        MeterReadingDateTime = validationResult.DateTime!.Value,
                        MeterReadValue = validationResult.MeterReadValue!
                    });
                }
                else
                {
                    result.FailedReadings++;
                    result.Errors.Add(validationResult.ErrorMessage!);
                }
            }
            catch (Exception ex)
            {
                result.FailedReadings++;
                result.Errors.Add($"Line {lineNumber}: Error validating record - {ex.Message}");
                _logger.LogError(ex, "Error validating record at line {LineNumber}", lineNumber);
            }
        }

        return validReadings;
    }

    public async Task ClearAllReadingsAsync()
    {
        var count = await _context.MeterReadings.CountAsync();
        
        // TODO: Add a transaction to the database, but InMemoryDatabase doesn't support transactions
        _context.MeterReadings.RemoveRange(_context.MeterReadings);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Successfully cleared {Count} meter readings from database", count);
    }
}
