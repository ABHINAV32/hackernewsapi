using HackerNewsAPI.Controllers;
using HackerNewsAPI.Interface;
using HackerNewsAPI.Models;
using HackerNewsAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace HackerNewsAPITests
{
    [TestClass]
    public class StoriesControllerTests
    {
        private Mock<HttpMessageHandler> _mockHttpHandler;
        private Mock<IHackerNewsService> _mockNewsService;
        private Mock<ILogger<StoriesController>> _mockLogger;
        private IMemoryCache _mockMemoryCache;
        private StoriesController _controller;
        private HackerNewsService _hackerNewsService;
        private HttpClient _httpClient;
        private MemoryCache _cache;

        [TestInitialize]
        public void Initialize()
        {
            _mockHttpHandler = new Mock<HttpMessageHandler>();
            _mockNewsService = new Mock<IHackerNewsService>();
            _mockLogger = new Mock<ILogger<StoriesController>>();
            _httpClient = new HttpClient(_mockHttpHandler.Object);
            _cache = new MemoryCache(new MemoryCacheOptions());
            var cachedStories = new List<Story>
            {
                new Story { Id = 1, Title = "Cached Story 1", Url = "http://cachedstory1.com" },
                new Story { Id = 2, Title = "Cached Story 2", Url = "http://cachedstory2.com" }
            };

            _mockMemoryCache = MockMemoryCacheService.GetMemoryCache(cachedStories);
            _hackerNewsService = new HackerNewsService(_httpClient, _mockMemoryCache);
            _mockNewsService = new Mock<IHackerNewsService>();
            _mockLogger = new Mock<ILogger<StoriesController>>();
            _controller = new StoriesController(_mockNewsService.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task GetTopStoriesFromServer_Returns_OK()
        {
            var expectedStories = new List<Story>
            {
                new Story { Id = 1, Title = "Test Story 1", Url = "http://teststory1.com" },
                new Story { Id = 2, Title = "Test Story 2", Url = "http://teststory2.com" }
            };
            _mockNewsService.Setup(x => x.GetTopStoriesAsyncFromServer()).ReturnsAsync(expectedStories);
            var result = await _controller.GetTopStoriesFromServer();

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var stories = okResult.Value as IEnumerable<object>;
            Assert.IsNotNull(stories);
            Assert.AreEqual(expectedStories.Count, stories.Count());
            Assert.IsTrue(expectedStories.All(s => stories.Any(x => x.GetType().GetProperty("Title").GetValue(x).ToString() == s.Title)));
        }

        [TestMethod]
        public async Task GetTopStoriesAsync_ReturnsStories_FromCache()
        {
            var cachedStories = new List<Story>
            {
                new Story { Id = 1, Title = "Cached Story 1", Url = "http://cachedstory1.com" },
                new Story { Id = 2, Title = "Cached Story 2", Url = "http://cachedstory2.com" }
            };
            var result = await _hackerNewsService.GetTopStoriesAsync();
            Assert.AreEqual(cachedStories.Count, result.Count);
            Assert.IsTrue(result.All(r => cachedStories.Any(c => c.Id == r.Id && c.Title == r.Title)));
            _mockHttpHandler.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }
        [TestMethod]
        public async Task GetTopStories_Returns_OK()
        {
            var expectedStories = new List<Story>
            {
                new Story { Id = 1, Title = "Test Story 1", Url = "http://teststory1.com" },
                new Story { Id = 2, Title = "Test Story 2", Url = "http://teststory2.com" }
            };
            _mockNewsService.Setup(x => x.GetTopStoriesAsync()).ReturnsAsync(expectedStories);

            var result = await _controller.GetTopStories();
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var stories = okResult.Value as IEnumerable<object>;
            Assert.IsNotNull(stories);
            Assert.AreEqual(expectedStories.Count, stories.Count());
            Assert.IsTrue(expectedStories.All(s => stories.Any(x => x.GetType().GetProperty("Title").GetValue(x).ToString() == s.Title)));
        }

        [TestMethod]
        public async Task GetTopStories_Exception_Returns_InternalServerError()
        {
            _mockNewsService.Setup(x => x.GetTopStoriesAsync()).ThrowsAsync(new Exception("Test Exception"));
            var result = await _controller.GetTopStories();
            Assert.IsNotNull(result);
            if (result is ObjectResult objectResult)
            {
                Assert.AreEqual(500, objectResult.StatusCode);
            }
            else
            {
                Assert.Fail($"Expected ObjectResult but found {result.GetType().Name}");
            }
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