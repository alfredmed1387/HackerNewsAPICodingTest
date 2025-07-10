using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HackerNews.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace HackerNews.UnitTests.Api.Middleware
{
    public class ExceptionHandlingMiddlewareTests
    {
        [Fact]
        public async Task Invoke_NoException_CallsNextMiddleware()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
            var wasCalled = false;
            RequestDelegate next = ctx =>
            {
                wasCalled = true;
                return Task.CompletedTask;
            };
            var middleware = new ExceptionHandlingMiddleware(next, loggerMock.Object);

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(wasCalled);
            Assert.NotEqual((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        }

        [Fact]
        public async Task Invoke_ExceptionThrown_ReturnsInternalServerErrorAndLogs()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
            var exception = new InvalidOperationException("Test exception");
            RequestDelegate next = ctx => throw exception;
            var middleware = new ExceptionHandlingMiddleware(next, loggerMock.Object);

            // Use a memory stream to capture the response body
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);

            responseBody.Seek(0, SeekOrigin.Begin);
            var responseText = new StreamReader(responseBody).ReadToEnd();
            var responseObj = JsonSerializer.Deserialize<JsonElement>(responseText);
            Assert.True(responseObj.TryGetProperty("error", out var errorProp));
            Assert.Equal("An unexpected error occurred.", errorProp.GetString());

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unhandled exception occurred.")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
