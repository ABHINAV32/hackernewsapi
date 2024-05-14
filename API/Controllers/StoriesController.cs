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
        [HttpGet("GetStoriesFromServer", Name = "GetTopStoriesFromServer")]
        public async Task<IActionResult> GetTopStoriesFromServer()
        {
            try
            {
                this._logger.LogDebug("Executing GetTopStoriesFromServer method");
                var stories = await _newsService.GetTopStoriesAsyncFromServer();
                this._logger.LogDebug("Exiting GetTopStoriesFromServer method");
                return Ok(stories.Select(s => new { s.Title, s.Url }));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch top stories");
                return StatusCode(500, "An error occurred while processing the request.");
            }
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
                this._logger.LogDebug("Executing GetTopStories method");
                var stories = await _newsService.GetTopStoriesAsync();
                this._logger.LogDebug("Exiting GetTopStories method");
                return Ok(stories.Select(s => new { s.Title, s.Url }));
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch top stories");
                return StatusCode(500, "An error occurred while processing the request.");
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
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }
    }
}