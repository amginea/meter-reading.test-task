using meter_reading.Application.Exceptions;
using meter_reading.Application.Middlewares;
using Microsoft.AspNetCore.Http;

namespace meter_reading.Tests.Middlewares
{
    [TestFixture]
    public class CsvUploadValidatorMiddlewareTests
    {
        [Test]
        public void InvokeAsync_RequestToUploadEndpointWithoutForm_ThrowsBadRequestException()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = new PathString("/api/v1/meter-readings/uploads");
            // ensure no form content type
            context.Request.ContentType = null;

            var nextCalled = false;
            RequestDelegate next = _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var sut = new CsvUploadValidatorMiddleware(next);

            // Act & Assert
            Assert.ThrowsAsync<BadRequestException>(async () => await sut.InvokeAsync(context));
            Assert.That(nextCalled, Is.False, "Next delegate should not be called when validation fails.");
        }

        [Test]
        public async Task InvokeAsync_RequestToUploadEndpointWithForm_CallsNext()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = new PathString("/api/v1/meter-readings/uploads");
            // mark request as form content
            context.Request.ContentType = "multipart/form-data; boundary=----WebKitFormBoundary";

            var nextCalled = false;
            RequestDelegate next = _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var sut = new CsvUploadValidatorMiddleware(next);

            // Act
            await sut.InvokeAsync(context);

            // Assert
            Assert.That(nextCalled, Is.True, "Next delegate should be invoked when request has form content.");
        }

        [Test]
        public async Task InvokeAsync_RequestToNonUploadEndpointWithoutForm_CallsNext()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = new PathString("/health");
            context.Request.ContentType = null;

            var nextCalled = false;
            RequestDelegate next = _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var sut = new CsvUploadValidatorMiddleware(next);

            // Act
            await sut.InvokeAsync(context);

            // Assert
            Assert.That(nextCalled, Is.True, "Next delegate should be invoked for non-upload endpoints regardless of content type.");
        }
    }
}