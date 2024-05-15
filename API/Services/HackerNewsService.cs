using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HackerNewsAPI.Interface;
using HackerNewsAPI.Models;
using Microsoft.Extensions.Caching.Memory;

namespace HackerNewsAPI.Services
{
    /// <summary>
    /// Service class for interacting with the HackerNews API.
    /// </summary>
    public class HackerNewsService : IHackerNewsService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        private const string BaseUrl = "https://hacker-news.firebaseio.com/v0/";

        /// <summary>
        /// Initializes a new instance of the <see cref="HackerNewsService"/> class.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance.</param>
        /// <param name="memoryCache">The IMemoryCache instance.</param>
        public HackerNewsService(HttpClient httpClient, IMemoryCache memoryCache)
        {
            _httpClient = httpClient;
            _cache = memoryCache;
        }

        /// <summary>
        /// Retrieves the top stories asynchronously from the HackerNews API.
        /// </summary>
        /// <returns>A list of top stories.</returns>
        public async Task<List<Story>> GetTopStoriesAsyncFromServer()
        {
            List<int> storyIds = await _httpClient.GetFromJsonAsync<List<int>>($"{BaseUrl}topstories.json?print=pretty");
            var tasks = storyIds.Take(200).Select(id => GetStoryAsync(id));
            var stories = (await Task.WhenAll(tasks)).Where(story => story != null && !string.IsNullOrEmpty(story.Url) &&
            !string.IsNullOrEmpty(story.Title)).ToList();

            return stories;
        }

        /// <summary>
        /// Retrieves the top stories asynchronously from cache or from the server if not available in cache.
        /// </summary>
        /// <returns>A list of top stories.</returns>
        public async Task<List<Story>> GetTopStoriesAsync()
        {
            if (!_cache.TryGetValue("TopStories", out List<Story> stories))
            {
                List<int> storyIds = await _httpClient.GetFromJsonAsync<List<int>>($"{BaseUrl}topstories.json?print=pretty");
                stories = new List<Story>();

                foreach (var id in storyIds.Take(200))
                {
                    var story = await _httpClient.GetFromJsonAsync<Story>($"{BaseUrl}item/{id}.json?print=pretty");
                    if (story != null)
                        stories.Add(story);
                }

                _cache.Set("TopStories", stories, TimeSpan.FromMinutes(60));
            }

            return stories;
        }

        /// <summary>
        /// Retrieves the top stories asynchronously using multiple threads from cache or from the server if not available in cache.
        /// </summary>
        /// <returns>A list of top stories.</returns>
        public async Task<List<Story>> GetTopStoriesAsyncMultiThreaded()
        {
            if (!_cache.TryGetValue("TopStories", out List<Story> stories))
            {
                List<int> storyIds = await _httpClient.GetFromJsonAsync<List<int>>($"{BaseUrl}topstories.json?print=pretty");
                var tasks = storyIds.Take(200).Select(id => GetStoryAsync(id));
                stories = (await Task.WhenAll(tasks)).Where(story => story != null && !string.IsNullOrEmpty(story.Url) &&
                !string.IsNullOrEmpty(story.Title)).ToList();

                _cache.Set("TopStories", stories, TimeSpan.FromMinutes(60));
            }

            return stories;
        }

        /// <summary>
        /// Retrieves a story asynchronously by its ID.
        /// </summary>
        /// <param name="id">The ID of the story to retrieve.</param>
        /// <returns>The story details.</returns>
        private async Task<Story> GetStoryAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Story>($"{BaseUrl}item/{id}.json?print=pretty");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}
