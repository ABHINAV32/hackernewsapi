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
        public async Task GetTopStoriesAsync_ReturnsStories_FromCache()
        {
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