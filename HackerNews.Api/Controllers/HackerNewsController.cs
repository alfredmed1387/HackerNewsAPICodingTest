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
        public async Task<ActionResult<List<BestStoryDto>>> Get([FromQuery] int n = 10)
        {
            if (n < 1 || n > 100)
            {
                _logger.LogWarning("Invalid value for n: {N}", n);
                return BadRequest("n must be between 1 and 100");
            }

            try
            {
                var stories = await _hackerNewsService.GetBestStoriesAsync(n);
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
