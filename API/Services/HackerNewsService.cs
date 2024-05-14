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
            if (!_cache.TryGetValue("TopStories", out List<Story> stories))
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
            if (!_cache.TryGetValue("TopStories", out List<Story> stories))
            {
                List<int> storyIds = await _httpClient.GetFromJsonAsync<List<int>>("https://hacker-news.firebaseio.com/v0/topstories.json?print=pretty");
                var tasks = storyIds.Take(200).Select(id => GetStoryAsync(id));
                stories = (await Task.WhenAll(tasks)).Where(story => story != null && !string.IsNullOrEmpty(story.Url) && 
                !string.IsNullOrEmpty(story.Title)).ToList();

                _cache.Set("TopStories", stories, TimeSpan.FromMinutes(60));
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
