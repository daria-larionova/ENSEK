namespace ENSEK.API.DTOs;

public class MeterReadingDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public DateTime MeterReadingDateTime { get; set; }
    public string MeterReadValue { get; set; } = string.Empty;
}


