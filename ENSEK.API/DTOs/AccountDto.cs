namespace ENSEK.API.DTOs;

public class AccountDto
{
    public int AccountId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class AccountWithReadingsDto : AccountDto
{
    public List<MeterReadingDto> MeterReadings { get; set; } = new();
}


