using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using ENSEK.API.DTOs;

namespace ENSEK.API.Services;

public class CsvParserService : ICsvParserService
{
    public Task<List<MeterReadingCsvRow>> ParseCsvAsync(Stream csvStream)
    {
        using var reader = new StreamReader(csvStream);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            TrimOptions = TrimOptions.Trim,
            BadDataFound = null
        };

        using var csv = new CsvReader(reader, config);
        return Task.FromResult(csv.GetRecords<MeterReadingCsvRow>().ToList());
    }
}
