using System.Globalization;
using ENSEK.API.Data;
using ENSEK.API.DTOs;
using ENSEK.API.Models;
using ENSEK.API.Validators;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ENSEK.Tests;

public class MeterReadingValidatorTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        
        context.Accounts.AddRange(
            new Account { AccountId = 2344, FirstName = "Tommy", LastName = "Test" },
            new Account { AccountId = 2233, FirstName = "Barry", LastName = "Test" }
        );
        context.SaveChanges();

        return context;
    }

    [Fact]
    public async Task ValidateAsync_ValidRecord_ReturnsSuccess()
    {
        var context = GetInMemoryDbContext();
        var validator = new MeterReadingValidator(context);

        var record = new MeterReadingCsvRow
        {
            AccountId = "2344",
            MeterReadingDateTime = "22/04/2019 09:24",
            MeterReadValue = "01002"
        };

        var result = await validator.ValidateAsync(record, 2);

        Assert.True(result.IsValid);
        Assert.Equal(2344, result.AccountId);
        Assert.Equal(DateTime.ParseExact("22/04/2019 09:24", "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture), result.DateTime);
        Assert.Equal("01002", result.MeterReadValue);
    }

    [Fact]
    public async Task ValidateAsync_InvalidAccountId_ReturnsFailure()
    {
        var context = GetInMemoryDbContext();
        var validator = new MeterReadingValidator(context);

        var record = new MeterReadingCsvRow
        {
            AccountId = "9999",
            MeterReadingDateTime = "22/04/2019 09:24",
            MeterReadValue = "01002"
        };

        var result = await validator.ValidateAsync(record, 2);

        Assert.False(result.IsValid);
        Assert.Contains("does not exist in the system", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateAsync_InvalidDateTime_ReturnsFailure()
    {
        var context = GetInMemoryDbContext();
        var validator = new MeterReadingValidator(context);

        var record = new MeterReadingCsvRow
        {
            AccountId = "2344",
            MeterReadingDateTime = "invalid-date",
            MeterReadValue = "01002"
        };

        var result = await validator.ValidateAsync(record, 2);

        Assert.False(result.IsValid);
        Assert.Contains("MeterReadingDateTime must be in format", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateAsync_DuplicateReading_ReturnsFailure()
    {
        var context = GetInMemoryDbContext();
        var validator = new MeterReadingValidator(context);
        
        var existingReading = new MeterReading
        {
            AccountId = 2344,
            MeterReadingDateTime = DateTime.ParseExact("22/04/2019 09:24", "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
            MeterReadValue = "01002"
        };
        context.MeterReadings.Add(existingReading);
        await context.SaveChangesAsync();
        

        var record = new MeterReadingCsvRow
        {
            AccountId = "2344",
            MeterReadingDateTime = "22/04/2019 09:24",
            MeterReadValue = "01003"
        };

        var result = await validator.ValidateAsync(record, 2);

        Assert.False(result.IsValid);
        Assert.Contains("Duplicate reading", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateAsync_OlderThanExistingReading_ReturnsFailure()
    {
        var context = GetInMemoryDbContext();
        var validator = new MeterReadingValidator(context);
        
        var existingReading = new MeterReading
        {
            AccountId = 2344,
            MeterReadingDateTime = DateTime.ParseExact("22/04/2019 12:00", "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
            MeterReadValue = "01000"
        };
        context.MeterReadings.Add(existingReading);
        await context.SaveChangesAsync();
        

        var record = new MeterReadingCsvRow
        {
            AccountId = "2344",
            MeterReadingDateTime = "20/04/2019 09:24",
            MeterReadValue = "01001"
        };

        var result = await validator.ValidateAsync(record, 2);

        Assert.False(result.IsValid);
        Assert.Contains("is older than existing reading", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateAsync_InvalidMeterValue_ReturnsFailure()
    {
        var context = GetInMemoryDbContext();
        var validator = new MeterReadingValidator(context);

        var record = new MeterReadingCsvRow
        {
            AccountId = "2344",
            MeterReadingDateTime = "22/04/2019 09:24",
            MeterReadValue = "invalid"
        };

        var result = await validator.ValidateAsync(record, 2);

        Assert.False(result.IsValid);
        Assert.Contains("must be numeric and up to 5 digits", result.ErrorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ValidateAsync_EmptyOrNullMeterValue_ReturnsFailure(string? meterValue)
    {
        var context = GetInMemoryDbContext();
        var validator = new MeterReadingValidator(context);

        var record = new MeterReadingCsvRow
        {
            AccountId = "2344",
            MeterReadingDateTime = "22/04/2019 09:24",
            MeterReadValue = meterValue ?? string.Empty
        };

        var result = await validator.ValidateAsync(record, 2);

        Assert.False(result.IsValid);
        Assert.Contains("MeterReadValue is required", result.ErrorMessage);
    }

    [Theory]
    [InlineData("-123")]
    [InlineData("-1")]
    [InlineData("123456")]
    [InlineData("1234567")]
    [InlineData("abc")]
    [InlineData("12.34")]
    [InlineData("12,34")]
    [InlineData("12a34")]
    public async Task ValidateAsync_InvalidMeterValueFormats_ReturnsFailure(string meterValue)
    {
        var context = GetInMemoryDbContext();
        var validator = new MeterReadingValidator(context);

        var record = new MeterReadingCsvRow
        {
            AccountId = "2344",
            MeterReadingDateTime = "22/04/2019 09:24",
            MeterReadValue = meterValue
        };

        var result = await validator.ValidateAsync(record, 2);

        Assert.False(result.IsValid);
        Assert.Contains("must be numeric and up to 5 digits", result.ErrorMessage);
    }

    [Theory]
    [InlineData("0", "00000")]
    [InlineData("1", "00001")]
    [InlineData("123", "00123")]
    [InlineData("12345", "12345")]
    [InlineData("  123  ", "00123")]
    public async Task ValidateAsync_ValidMeterValues_ReturnsSuccessWithPaddedValue(string inputValue, string expectedValue)
    {
        var context = GetInMemoryDbContext();
        var validator = new MeterReadingValidator(context);

        var record = new MeterReadingCsvRow
        {
            AccountId = "2344",
            MeterReadingDateTime = "22/04/2019 09:24",
            MeterReadValue = inputValue
        };

        var result = await validator.ValidateAsync(record, 2);

        Assert.True(result.IsValid);
        Assert.Equal(expectedValue, result.MeterReadValue);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("12.34")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ValidateAsync_InvalidAccountIdFormats_ReturnsFailure(string? accountId)
    {
        var context = GetInMemoryDbContext();
        var validator = new MeterReadingValidator(context);

        var record = new MeterReadingCsvRow
        {
            AccountId = accountId ?? string.Empty,
            MeterReadingDateTime = "22/04/2019 09:24",
            MeterReadValue = "12345"
        };

        var result = await validator.ValidateAsync(record, 2);

        Assert.False(result.IsValid);
        Assert.Contains("AccountId must be a valid integer", result.ErrorMessage);
    }

    [Theory]
    [InlineData("22/04/2019 9:24")]  // Single digit hour
    [InlineData("2/4/2019 09:24")]   // Single digit day/month
    [InlineData("2/4/2019 9:24")]    // Single digit day/month/hour
    [InlineData("22/04/2019 09:24")] // Standard format
    public async Task ValidateAsync_ValidDateTimeFormats_ReturnsSuccess(string dateTime)
    {
        var context = GetInMemoryDbContext();
        var validator = new MeterReadingValidator(context);

        var record = new MeterReadingCsvRow
        {
            AccountId = "2344",
            MeterReadingDateTime = dateTime,
            MeterReadValue = "12345"
        };

        var result = await validator.ValidateAsync(record, 2);

        Assert.True(result.IsValid);
        Assert.NotNull(result.DateTime);
    }

    [Theory]
    [InlineData("2019-04-22 09:24")]  // ISO format
    [InlineData("04/22/2019 09:24")]  // US format
    [InlineData("22-04-2019 09:24")]  // Wrong separator
    [InlineData("22/04/19 09:24")]    // 2-digit year
    [InlineData("32/04/2019 09:24")]  // Invalid day
    [InlineData("22/13/2019 09:24")]  // Invalid month
    [InlineData("22/04/2019 25:24")]  // Invalid hour
    [InlineData("22/04/2019 09:60")]  // Invalid minute
    [InlineData("not-a-date")]
    [InlineData("")]
    [InlineData(null)]
    public async Task ValidateAsync_InvalidDateTimeFormats_ReturnsFailure(string? dateTime)
    {
        var context = GetInMemoryDbContext();
        var validator = new MeterReadingValidator(context);

        var record = new MeterReadingCsvRow
        {
            AccountId = "2344",
            MeterReadingDateTime = dateTime ?? string.Empty,
            MeterReadValue = "12345"
        };

        var result = await validator.ValidateAsync(record, 2);

        Assert.False(result.IsValid);
        Assert.Contains("MeterReadingDateTime must be in format", result.ErrorMessage);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    [InlineData(999)]
    public async Task ValidateAsync_DifferentLineNumbers_IncludeCorrectLineNumber(int lineNumber)
    {
        var context = GetInMemoryDbContext();
        var validator = new MeterReadingValidator(context);

        var record = new MeterReadingCsvRow
        {
            AccountId = "9999", // Invalid account
            MeterReadingDateTime = "22/04/2019 09:24",
            MeterReadValue = "12345"
        };

        var result = await validator.ValidateAsync(record, lineNumber);

        Assert.False(result.IsValid);
        Assert.Contains($"Line {lineNumber}:", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateAsync_ValidRecordWithDifferentAccount_ReturnsSuccess()
    {
        var context = GetInMemoryDbContext();
        var validator = new MeterReadingValidator(context);

        var record = new MeterReadingCsvRow
        {
            AccountId = "2233", // Different valid account
            MeterReadingDateTime = "22/04/2019 09:24",
            MeterReadValue = "54321"
        };

        var result = await validator.ValidateAsync(record, 2);

        Assert.True(result.IsValid);
        Assert.Equal(2233, result.AccountId);
        Assert.Equal("54321", result.MeterReadValue);
    }

    [Fact]
    public async Task ValidateAsync_NewerReadingThanExisting_ReturnsSuccess()
    {
        var context = GetInMemoryDbContext();
        var validator = new MeterReadingValidator(context);
        
        // Add existing reading
        var existingReading = new MeterReading
        {
            AccountId = 2344,
            MeterReadingDateTime = DateTime.ParseExact("22/04/2019 09:24", "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
            MeterReadValue = "01000"
        };
        context.MeterReadings.Add(existingReading);
        await context.SaveChangesAsync();

        // Try to add newer reading
        var record = new MeterReadingCsvRow
        {
            AccountId = "2344",
            MeterReadingDateTime = "23/04/2019 09:24", // Next day
            MeterReadValue = "01001"
        };

        var result = await validator.ValidateAsync(record, 2);

        Assert.True(result.IsValid);
        Assert.Equal(2344, result.AccountId);
        Assert.Equal("01001", result.MeterReadValue);
    }
}
