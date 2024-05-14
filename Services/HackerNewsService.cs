using HackerNewsAPI.Interface;
using HackerNewsAPI.Models;
using Microsoft.Extensions.Caching.Memory;

namespace HackerNewsAPI.Services
{
    public class HackerNewsService : IHackerNewsService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public HackerNewsService(HttpClient httpClient, IMemoryCache memoryCache)
        {
            _httpClient = httpClient;
            _cache = memoryCache;
        }

        public async Task<List<Story>> GetTopStoriesAsync()
        {
            List<Story> stories = _cache.Get<List<Story>>("TopStories");
            if (stories == null)
            {
                List<int> storyIds = await _httpClient.GetFromJsonAsync<List<int>>("https://hacker-news.firebaseio.com/v0/topstories.json?print=pretty");
                stories = new List<Story>();

                foreach (var id in storyIds.Take(200))
                {
                    var story = await _httpClient.GetFromJsonAsync<Story>($"https://hacker-news.firebaseio.com/v0/item/{id}.json?print=pretty");
                    if (story != null)
                        stories.Add(story);
                }

                _cache.Set("TopStories", stories, TimeSpan.FromMinutes(60));
            }

            return stories;
        }

        public async Task<List<Story>> GetTopStoriesAsyncMultiThreaded()
        {
            List<Story> stories = _cache.Get<List<Story>>("TopStories");
            if (stories == null)
            {
                List<int> storyIds = await _httpClient.GetFromJsonAsync<List<int>>("https://hacker-news.firebaseio.com/v0/topstories.json?print=pretty");
                var tasks = storyIds.Take(200).Select(id => GetStoryAsync(id));
                stories = (await Task.WhenAll(tasks)).Where(story => story != null).ToList();

                _cache.Set("TopStories", stories, TimeSpan.FromMinutes(60));
            }

            return stories;
        }

        /// <summary>
        /// Third version this can be use if multithreaded can cause issues 
        /// </summary>
        /// <returns></returns>
        public async Task<List<Story>> GetTopStoriesAsyncMultiThreadedWithLimit()
        {
            List<Story> stories = _cache.Get<List<Story>>("TopStories");
            if (stories == null)
            {
                List<int> storyIds = await _httpClient.GetFromJsonAsync<List<int>>("https://hacker-news.firebaseio.com/v0/topstories.json?print=pretty");
                //only 10 semaphore
                var semaphore = new SemaphoreSlim(10);
                var tasks = storyIds.Take(200).Select(async id =>
                {
                    //Take semaphore control
                    await semaphore.WaitAsync();
                    try
                    {
                        return await GetStoryAsync(id);
                    }
                    finally
                    {
                        // Release semaphore control
                        semaphore.Release(); 
                    }
                });

                stories = (await Task.WhenAll(tasks)).Where(story => story != null).ToList();

                _cache.Set("TopStories", stories, TimeSpan.FromMinutes(60)); // Cache for 1 hour
            }

            return stories;
        }

        private async Task<Story> GetStoryAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Story>($"https://hacker-news.firebaseio.com/v0/item/{id}.json?print=pretty");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}
