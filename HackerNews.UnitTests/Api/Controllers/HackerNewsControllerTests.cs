using HackerNews.Api.Controllers;
using HackerNews.Models;
using HackerNews.Services;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace HackerNews.UnitTests.Api.Controllers
{
    public class HackerNewsControllerTests
    {
        private readonly Mock<IHackerNewsService> _mockService;
        private readonly Mock<ILogger<HackerNewsController>> _mockLogger;
        private readonly HackerNewsController _controller;

        public HackerNewsControllerTests()
        {
            _mockService = new Mock<IHackerNewsService>();
            _mockLogger = new Mock<ILogger<HackerNewsController>>();
            _controller = new HackerNewsController(_mockService.Object, _mockLogger.Object);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        [InlineData(-5)]
        public async Task Get_ReturnsOk_WithStories_WhenNIsValid(int n)
        {
            // Arrange
            var stories = new List<BestStoryDto>
            {
                new BestStoryDto { Title = "Story 1", Uri = "http://1", PostedBy = "user1", Time = DateTime.UtcNow, Score = 100, CommentCount = 10 },
                new BestStoryDto { Title = "Story 2", Uri = "http://2", PostedBy = "user2", Time = DateTime.UtcNow, Score = 90, CommentCount = 5 }
            };
            _mockService.Setup(s => s.GetBestStoriesAsync( It.IsAny<int>())).ReturnsAsync(stories);

            // Act
            var result = await _controller.Get(n);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedStories = Assert.IsType<List<BestStoryDto>>(okResult.Value);
            Assert.Equal(stories, returnedStories);
        }
        [Fact]
        public async Task Get_ReturnsBadRequest_WhenNoDataReturned()
        {
            //Assert
            int n = 5;

            // Act
            var result = await _controller.Get(n);

            // Assert
            var badRequest = Assert.IsType<NotFoundResult>(result.Result);
            Assert.Equal(404, badRequest.StatusCode);
        }

        [Fact]
        public async Task Get_ReturnsInternalServerError_WhenServiceThrows()
        {
            // Arrange
            int n = 10;
            _mockService.Setup(s => s.GetBestStoriesAsync(n)).ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _controller.Get(n);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Equal("An error occurred while processing your request.", statusResult.Value);
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error occurred while fetching best stories")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
