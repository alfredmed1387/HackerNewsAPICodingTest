using System.Net.Http.Json;
using HackerNews.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HackerNews.Services.Implementation
{
    public class HackerNewsService : IHackerNewsService
    {
        private readonly string _bestStoriesUrl;
        private readonly string _itemUrlTemplate;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly int _bestStoriesCacheMinutes;
        private readonly int _storyCacheMinutes;
        private readonly int _maxConcurrentRequests;
        private readonly ILogger<HackerNewsService> _logger;

        public HackerNewsService(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IOptions<HackerNewsSettings> settings,
            ILogger<HackerNewsService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _logger = logger;
            _bestStoriesUrl = settings.Value.BestStoriesUrl ?? throw new ArgumentNullException(nameof(settings.Value.BestStoriesUrl), "BestStoriesUrl cannot be null");
            _itemUrlTemplate = settings.Value.ItemUrlTemplate ?? throw new ArgumentNullException(nameof(settings.Value.ItemUrlTemplate), "ItemUrlTemplate cannot be null");
            _bestStoriesCacheMinutes = settings.Value.BestStoriesCacheMinutes;
            _storyCacheMinutes = settings.Value.StoryCacheMinutes;
            _maxConcurrentRequests = settings.Value.MaxConcurrentRequests;
        }

        public async Task<List<BestStoryDto>> GetBestStoriesAsync(int n)
        {
            var client = _httpClientFactory.CreateClient();
            List<int>? storyIds = null;

            try
            {
                storyIds = await _cache.GetOrCreateAsync("beststories", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_bestStoriesCacheMinutes);
                    try
                    {
                        return await client.GetFromJsonAsync<List<int>>(_bestStoriesUrl).ConfigureAwait(false) ?? new List<int>();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to fetch best stories from {Url}", _bestStoriesUrl);
                        return new List<int>();
                    }
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving best story IDs from cache.");
                return new List<BestStoryDto>();
            }

            if (storyIds?.Count == 0)
            {
                _logger.LogWarning("No best story IDs were retrieved.");
                return new List<BestStoryDto>();
            }

            var semaphore = new SemaphoreSlim(_maxConcurrentRequests);

            // Create tasks up front, let SemaphoreSlim throttle concurrency inside each task
            var fetchTasks = (storyIds ?? Enumerable.Empty<int>())
                .Take(n)
                .Select(async id =>
                {
                    await semaphore.WaitAsync().ConfigureAwait(false);
                    try
                    {
                        return await GetStoryAsync(client, id).ConfigureAwait(false);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }).ToList();

            BestStoryDto?[] stories;
            try
            {
                stories = await Task.WhenAll(fetchTasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching best stories.");
                return new List<BestStoryDto>();
            }

            return stories
                .Where(s => s != null)
                .OrderByDescending(s => s!.Score)
                .Take(n)
                .ToList()!;
        }

        private async ValueTask<BestStoryDto?> GetStoryAsync(HttpClient client, int id)
        {
            try
            {
                // Try to get from cache; if present, short-circuit with ValueTask for efficiency
                if (_cache.TryGetValue($"story_{id}", out BestStoryDto? cachedStory) && cachedStory != null)
                    return cachedStory;

                return await _cache.GetOrCreateAsync($"story_{id}", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_storyCacheMinutes);
                    var url = string.Format(_itemUrlTemplate, id);
                    try
                    {
                        var story = await client.GetFromJsonAsync<HackerNewsStory>(url).ConfigureAwait(false);
                        if (story == null)
                        {
                            _logger.LogWarning("Story with ID {Id} not found at {Url}", id, url);
                            return null;
                        }

                        return new BestStoryDto
                        {
                            Title = story.title ?? "",
                            Uri = story.url ?? "",
                            PostedBy = story.by ?? "",
                            Time = DateTimeOffset.FromUnixTimeSeconds(story.time).UtcDateTime,
                            Score = story.score,
                            CommentCount = story.descendants
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to fetch story with ID {Id} from {Url}", id, url);
                        return null;
                    }
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving story with ID {Id} from cache.", id);
                return null;
            }
        }
    }
}
