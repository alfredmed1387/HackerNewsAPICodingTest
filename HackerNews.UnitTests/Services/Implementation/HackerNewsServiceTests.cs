using System.Net;
using System.Text.Json;
using HackerNews.Models;
using HackerNews.Services.Implementation;
using HackerNews.UnitTests.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace HackerNews.UnitTests.Services.Implementation
{
    public class HackerNewsServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
        private readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());
        private readonly Mock<ILogger<HackerNewsService>> _loggerMock = new();
        private readonly HackerNewsSettings _settings = new()
        {
            BestStoriesUrl = "https://test/beststories",
            ItemUrlTemplate = "https://test/item/{0}",
            BestStoriesCacheMinutes = 1,
            StoryCacheMinutes = 1,
            MaxConcurrentRequests = 2
        };

        private HackerNewsService CreateService(HttpMessageHandler handler)
        {
            var client = new HttpClient(handler);
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
            return new HackerNewsService(
                _httpClientFactoryMock.Object,
                _memoryCache,
                Options.Create(_settings),
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task GetBestStoriesAsync_ReturnsStories_WhenApiReturnsData()
        {
            // Arrange
            var bestStoryIds = new List<int> { 1, 2 };
            var story1 = new { by = "user1", descendants = 10, id = 1, score = 100, time = 1700000000, title = "Story 1", url = "http://story1" };
            var story2 = new { by = "user2", descendants = 5, id = 2, score = 200, time = 1700000100, title = "Story 2", url = "http://story2" };

            var handler = new TestHttpMessageHandler((request) =>
            {
                if (request.RequestUri!.ToString() == _settings.BestStoriesUrl)
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(bestStoryIds))
                    };
                if (request.RequestUri!.ToString() == string.Format(_settings.ItemUrlTemplate, 1))
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(story1))
                    };
                if (request.RequestUri!.ToString() == string.Format(_settings.ItemUrlTemplate, 2))
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(story2))
                    };
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            var service = CreateService(handler);

            // Act
            var result = await service.GetBestStoriesAsync(2);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Story 2", result[0].Title); // Sorted by score descending
            Assert.Equal("Story 1", result[1].Title);
        }

        [Fact]
        public async Task GetBestStoriesAsync_ReturnsStories_WhenApiReturnsDataTest()
        {
            // Arrange
            var bestStoryIds = new List<int> { 1, 2 };
            var story1 = new { by = "user1", descendants = 10, id = 1, score = 100, time = 1700000000, title = "Story 1", url = "http://story1" };
            var story2 = new { by = "user2", descendants = 5, id = 2, score = 200, time = 1700000100, title = "Story 2", url = "http://story2" };

            var handler = new TestHttpMessageHandler((request) =>
            {
                if (request.RequestUri!.ToString() == _settings.BestStoriesUrl)
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(bestStoryIds))
                    };
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });
            var client = new HttpClient(handler);
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            var memoryCache = new Mock<IMemoryCache>();
            memoryCache.Setup(m => m.TryGetValue("beststories", out It.Ref<object>.IsAny)).Returns(false);

            var mockEntry = new Mock<ICacheEntry>();
            memoryCache.Setup(m => m.CreateEntry("beststories")).Returns(mockEntry.Object);

            memoryCache.Setup(m => m.TryGetValue(It.Is<string>(name => name.Contains("story_")) , out It.Ref<object>.IsAny )).Throws(new Exception("Cache error"));

            var service = new HackerNewsService(_httpClientFactoryMock.Object, memoryCache.Object, Options.Create(_settings), _loggerMock.Object);

            // Act
            var result = await service.GetBestStoriesAsync(2);

            // Assert
            Assert.Equal(0, result?.Count);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetBestStoriesAsync_ReturnsEmpty_WhenNoStories()
        {
            // Arrange
            var handler = new TestHttpMessageHandler((request) =>
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("[]")
                };
            });

            var service = CreateService(handler);

            // Act
            var result = await service.GetBestStoriesAsync(5);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBestStoriesAsync_HandlesApiException_AndLogsError()
        {
            // Arrange
            var handler = new TestHttpMessageHandler((request) =>
            {
                throw new HttpRequestException("API error");
            }, CancellationToken.None);

            var service = CreateService(handler);

            // Act
            var result = await service.GetBestStoriesAsync(1);

            // Assert
            Assert.Empty(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch best stories")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetBestStoriesAsync_HandlesStoryNotFound_AndLogsError()
        {
            // Arrange
            var bestStoryIds = new List<int> { 1 };
            var handler = new TestHttpMessageHandler((request) =>
            {
                if (request.RequestUri!.ToString() == _settings.BestStoriesUrl)
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(bestStoryIds))
                    };
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            var service = CreateService(handler);

            // Act
            var result = await service.GetBestStoriesAsync(1);

            // Assert
            Assert.Empty(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to fetch")),
                    It.IsAny<HttpRequestException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetBestStoriesAsync_HandlesEmptyStories_AndLogsWarning()
        {
            // Arrange
            var bestStoryIds = new List<int> { 1 };
            var handler = new TestHttpMessageHandler((request) =>
            {
                if (request.RequestUri!.ToString() == _settings.BestStoriesUrl)
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(bestStoryIds))
                    };
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("null")
                };
            });

            var service = CreateService(handler);

            // Act
            var result = await service.GetBestStoriesAsync(1);

            // Assert
            Assert.Empty(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetBestStoriesAsync_RespectsMaxConcurrentRequests()
        {
            // Arrange
            var bestStoryIds = Enumerable.Range(1, 10).ToList();
            int concurrent = 0, maxObserved = 0;
            var handler = new TestHttpMessageHandler(async (request) =>
            {
                Interlocked.Increment(ref concurrent);
                maxObserved = Math.Max(maxObserved, concurrent);
                await Task.Delay(50); // Simulate work
                Interlocked.Decrement(ref concurrent);

                if (request.RequestUri!.ToString() == _settings.BestStoriesUrl)
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(bestStoryIds))
                    };
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(new
                    {
                        by = "user",
                        descendants = 1,
                        id = 1,
                        score = 1,
                        time = 1700000000,
                        title = "Story",
                        url = "http://story"
                    }))
                };
            });

            var service = CreateService(handler);

            // Act
            var result = await service.GetBestStoriesAsync(5);

            // Assert
            Assert.Equal(5, result.Count);
            Assert.True(maxObserved <= _settings.MaxConcurrentRequests);
        }

        [Fact]
        public async Task GetBestStoriesAsync_ReturnsEmptyList_WhenExceptionThrownGettingCachedItems()
        {
            // Arrange
            var memoryCache = new Mock<IMemoryCache>();
            memoryCache.Setup(m => m.TryGetValue(It.IsAny<string>(), out It.Ref<object>.IsAny)).Throws(new Exception("Cache error"));

            var service = new HackerNewsService(_httpClientFactoryMock.Object, memoryCache.Object, Options.Create(_settings), _loggerMock.Object);

            // Act
            var result = await service.GetBestStoriesAsync(5);

            // Assert
            Assert.Empty(result);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
        
    }
}