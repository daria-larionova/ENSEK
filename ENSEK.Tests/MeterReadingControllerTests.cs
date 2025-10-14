using System.Text;
using ENSEK.API.Controllers;
using ENSEK.API.DTOs;
using ENSEK.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ENSEK.Tests;

public class MeterReadingControllerTests
{
    [Fact]
    public async Task UploadMeterReadings_NoFile_ReturnsBadRequest()
    {
        var mockService = new Mock<IMeterReadingService>();
        var mockLogger = new Mock<ILogger<MeterReadingController>>();
        var controller = new MeterReadingController(mockService.Object, mockLogger.Object);

        var result = await controller.UploadMeterReadings(null!);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task UploadMeterReadings_NonCsvFile_ReturnsBadRequest()
    {
        var mockService = new Mock<IMeterReadingService>();
        var mockLogger = new Mock<ILogger<MeterReadingController>>();
        var controller = new MeterReadingController(mockService.Object, mockLogger.Object);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.txt");
        fileMock.Setup(f => f.Length).Returns(100);

        var result = await controller.UploadMeterReadings(fileMock.Object);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task UploadMeterReadings_ValidFile_ReturnsOk()
    {
        var mockService = new Mock<IMeterReadingService>();
        var mockLogger = new Mock<ILogger<MeterReadingController>>();
        var controller = new MeterReadingController(mockService.Object, mockLogger.Object);

        var uploadResult = new MeterReadingUploadResult
        {
            SuccessfulReadings = 5,
            FailedReadings = 2,
            Errors = new List<string> { "Error 1", "Error 2" }
        };

        mockService
            .Setup(s => s.ProcessMeterReadingsAsync(It.IsAny<Stream>()))
            .ReturnsAsync(uploadResult);

        var content = "AccountId,MeterReadingDateTime,MeterReadValue\n2344,22/04/2019 09:24,01002";
        var fileName = "test.csv";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var fileMock = new Mock<IFormFile>();
        
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

        var result = await controller.UploadMeterReadings(fileMock.Object);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        mockService.Verify(s => s.ProcessMeterReadingsAsync(It.IsAny<Stream>()), Times.Once);
    }

    [Theory]
    [InlineData("test.txt")]
    [InlineData("test.doc")]
    [InlineData("test.xlsx")]
    [InlineData("test.pdf")]
    [InlineData("test")]
    [InlineData("")]
    public async Task UploadMeterReadings_NonCsvFileExtensions_ReturnsBadRequest(string fileName)
    {
        var mockService = new Mock<IMeterReadingService>();
        var mockLogger = new Mock<ILogger<MeterReadingController>>();
        var controller = new MeterReadingController(mockService.Object, mockLogger.Object);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(100);

        var result = await controller.UploadMeterReadings(fileMock.Object);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Theory]
    [InlineData("test.csv")]
    [InlineData("TEST.CSV")]
    [InlineData("test.Csv")]
    [InlineData("data.csv")]
    [InlineData("meter_readings.csv")]
    [InlineData("file.with.dots.csv")]
    public async Task UploadMeterReadings_ValidCsvFileExtensions_ProcessesFile(string fileName)
    {
        var mockService = new Mock<IMeterReadingService>();
        var mockLogger = new Mock<ILogger<MeterReadingController>>();
        var controller = new MeterReadingController(mockService.Object, mockLogger.Object);

        var uploadResult = new MeterReadingUploadResult
        {
            SuccessfulReadings = 1,
            FailedReadings = 0,
            Errors = new List<string>()
        };

        mockService
            .Setup(s => s.ProcessMeterReadingsAsync(It.IsAny<Stream>()))
            .ReturnsAsync(uploadResult);

        var content = "AccountId,MeterReadingDateTime,MeterReadValue\n2344,22/04/2019 09:24,01002";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var fileMock = new Mock<IFormFile>();
        
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

        var result = await controller.UploadMeterReadings(fileMock.Object);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        mockService.Verify(s => s.ProcessMeterReadingsAsync(It.IsAny<Stream>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    public async Task UploadMeterReadings_ZeroFileSize_ReturnsBadRequest(long fileSize)
    {
        var mockService = new Mock<IMeterReadingService>();
        var mockLogger = new Mock<ILogger<MeterReadingController>>();
        var controller = new MeterReadingController(mockService.Object, mockLogger.Object);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.csv");
        fileMock.Setup(f => f.Length).Returns(fileSize);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        var result = await controller.UploadMeterReadings(fileMock.Object);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task UploadMeterReadings_NegativeFileSizes_ThrowsException(long fileSize)
    {
        var mockService = new Mock<IMeterReadingService>();
        var mockLogger = new Mock<ILogger<MeterReadingController>>();
        var controller = new MeterReadingController(mockService.Object, mockLogger.Object);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.csv");
        fileMock.Setup(f => f.Length).Returns(fileSize);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        await Assert.ThrowsAsync<NullReferenceException>(() => controller.UploadMeterReadings(fileMock.Object));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    [InlineData(100000)]
    public async Task UploadMeterReadings_ValidFileSizes_ProcessesFile(long fileSize)
    {
        var mockService = new Mock<IMeterReadingService>();
        var mockLogger = new Mock<ILogger<MeterReadingController>>();
        var controller = new MeterReadingController(mockService.Object, mockLogger.Object);

        var uploadResult = new MeterReadingUploadResult
        {
            SuccessfulReadings = 1,
            FailedReadings = 0,
            Errors = new List<string>()
        };

        mockService
            .Setup(s => s.ProcessMeterReadingsAsync(It.IsAny<Stream>()))
            .ReturnsAsync(uploadResult);

        var content = "AccountId,MeterReadingDateTime,MeterReadValue\n2344,22/04/2019 09:24,01002";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var fileMock = new Mock<IFormFile>();
        
        fileMock.Setup(f => f.FileName).Returns("test.csv");
        fileMock.Setup(f => f.Length).Returns(fileSize);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

        var result = await controller.UploadMeterReadings(fileMock.Object);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        mockService.Verify(s => s.ProcessMeterReadingsAsync(It.IsAny<Stream>()), Times.Once);
    }

    [Fact]
    public async Task UploadMeterReadings_ServiceThrowsException_ThrowsException()
    {
        var mockService = new Mock<IMeterReadingService>();
        var mockLogger = new Mock<ILogger<MeterReadingController>>();
        var controller = new MeterReadingController(mockService.Object, mockLogger.Object);

        mockService
            .Setup(s => s.ProcessMeterReadingsAsync(It.IsAny<Stream>()))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        var content = "AccountId,MeterReadingDateTime,MeterReadValue\n2344,22/04/2019 09:24,01002";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var fileMock = new Mock<IFormFile>();
        
        fileMock.Setup(f => f.FileName).Returns("test.csv");
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.UploadMeterReadings(fileMock.Object));
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(5, 0, 0)]
    [InlineData(0, 3, 3)]
    [InlineData(10, 5, 2)]
    [InlineData(100, 50, 10)]
    public async Task UploadMeterReadings_DifferentUploadResults_ReturnsCorrectResult(int successful, int failed, int errorCount)
    {
        var mockService = new Mock<IMeterReadingService>();
        var mockLogger = new Mock<ILogger<MeterReadingController>>();
        var controller = new MeterReadingController(mockService.Object, mockLogger.Object);

        var errors = new List<string>();
        for (int i = 0; i < errorCount; i++)
        {
            errors.Add($"Error {i + 1}");
        }

        var uploadResult = new MeterReadingUploadResult
        {
            SuccessfulReadings = successful,
            FailedReadings = failed,
            Errors = errors
        };

        mockService
            .Setup(s => s.ProcessMeterReadingsAsync(It.IsAny<Stream>()))
            .ReturnsAsync(uploadResult);

        var content = "AccountId,MeterReadingDateTime,MeterReadValue\n2344,22/04/2019 09:24,01002";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var fileMock = new Mock<IFormFile>();
        
        fileMock.Setup(f => f.FileName).Returns("test.csv");
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

        var result = await controller.UploadMeterReadings(fileMock.Object);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResult = Assert.IsType<MeterReadingUploadResult>(okResult.Value);
        Assert.Equal(successful, returnedResult.SuccessfulReadings);
        Assert.Equal(failed, returnedResult.FailedReadings);
        Assert.Equal(errorCount, returnedResult.Errors.Count);
    }

    [Fact]
    public async Task UploadMeterReadings_FileStreamIsNull_ThrowsException()
    {
        var mockService = new Mock<IMeterReadingService>();
        var mockLogger = new Mock<ILogger<MeterReadingController>>();
        var controller = new MeterReadingController(mockService.Object, mockLogger.Object);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.csv");
        fileMock.Setup(f => f.Length).Returns(100);
        fileMock.Setup(f => f.OpenReadStream()).Returns((Stream)null!);

        await Assert.ThrowsAsync<NullReferenceException>(() => controller.UploadMeterReadings(fileMock.Object));
    }

    [Fact]
    public async Task UploadMeterReadings_EmptyFile_ProcessesCorrectly()
    {
        var mockService = new Mock<IMeterReadingService>();
        var mockLogger = new Mock<ILogger<MeterReadingController>>();
        var controller = new MeterReadingController(mockService.Object, mockLogger.Object);

        var uploadResult = new MeterReadingUploadResult
        {
            SuccessfulReadings = 0,
            FailedReadings = 0,
            Errors = new List<string>()
        };

        mockService
            .Setup(s => s.ProcessMeterReadingsAsync(It.IsAny<Stream>()))
            .ReturnsAsync(uploadResult);

        var content = "AccountId,MeterReadingDateTime,MeterReadValue";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var fileMock = new Mock<IFormFile>();
        
        fileMock.Setup(f => f.FileName).Returns("test.csv");
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

        var result = await controller.UploadMeterReadings(fileMock.Object);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        mockService.Verify(s => s.ProcessMeterReadingsAsync(It.IsAny<Stream>()), Times.Once);
    }
}


