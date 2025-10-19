using meter_reading.Domain.Entities;
using meter_reading.Domain.Interfaces;
using meter_reading.Presentation.Controllers;
using meter_reading.Presentation.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace meter_reading.Tests.Controllers
{
    [TestFixture]
    public class MeterReadingControllerTests
    {
        private Mock<IService<MeterReading>> _mockMeterReadingService;
        private MeterReadingController _controller;
        private DefaultHttpContext _httpContext;

        [SetUp]
        public void Setup()
        {
            _mockMeterReadingService = new Mock<IService<MeterReading>>();
            _controller = new MeterReadingController(_mockMeterReadingService.Object);
            _httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };
        }

        #region Constructor Tests

        [Test]
        public void Constructor_WhenServiceIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IService<MeterReading> nullService = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new MeterReadingController(nullService));

            Assert.That(exception.ParamName, Is.EqualTo("meterReadinService"));
        }

        [Test]
        public void Constructor_WhenServiceIsValid_CreatesInstance()
        {
            // Arrange
            var mockService = new Mock<IService<MeterReading>>();

            // Act
            var controller = new MeterReadingController(mockService.Object);

            // Assert
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller, Is.InstanceOf<MeterReadingController>());
        }

        #endregion

        #region Uploads Tests

        [Test]
        public async Task Uploads_WhenAllRecordsSuccessfullySaved_ReturnsCorrectResponse()
        {
            // Arrange
            var meterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    MeterReadingId = 1,
                    AccountId = 100,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 12345,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 2,
                    AccountId = 101,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 67890,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 3,
                    AccountId = 102,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 11111,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                }
            };

            // Simulate the filter storing the CSV items in HttpContext
            _httpContext.Items["csv"] = meterReadings;

            _mockMeterReadingService
                .Setup(s => s.Upload(It.IsAny<List<MeterReading>>()))
                .ReturnsAsync(3);

            // Act
            var result = await _controller.Uploads();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.InstanceOf<UploadResponse>());

            var response = okResult.Value as UploadResponse;
            Assert.That(response.Success, Is.EqualTo(3));
            Assert.That(response.Failed, Is.EqualTo(0));

            _mockMeterReadingService.Verify(s => s.Upload(It.IsAny<List<MeterReading>>()), Times.Once);
        }

        [Test]
        public async Task Uploads_WhenPartialSuccess_ReturnsCorrectSuccessAndFailedCounts()
        {
            // Arrange
            var meterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    MeterReadingId = 1,
                    AccountId = 100,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 12345,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 2,
                    AccountId = 101,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 67890,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 3,
                    AccountId = 102,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 11111,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 4,
                    AccountId = 103,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 22222,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 5,
                    AccountId = 104,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 33333,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                }
            };

            _httpContext.Items["csv"] = meterReadings;

            // Simulate that only 3 out of 5 records were successfully saved
            _mockMeterReadingService
                .Setup(s => s.Upload(It.IsAny<List<MeterReading>>()))
                .ReturnsAsync(3);

            // Act
            var result = await _controller.Uploads();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var response = okResult.Value as UploadResponse;

            Assert.That(response.Success, Is.EqualTo(3));
            Assert.That(response.Failed, Is.EqualTo(2));

            _mockMeterReadingService.Verify(s => s.Upload(It.IsAny<List<MeterReading>>()), Times.Once);
        }

        [Test]
        public async Task Uploads_WhenNoRecordsSuccessfullySaved_ReturnsAllFailed()
        {
            // Arrange
            var meterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    MeterReadingId = 1,
                    AccountId = 100,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 12345,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 2,
                    AccountId = 101,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 67890,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                }
            };

            _httpContext.Items["csv"] = meterReadings;

            _mockMeterReadingService
                .Setup(s => s.Upload(It.IsAny<List<MeterReading>>()))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.Uploads();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var response = okResult.Value as UploadResponse;

            Assert.That(response.Success, Is.EqualTo(0));
            Assert.That(response.Failed, Is.EqualTo(2));

            _mockMeterReadingService.Verify(s => s.Upload(It.IsAny<List<MeterReading>>()), Times.Once);
        }

        [Test]
        public async Task Uploads_WhenEmptyList_ReturnsZeroSuccessAndFailed()
        {
            // Arrange
            var meterReadings = new List<MeterReading>();

            _httpContext.Items["csv"] = meterReadings;

            _mockMeterReadingService
                .Setup(s => s.Upload(It.IsAny<List<MeterReading>>()))
                .ReturnsAsync(0);

            // Act
            var result = await _controller.Uploads();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var response = okResult.Value as UploadResponse;

            Assert.That(response.Success, Is.EqualTo(0));
            Assert.That(response.Failed, Is.EqualTo(0));

            _mockMeterReadingService.Verify(s => s.Upload(It.IsAny<List<MeterReading>>()), Times.Once);
        }

        [Test]
        public async Task Uploads_WhenSingleRecordSuccessfullySaved_ReturnsCorrectCounts()
        {
            // Arrange
            var meterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    MeterReadingId = 1,
                    AccountId = 100,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 12345,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                }
            };

            _httpContext.Items["csv"] = meterReadings;

            _mockMeterReadingService
                .Setup(s => s.Upload(It.IsAny<List<MeterReading>>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.Uploads();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var response = okResult.Value as UploadResponse;

            Assert.That(response.Success, Is.EqualTo(1));
            Assert.That(response.Failed, Is.EqualTo(0));

            _mockMeterReadingService.Verify(s => s.Upload(It.IsAny<List<MeterReading>>()), Times.Once);
        }

        [Test]
        public async Task Uploads_WhenCalled_ReturnsOkResult()
        {
            // Arrange
            var meterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    MeterReadingId = 1,
                    AccountId = 100,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 12345,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                }
            };

            _httpContext.Items["csv"] = meterReadings;

            _mockMeterReadingService
                .Setup(s => s.Upload(It.IsAny<List<MeterReading>>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.Uploads();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task Uploads_WhenServiceIsCalled_PassesCorrectMeterReadingsList()
        {
            // Arrange
            var expectedMeterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    MeterReadingId = 1,
                    AccountId = 100,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 12345,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new MeterReading
                {
                    MeterReadingId = 2,
                    AccountId = 101,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 67890,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                }
            };

            _httpContext.Items["csv"] = expectedMeterReadings;

            List<MeterReading> capturedMeterReadings = null;
            _mockMeterReadingService
                .Setup(s => s.Upload(It.IsAny<List<MeterReading>>()))
                .Callback<List<MeterReading>>(mr => capturedMeterReadings = mr)
                .ReturnsAsync(2);

            // Act
            await _controller.Uploads();

            // Assert
            Assert.That(capturedMeterReadings, Is.Not.Null);
            Assert.That(capturedMeterReadings.Count, Is.EqualTo(2));
            Assert.That(capturedMeterReadings[0].MeterReadingId, Is.EqualTo(1));
            Assert.That(capturedMeterReadings[1].MeterReadingId, Is.EqualTo(2));
        }

        [Test]
        public async Task Uploads_WhenLargeDataset_ReturnsCorrectCounts()
        {
            // Arrange
            var meterReadings = new List<MeterReading>();
            for (int i = 1; i <= 100; i++)
            {
                meterReadings.Add(new MeterReading
                {
                    MeterReadingId = i,
                    AccountId = 100 + i,
                    MeterReadingDateTime = DateTime.UtcNow.AddDays(-i),
                    MeterReadValue = 10000 + i,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                });
            }

            _httpContext.Items["csv"] = meterReadings;

            // Simulate 80 out of 100 records saved successfully
            _mockMeterReadingService
                .Setup(s => s.Upload(It.IsAny<List<MeterReading>>()))
                .ReturnsAsync(80);

            // Act
            var result = await _controller.Uploads();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var response = okResult.Value as UploadResponse;

            Assert.That(response.Success, Is.EqualTo(80));
            Assert.That(response.Failed, Is.EqualTo(20));

            _mockMeterReadingService.Verify(s => s.Upload(It.IsAny<List<MeterReading>>()), Times.Once);
        }

        [Test]
        public async Task Uploads_ResponseObject_HasCorrectProperties()
        {
            // Arrange
            var meterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    MeterReadingId = 1,
                    AccountId = 100,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 12345,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                }
            };

            _httpContext.Items["csv"] = meterReadings;

            _mockMeterReadingService
                .Setup(s => s.Upload(It.IsAny<List<MeterReading>>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.Uploads();

            // Assert
            var okResult = result as OkObjectResult;
            var response = okResult.Value as UploadResponse;

            Assert.That(response, Is.Not.Null);
            Assert.That(response, Has.Property("Success"));
            Assert.That(response, Has.Property("Failed"));
        }

        [Test]
        public async Task Uploads_WhenServiceThrowsException_ExceptionPropagates()
        {
            // Arrange
            var meterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    MeterReadingId = 1,
                    AccountId = 100,
                    MeterReadingDateTime = DateTime.UtcNow,
                    MeterReadValue = 12345,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                }
            };

            _httpContext.Items["csv"] = meterReadings;

            _mockMeterReadingService
                .Setup(s => s.Upload(It.IsAny<List<MeterReading>>()))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _controller.Uploads());

            Assert.That(exception.Message, Is.EqualTo("Database connection failed"));
        }

        #endregion
    }
}