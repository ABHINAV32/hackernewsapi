using HackerNewsAPI.Models;
using HackerNewsAPI.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;

namespace HackerNewsAPITest
{

    [TestClass]
    public class HackerNewsServiceTests
    {
        private Mock<HttpMessageHandler> _mockHttpHandler;
        private HttpClient _httpClient;
        private MemoryCache _cache;
        private IMemoryCache _mockMemoryCache;
        private HackerNewsService _hackerNewsService;

        [TestInitialize]
        public void Initialize()
        {
            _mockHttpHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpHandler.Object);
            _cache = new MemoryCache(new MemoryCacheOptions());
            var cachedStories = new List<Story>
            {
                new Story { Id = 1, Title = "Cached Story 1", Url = "http://cachedstory1.com" },
                new Story { Id = 2, Title = "Cached Story 2", Url = "http://cachedstory2.com" }
            };

            _mockMemoryCache = MockMemoryCacheService.GetMemoryCache(cachedStories);
            _hackerNewsService = new HackerNewsService(_httpClient, _mockMemoryCache);
        }

        [TestMethod]
        public async Task GetTopStoriesAsync_ReturnsStories_WhenCacheIsEmpty()
        {
            var cachedStories = new List<Story>();
            _mockMemoryCache = MockMemoryCacheService.GetMemoryCache(cachedStories);
            _hackerNewsService = new HackerNewsService(_httpClient, _mockMemoryCache);

            var fakeStoryIds = Enumerable.Range(1, 200).ToList();
            var fakeStories = fakeStoryIds.Select(id => new Story { Id = id, Title = $"Story {id}", Url = $"http://storyurl{id}.com" }).ToList();

            _mockHttpHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
               ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.AbsoluteUri.Contains("topstories.json")),
               ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(JsonConvert.SerializeObject(fakeStoryIds))
               });

            foreach (var story in fakeStories)
            {
                _mockHttpHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.AbsoluteUri.Contains($"item/{story.Id}.json")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonConvert.SerializeObject(story))
                    });
            }
            var result = await _hackerNewsService.GetTopStoriesAsync();

            Assert.AreEqual(200, result.Count); // Assert the number of stories fetched matches expected number
            _mockHttpHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(201), // Once for the top stories and 200 times for each story
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task GetTopStoriesAsync_ReturnsStories_FromCache()
        {
            // Arrange
            var cachedStories = new List<Story>
            {
                new Story { Id = 1, Title = "Cached Story 1", Url = "http://cachedstory1.com" },
                new Story { Id = 2, Title = "Cached Story 2", Url = "http://cachedstory2.com" }
            };

            // Act
            var result = await _hackerNewsService.GetTopStoriesAsync();

            // Assert
            Assert.AreEqual(cachedStories.Count, result.Count);
            Assert.IsTrue(result.All(r => cachedStories.Any(c => c.Id == r.Id && c.Title == r.Title)));
            _mockHttpHandler.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        public static class MockMemoryCacheService
        {
            public static IMemoryCache GetMemoryCache(object expectedValue)
            {
                var mockMemoryCache = new Mock<IMemoryCache>();
                mockMemoryCache
                    .Setup(x => x.TryGetValue(It.IsAny<object>(), out expectedValue))
                    .Returns(true);
                return mockMemoryCache.Object;
            }
        }
    }
}