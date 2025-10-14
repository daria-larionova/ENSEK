namespace ENSEK.API.DTOs;

public class MeterReadingUploadResult
{
    public int SuccessfulReadings { get; set; }
    public int FailedReadings { get; set; }
    public List<string> Errors { get; set; } = new();
}


