using HackerNews.Models;

namespace HackerNews.Services
{
    public interface IHackerNewsService
    {
        Task<List<BestStoryDto>> GetBestStoriesAsync(int n);
    }
}
