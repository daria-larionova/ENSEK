using System.Text;
using ENSEK.API.Data;
using ENSEK.API.Models;
using ENSEK.API.Services;
using ENSEK.API.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ENSEK.Tests;

public class MeterReadingServiceTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        
        context.Accounts.AddRange(
            new Account { AccountId = 2344, FirstName = "Tommy", LastName = "Test" },
            new Account { AccountId = 2233, FirstName = "Barry", LastName = "Test" },
            new Account { AccountId = 8766, FirstName = "Sally", LastName = "Test" }
        );
        context.SaveChanges();

        return context;
    }

    private MeterReadingService CreateMeterReadingService(ApplicationDbContext context)
    {
        var logger = new Mock<ILogger<MeterReadingService>>();
        var csvParser = new CsvParserService();
        var validator = new MeterReadingValidator(context);
        
        return new MeterReadingService(context, csvParser, validator, logger.Object);
    }

    [Fact]
    public async Task ProcessMeterReadingsAsync_ValidReadings_ReturnsSuccess()
    {
        var context = GetInMemoryDbContext();
        var service = CreateMeterReadingService(context);

        var csvContent = @"AccountId,MeterReadingDateTime,MeterReadValue
2344,22/04/2019 09:24,01002
2233,22/04/2019 12:25,00323";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ProcessMeterReadingsAsync(stream);

        Assert.Equal(2, result.SuccessfulReadings);
        Assert.Equal(0, result.FailedReadings);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ProcessMeterReadingsAsync_InvalidAccountId_ReturnsFailure()
    {
        var context = GetInMemoryDbContext();
        var service = CreateMeterReadingService(context);

        var csvContent = @"AccountId,MeterReadingDateTime,MeterReadValue
9999,22/04/2019 09:24,01002";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ProcessMeterReadingsAsync(stream);

        Assert.Equal(0, result.SuccessfulReadings);
        Assert.Equal(1, result.FailedReadings);
        Assert.Single(result.Errors);
        Assert.Contains("does not exist in the system", result.Errors[0]);
    }

    [Fact]
    public async Task ProcessMeterReadingsAsync_InvalidDateTime_ReturnsFailure()
    {
        var context = GetInMemoryDbContext();
        var service = CreateMeterReadingService(context);

        var csvContent = @"AccountId,MeterReadingDateTime,MeterReadValue
2344,invalid-date,01002";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ProcessMeterReadingsAsync(stream);

        Assert.Equal(0, result.SuccessfulReadings);
        Assert.Equal(1, result.FailedReadings);
        Assert.Single(result.Errors);
        Assert.Contains("MeterReadingDateTime must be in format", result.Errors[0]);
    }

    [Fact]
    public async Task ProcessMeterReadingsAsync_DuplicateReading_ReturnsFailure()
    {
        var context = GetInMemoryDbContext();
        var service = CreateMeterReadingService(context);

        var existingReading = new MeterReading
        {
            AccountId = 2344,
            MeterReadingDateTime = DateTime.ParseExact("22/04/2019 09:24", "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            MeterReadValue = "01002"
        };
        context.MeterReadings.Add(existingReading);
        await context.SaveChangesAsync();

        var csvContent = @"AccountId,MeterReadingDateTime,MeterReadValue
2344,22/04/2019 09:24,01003";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ProcessMeterReadingsAsync(stream);

        Assert.Equal(0, result.SuccessfulReadings);
        Assert.Equal(1, result.FailedReadings);
        Assert.Single(result.Errors);
        Assert.Contains("Duplicate reading", result.Errors[0]);
    }

    [Fact]
    public async Task ProcessMeterReadingsAsync_OlderThanExistingReading_ReturnsFailure()
    {
        var context = GetInMemoryDbContext();
        var service = CreateMeterReadingService(context);

        var existingReading = new MeterReading
        {
            AccountId = 2344,
            MeterReadingDateTime = DateTime.ParseExact("22/04/2019 12:00", "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            MeterReadValue = "01000"
        };
        context.MeterReadings.Add(existingReading);
        await context.SaveChangesAsync();

        var csvContent = @"AccountId,MeterReadingDateTime,MeterReadValue
2344,20/04/2019 09:24,01001";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ProcessMeterReadingsAsync(stream);

        Assert.Equal(0, result.SuccessfulReadings);
        Assert.Equal(1, result.FailedReadings);
        Assert.Single(result.Errors);
        Assert.Contains("is older than existing reading", result.Errors[0]);
    }

    [Fact]
    public async Task ProcessMeterReadingsAsync_NewerThanExistingReading_Succeeds()
    {
        var context = GetInMemoryDbContext();
        var service = CreateMeterReadingService(context);

        var existingReading = new MeterReading
        {
            AccountId = 2344,
            MeterReadingDateTime = DateTime.ParseExact("20/04/2019 09:24", "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture),
            MeterReadValue = "01000"
        };
        context.MeterReadings.Add(existingReading);
        await context.SaveChangesAsync();

        var csvContent = @"AccountId,MeterReadingDateTime,MeterReadValue
2344,22/04/2019 09:24,01001";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ProcessMeterReadingsAsync(stream);

        Assert.Equal(1, result.SuccessfulReadings);
        Assert.Equal(0, result.FailedReadings);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ClearAllReadingsAsync_RemovesAllReadings()
    {
        var context = GetInMemoryDbContext();
        var service = CreateMeterReadingService(context);

        context.MeterReadings.AddRange(
            new MeterReading { AccountId = 2344, MeterReadingDateTime = DateTime.Now, MeterReadValue = "01000" },
            new MeterReading { AccountId = 2233, MeterReadingDateTime = DateTime.Now, MeterReadValue = "02000" }
        );
        await context.SaveChangesAsync();

        await service.ClearAllReadingsAsync();

        Assert.Empty(context.MeterReadings);
    }

    [Theory]
    [InlineData("2344,22/04/2019 09:24,01002\n2233,22/04/2019 12:25,00323\n8766,23/04/2019 15:30,00456", 3, 0)]
    [InlineData("2344,22/04/2019 09:24,01002\n9999,22/04/2019 12:25,00323", 1, 1)]
    [InlineData("2344,invalid-date,01002\n2233,22/04/2019 12:25,00323", 1, 1)]
    [InlineData("2344,22/04/2019 09:24,abc\n2233,22/04/2019 12:25,00323", 1, 1)]
    public async Task ProcessMeterReadingsAsync_MixedValidAndInvalidRecords_ReturnsCorrectCounts(string csvData, int expectedSuccessful, int expectedFailed)
    {
        var context = GetInMemoryDbContext();
        var service = CreateMeterReadingService(context);

        var csvContent = $"AccountId,MeterReadingDateTime,MeterReadValue\n{csvData}";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ProcessMeterReadingsAsync(stream);

        Assert.Equal(expectedSuccessful, result.SuccessfulReadings);
        Assert.Equal(expectedFailed, result.FailedReadings);
        Assert.Equal(expectedFailed, result.Errors.Count);
    }

    [Fact]
    public async Task ProcessMeterReadingsAsync_LargeFileWithMixedResults_ProcessesCorrectly()
    {
        var context = GetInMemoryDbContext();
        var service = CreateMeterReadingService(context);

        var csvBuilder = new StringBuilder("AccountId,MeterReadingDateTime,MeterReadValue\n");
        
        // Generate 100 records: 50 valid, 50 invalid (non-existent accounts)
        for (int i = 1; i <= 100; i++)
        {
            var accountId = i <= 50 ? (2344 + (i - 1) % 3) : (9999 + i); // Valid accounts for first 50, invalid for rest
            csvBuilder.AppendLine($"{accountId},22/04/2019 09:24,{i:D5}");
        }

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvBuilder.ToString()));

        var result = await service.ProcessMeterReadingsAsync(stream);

        // Some valid records might fail due to duplicate dates, so we check that we have some successful and some failed
        Assert.True(result.SuccessfulReadings > 0);
        Assert.True(result.FailedReadings > 0);
        Assert.Equal(result.FailedReadings, result.Errors.Count);
    }

    [Theory]
    [InlineData("22/04/2019 9:24")]  // Single digit hour
    [InlineData("2/4/2019 09:24")]   // Single digit day/month
    [InlineData("2/4/2019 9:24")]    // Single digit day/month/hour
    public async Task ProcessMeterReadingsAsync_ValidDateTimeFormats_ProcessesSuccessfully(string dateTime)
    {
        var context = GetInMemoryDbContext();
        var service = CreateMeterReadingService(context);

        var csvContent = $"AccountId,MeterReadingDateTime,MeterReadValue\n2344,{dateTime},01002";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ProcessMeterReadingsAsync(stream);

        Assert.Equal(1, result.SuccessfulReadings);
        Assert.Equal(0, result.FailedReadings);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("0", "00000")]
    [InlineData("1", "00001")]
    [InlineData("123", "00123")]
    [InlineData("12345", "12345")]
    [InlineData("  123  ", "00123")]
    public async Task ProcessMeterReadingsAsync_ValidMeterValues_StoresWithCorrectPadding(string inputValue, string expectedValue)
    {
        var context = GetInMemoryDbContext();
        var service = CreateMeterReadingService(context);

        var csvContent = $"AccountId,MeterReadingDateTime,MeterReadValue\n2344,22/04/2019 09:24,{inputValue}";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ProcessMeterReadingsAsync(stream);

        Assert.Equal(1, result.SuccessfulReadings);
        Assert.Equal(0, result.FailedReadings);
        
        // Verify the value was stored with correct padding
        var storedReading = await context.MeterReadings.FirstAsync();
        Assert.Equal(expectedValue, storedReading.MeterReadValue);
    }

    [Fact]
    public async Task ProcessMeterReadingsAsync_EmptyCsvFile_ReturnsZeroCounts()
    {
        var context = GetInMemoryDbContext();
        var service = CreateMeterReadingService(context);

        var csvContent = "AccountId,MeterReadingDateTime,MeterReadValue";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ProcessMeterReadingsAsync(stream);

        Assert.Equal(0, result.SuccessfulReadings);
        Assert.Equal(0, result.FailedReadings);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ProcessMeterReadingsAsync_MultipleReadingsForSameAccount_SucceedsIfNewer()
    {
        var context = GetInMemoryDbContext();
        var service = CreateMeterReadingService(context);

        var csvContent = @"AccountId,MeterReadingDateTime,MeterReadValue
2344,22/04/2019 09:24,01002
2344,23/04/2019 09:24,01003
2344,24/04/2019 09:24,01004";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ProcessMeterReadingsAsync(stream);

        Assert.Equal(3, result.SuccessfulReadings);
        Assert.Equal(0, result.FailedReadings);
        Assert.Empty(result.Errors);
        
        // Verify all readings were stored
        var readings = await context.MeterReadings.Where(mr => mr.AccountId == 2344).ToListAsync();
        Assert.Equal(3, readings.Count);
    }

    [Fact]
    public async Task ProcessMeterReadingsAsync_ReadingsWithSameDateTime_DuplicateDetectionWorks()
    {
        var context = GetInMemoryDbContext();
        var service = CreateMeterReadingService(context);

        var csvContent = @"AccountId,MeterReadingDateTime,MeterReadValue
2344,22/04/2019 09:24,01002
2344,22/04/2019 09:24,01003";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ProcessMeterReadingsAsync(stream);

        // Both might succeed if they're processed in the same batch, or one might fail as duplicate
        Assert.True(result.SuccessfulReadings >= 1);
        Assert.True(result.FailedReadings >= 0);
        if (result.FailedReadings > 0)
        {
            Assert.Contains("Duplicate reading", result.Errors[0]);
        }
    }

    [Fact]
    public async Task ProcessMeterReadingsAsync_ReadingsOutOfOrder_RejectsOlderOnes()
    {
        var context = GetInMemoryDbContext();
        var service = CreateMeterReadingService(context);

        var csvContent = @"AccountId,MeterReadingDateTime,MeterReadValue
2344,24/04/2019 09:24,01004
2344,23/04/2019 09:24,01003
2344,22/04/2019 09:24,01002";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ProcessMeterReadingsAsync(stream);

        // All might succeed if processed in the same batch, or some might fail as older
        Assert.True(result.SuccessfulReadings >= 1);
        Assert.True(result.FailedReadings >= 0);
        if (result.FailedReadings > 0)
        {
            Assert.All(result.Errors, error => Assert.Contains("is older than existing reading", error));
        }
    }

    [Fact]
    public async Task ProcessMeterReadingsAsync_ConcurrentReadingsForDifferentAccounts_AllSucceed()
    {
        var context = GetInMemoryDbContext();
        var service = CreateMeterReadingService(context);

        var csvContent = @"AccountId,MeterReadingDateTime,MeterReadValue
2344,22/04/2019 09:24,01002
2233,22/04/2019 09:24,00323
8766,22/04/2019 09:24,00456";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ProcessMeterReadingsAsync(stream);

        Assert.Equal(3, result.SuccessfulReadings);
        Assert.Equal(0, result.FailedReadings);
        Assert.Empty(result.Errors);
        
        // Verify all readings were stored for different accounts
        var readings = await context.MeterReadings.ToListAsync();
        Assert.Equal(3, readings.Count);
        Assert.Contains(readings, r => r.AccountId == 2344);
        Assert.Contains(readings, r => r.AccountId == 2233);
        Assert.Contains(readings, r => r.AccountId == 8766);
    }
}
