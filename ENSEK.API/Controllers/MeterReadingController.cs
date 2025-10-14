using Asp.Versioning;
using ENSEK.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ENSEK.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class MeterReadingController : ControllerBase
{
    private readonly IMeterReadingService _meterReadingService;
    private readonly ILogger<MeterReadingController> _logger;

    public MeterReadingController(IMeterReadingService meterReadingService, ILogger<MeterReadingController> logger)
    {
        _meterReadingService = meterReadingService;
        _logger = logger;
    }

    [HttpPost("meter-reading-uploads")]
    public async Task<IActionResult> UploadMeterReadings(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only CSV files are accepted");

        using var stream = file.OpenReadStream();
        var result = await _meterReadingService.ProcessMeterReadingsAsync(stream);

        _logger.LogInformation("Meter readings upload: {SuccessCount} successful, {FailureCount} failed",
            result.SuccessfulReadings, result.FailedReadings);

        return Ok(result);
    }

    [HttpDelete("clear-readings")]
    public async Task<IActionResult> ClearAllReadings()
    {
        await _meterReadingService.ClearAllReadingsAsync();
        return Ok(new { message = "All meter readings cleared successfully" });
    }
}
