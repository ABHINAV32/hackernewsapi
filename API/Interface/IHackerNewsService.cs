using HackerNewsAPI.Models;

namespace HackerNewsAPI.Interface
{
    public interface IHackerNewsService
    {
        public Task<List<Story>> GetTopStoriesAsyncFromServer();

        public Task<List<Story>> GetTopStoriesAsync();

        public Task<List<Story>> GetTopStoriesAsyncMultiThreaded();
    }
}
