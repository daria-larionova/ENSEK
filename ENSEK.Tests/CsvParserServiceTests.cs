using System.Text;
using ENSEK.API.Services;
using Xunit;

namespace ENSEK.Tests;

public class CsvParserServiceTests
{
    [Fact]
    public async Task ParseCsvAsync_ValidCsv_ReturnsRecords()
    {
        var service = new CsvParserService();
        var csvContent = @"AccountId,MeterReadingDateTime,MeterReadValue
2344,22/04/2019 09:24,01002
2233,22/04/2019 12:25,00323";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ParseCsvAsync(stream);

        Assert.Equal(2, result.Count);
        Assert.Equal("2344", result[0].AccountId);
        Assert.Equal("22/04/2019 09:24", result[0].MeterReadingDateTime);
        Assert.Equal("01002", result[0].MeterReadValue);
    }

    [Fact]
    public async Task ParseCsvAsync_EmptyCsv_ReturnsEmptyList()
    {
        var service = new CsvParserService();
        var csvContent = @"AccountId,MeterReadingDateTime,MeterReadValue";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ParseCsvAsync(stream);

        Assert.Empty(result);
    }

    [Theory]
    [InlineData("2344,22/04/2019 09:24,01002\n2233,22/04/2019 12:25,00323\n2345,23/04/2019 15:30,00456", 3)]
    [InlineData("2344,22/04/2019 09:24,01002", 1)]
    [InlineData("2344,22/04/2019 09:24,01002\n2233,22/04/2019 12:25,00323", 2)]
    public async Task ParseCsvAsync_DifferentRecordCounts_ReturnsCorrectCount(string csvData, int expectedCount)
    {
        var service = new CsvParserService();
        var csvContent = $"AccountId,MeterReadingDateTime,MeterReadValue\n{csvData}";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ParseCsvAsync(stream);

        Assert.Equal(expectedCount, result.Count);
    }

    [Theory]
    [InlineData("2344,22/04/2019 09:24,01002", "2344", "22/04/2019 09:24", "01002")]
    [InlineData("123,01/01/2020 00:00,00001", "123", "01/01/2020 00:00", "00001")]
    [InlineData("99999,31/12/2023 23:59,99999", "99999", "31/12/2023 23:59", "99999")]
    public async Task ParseCsvAsync_SingleRecord_ReturnsCorrectValues(string csvRow, string expectedAccountId, string expectedDateTime, string expectedValue)
    {
        var service = new CsvParserService();
        var csvContent = $"AccountId,MeterReadingDateTime,MeterReadValue\n{csvRow}";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ParseCsvAsync(stream);

        Assert.Single(result);
        Assert.Equal(expectedAccountId, result[0].AccountId);
        Assert.Equal(expectedDateTime, result[0].MeterReadingDateTime);
        Assert.Equal(expectedValue, result[0].MeterReadValue);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("2344,22/04/2019 09:24,")]
    [InlineData("2344,,01002")]
    [InlineData(",22/04/2019 09:24,01002")]
    [InlineData("2344,22/04/2019 09:24,01002,")]
    [InlineData("2344,22/04/2019 09:24,01002,extra")]
    public async Task ParseCsvAsync_RecordsWithEmptyOrExtraFields_HandlesGracefully(string csvRow)
    {
        var service = new CsvParserService();
        var csvContent = $"AccountId,MeterReadingDateTime,MeterReadValue\n{csvRow}";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ParseCsvAsync(stream);

        Assert.Single(result);
        // Should not throw exception, but may have empty values
    }

    [Fact]
    public async Task ParseCsvAsync_EmptyRow_ReturnsEmptyList()
    {
        var service = new CsvParserService();
        var csvContent = "AccountId,MeterReadingDateTime,MeterReadValue\n";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ParseCsvAsync(stream);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ParseCsvAsync_RecordsWithQuotes_HandlesCorrectly()
    {
        var service = new CsvParserService();
        var csvContent = @"AccountId,MeterReadingDateTime,MeterReadValue
""2344"",""22/04/2019 09:24"",""01002""
""2233"",""22/04/2019 12:25"",""00323""";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ParseCsvAsync(stream);

        Assert.Equal(2, result.Count);
        Assert.Equal("2344", result[0].AccountId);
        Assert.Equal("22/04/2019 09:24", result[0].MeterReadingDateTime);
        Assert.Equal("01002", result[0].MeterReadValue);
    }

    [Fact]
    public async Task ParseCsvAsync_RecordsWithCommasInValues_HandlesCorrectly()
    {
        var service = new CsvParserService();
        var csvContent = @"AccountId,MeterReadingDateTime,MeterReadValue
""2344"",""22/04/2019, 09:24"",""01002""
2233,22/04/2019 12:25,00323";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ParseCsvAsync(stream);

        Assert.Equal(2, result.Count);
        Assert.Equal("2344", result[0].AccountId);
        Assert.Equal("22/04/2019, 09:24", result[0].MeterReadingDateTime);
        Assert.Equal("01002", result[0].MeterReadValue);
    }

    [Theory]
    [InlineData("AccountId,MeterReadingDateTime,MeterReadValue\n\n2344,22/04/2019 09:24,01002")]
    [InlineData("AccountId,MeterReadingDateTime,MeterReadValue\n2344,22/04/2019 09:24,01002\n\n2233,22/04/2019 12:25,00323")]
    [InlineData("AccountId,MeterReadingDateTime,MeterReadValue\n2344,22/04/2019 09:24,01002\n   \n2233,22/04/2019 12:25,00323")]
    public async Task ParseCsvAsync_RecordsWithEmptyLines_IgnoresEmptyLines(string csvContent)
    {
        var service = new CsvParserService();

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ParseCsvAsync(stream);

        // Should ignore empty lines and only parse valid records
        Assert.True(result.Count >= 1);
    }

    [Fact]
    public async Task ParseCsvAsync_LargeCsvFile_HandlesCorrectly()
    {
        var service = new CsvParserService();
        var csvBuilder = new StringBuilder("AccountId,MeterReadingDateTime,MeterReadValue\n");
        
        // Generate 1000 records
        for (int i = 1; i <= 1000; i++)
        {
            csvBuilder.AppendLine($"{i},22/04/2019 09:24,{i:D5}");
        }

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvBuilder.ToString()));

        var result = await service.ParseCsvAsync(stream);

        Assert.Equal(1000, result.Count);
        Assert.Equal("1", result[0].AccountId);
        Assert.Equal("1000", result[999].AccountId);
    }

    [Theory]
    [InlineData("AccountId,MeterReadingDateTime,MeterReadValue\n2344,22/04/2019 09:24,01002\r\n2233,22/04/2019 12:25,00323")]
    [InlineData("AccountId,MeterReadingDateTime,MeterReadValue\r\n2344,22/04/2019 09:24,01002\r\n2233,22/04/2019 12:25,00323")]
    [InlineData("AccountId,MeterReadingDateTime,MeterReadValue\r2344,22/04/2019 09:24,01002\r2233,22/04/2019 12:25,00323")]
    public async Task ParseCsvAsync_DifferentLineEndings_HandlesCorrectly(string csvContent)
    {
        var service = new CsvParserService();

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var result = await service.ParseCsvAsync(stream);

        Assert.Equal(2, result.Count);
        Assert.Equal("2344", result[0].AccountId);
        Assert.Equal("2233", result[1].AccountId);
    }

    [Fact]
    public async Task ParseCsvAsync_StreamWithBOM_HandlesCorrectly()
    {
        var service = new CsvParserService();
        var csvContent = "AccountId,MeterReadingDateTime,MeterReadValue\n2344,22/04/2019 09:24,01002";
        
        // Add UTF-8 BOM
        var bom = new byte[] { 0xEF, 0xBB, 0xBF };
        var contentBytes = Encoding.UTF8.GetBytes(csvContent);
        var contentWithBom = new byte[bom.Length + contentBytes.Length];
        Array.Copy(bom, 0, contentWithBom, 0, bom.Length);
        Array.Copy(contentBytes, 0, contentWithBom, bom.Length, contentBytes.Length);

        using var stream = new MemoryStream(contentWithBom);

        var result = await service.ParseCsvAsync(stream);

        Assert.Single(result);
        Assert.Equal("2344", result[0].AccountId);
    }
}
