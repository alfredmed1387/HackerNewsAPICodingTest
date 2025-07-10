using HackerNews.Models;
using HackerNews.Services;
using Microsoft.AspNetCore.Mvc;

namespace HackerNews.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HackerNewsController : ControllerBase
    {
        private readonly IHackerNewsService _hackerNewsService;
        private readonly ILogger<HackerNewsController> _logger;
        private const int MaxStories = 100;
        private const int DefaultStories = 10;

        public HackerNewsController(
            IHackerNewsService hackerNewsService,
            ILogger<HackerNewsController> logger)
        {
            _hackerNewsService = hackerNewsService;
            _logger = logger;
        }

        /// <summary>
        /// Get the first n best stories from Hacker News, sorted by score descending.
        /// </summary>
        /// <param name="n">Number of stories to return (default 10, max 100)</param>
        [HttpGet]
        public async Task<ActionResult<List<BestStoryDto>>> Get([FromQuery] int n = DefaultStories)
        {
            // Validate input early
            if (n < 1) n = DefaultStories;
            if (n > MaxStories) n = MaxStories;

            try
            {
                var stories = await _hackerNewsService.GetBestStoriesAsync(n).ConfigureAwait(false);

                if (stories == null || stories.Count == 0)
                    return NotFound();

                return Ok(stories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching best stories from Hacker News.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
