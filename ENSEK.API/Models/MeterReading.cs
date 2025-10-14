namespace ENSEK.API.Models;

public class MeterReading
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public DateTime MeterReadingDateTime { get; set; }
    public string MeterReadValue { get; set; } = string.Empty;
    public virtual Account? Account { get; set; }
}
