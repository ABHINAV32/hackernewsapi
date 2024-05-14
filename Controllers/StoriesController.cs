using HackerNewsAPI.Interface;
using HackerNewsAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HackerNewsAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StoriesController : ControllerBase
    {
        private readonly ILogger<StoriesController> _logger;
        private readonly IHackerNewsService _newsService;       

        public StoriesController(IHackerNewsService newsService, ILogger<StoriesController> logger)
        {
            _newsService = newsService;
            _logger = logger;
        }

        /// <summary>
        /// Get Top Stories 
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetStories", Name = "GetStories")]
        public async Task<IActionResult> GetTopStories()
        {
            try
            {

                var stories = await _newsService.GetTopStoriesAsync();
                return Ok(stories.Select(s => new { s.Title, s.Url }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch top stories");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// Get Top Stories with multi threading enabled
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetStoriesMultiThreaded", Name = "GetStoriesMultiThreaded")]
        public async Task<IActionResult> GetTopStoriesMultiThreaded()
        {
            try
            {
                var stories = await _newsService.GetTopStoriesAsyncMultiThreaded();
                return Ok(stories.Select(s => new { s.Title, s.Url }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch top stories");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Get Top Stories with multi threading enabled with lmit of number of threads
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetStoriesMultiThreadedWithLimit", Name = "GetStoriesMultiThreadedWithLimit")]
        public async Task<IActionResult> GetTopStoriesMultiThreadedWithLimit()
        {
            try
            {

                var stories = await _newsService.GetTopStoriesAsyncMultiThreadedWithLimit();
                return Ok(stories.Select(s => new { s.Title, s.Url }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch top stories");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}