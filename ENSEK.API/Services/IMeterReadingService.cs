using ENSEK.API.DTOs;

namespace ENSEK.API.Services;

public interface IMeterReadingService
{
    Task<MeterReadingUploadResult> ProcessMeterReadingsAsync(Stream csvStream);
    Task ClearAllReadingsAsync();
}
