using meter_reading.Application.Exceptions;
using meter_reading.Presentation.Controllers;
using meter_reading.Presentation.Responses;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace meter_reading.Tests.Controllers
{
    [TestFixture]
    public class ErrorControllerTests
    {
        // Minimal IExceptionHandlerFeature implementation for tests
        private sealed class TestExceptionHandlerFeature : IExceptionHandlerFeature
        {
            public Exception Error { get; set; }
            public TestExceptionHandlerFeature(Exception ex) => Error = ex;
        }

        [Test]
        public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
        {
            // Arrange / Act / Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new ErrorController(null!));
            Assert.That(ex!.ParamName, Is.EqualTo("logger"));
        }

        [Test]
        public async Task Process_WithApiException_ReturnsStatusCodeAndErrorResponse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ErrorController>>();
            var sut = new ErrorController(loggerMock.Object);

            var apiEx = new BadRequestException("bad request"); // BadRequestException derives from ApiException

            var context = new DefaultHttpContext();
            context.Features.Set<IExceptionHandlerFeature>(new TestExceptionHandlerFeature(apiEx));

            sut.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            // Act
            var result = await sut.Process();

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var obj = (ObjectResult)result;
            Assert.That(obj.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));

            Assert.That(obj.Value, Is.TypeOf<ErrorResponse>());
            var resp = (ErrorResponse)obj.Value!;
            Assert.That(resp.ErrorMessage, Is.EqualTo($"{HttpStatusCode.BadRequest} : {apiEx.Message}"));
            Assert.That(resp.TraceId, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public async Task Process_WhenNoException_Returns500AndGenericMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ErrorController>>();
            var sut = new ErrorController(loggerMock.Object);

            var context = new DefaultHttpContext();
            // No IExceptionHandlerFeature set => default branch should be taken

            sut.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            // Act
            var result = await sut.Process();

            // Assert
            Assert.That(result, Is.TypeOf<ObjectResult>());
            var obj = (ObjectResult)result;
            Assert.That(obj.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));

            Assert.That(obj.Value, Is.TypeOf<ErrorResponse>());
            var resp = (ErrorResponse)obj.Value!;
            Assert.That(resp.ErrorMessage, Is.EqualTo($"{HttpStatusCode.InternalServerError} : An internal server error occurred."));
            Assert.That(resp.TraceId, Is.Not.EqualTo(Guid.Empty));
        }
    }
}