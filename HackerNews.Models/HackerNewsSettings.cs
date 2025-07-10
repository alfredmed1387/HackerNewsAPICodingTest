
namespace HackerNews.Models
{
    public class HackerNewsSettings
    {
        public string BestStoriesUrl { get; set; } = string.Empty;
        public string ItemUrlTemplate { get; set; } = string.Empty;
        public int BestStoriesCacheMinutes { get; set; } = 1;
        public int StoryCacheMinutes { get; set; } = 5;
        public int MaxConcurrentRequests { get; set; } = 5;
    }
}
