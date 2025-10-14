using ENSEK.API.DTOs;

namespace ENSEK.API.Services;

public interface ICsvParserService
{
    Task<List<MeterReadingCsvRow>> ParseCsvAsync(Stream csvStream);
}
