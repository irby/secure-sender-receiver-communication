using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Moq.Protected;
using SenderConsole.Core.Services;
using SenderConsole.Core.Services.Interfaces;
using Xunit;

namespace SenderConsole.Core.Tests.Services
{
    public class ChallengeServiceTests : IAsyncLifetime
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly IServiceProvider _serviceProvider;
        private readonly ChallengeService _service;
        private readonly Mock<HttpMessageHandler> _mockHttp;
        private readonly Mock<IStorageService> _mockStorage;

        private string _privateKey;
        
        public ChallengeServiceTests()
        {
            _serviceCollection = new ServiceCollection();
            _mockHttp = new Mock<HttpMessageHandler>();
            var http = new HttpClient(_mockHttp.Object);
            _serviceCollection.TryAddTransient(x => http);

            _mockStorage = new Mock<IStorageService>();
            _serviceCollection.TryAddTransient(x => _mockStorage.Object);
            
            _serviceCollection.TryAddTransient<ChallengeService>();
            
            _serviceProvider = _serviceCollection.BuildServiceProvider();
            _service = _serviceProvider.GetService<ChallengeService>();
        }

        [Fact]
        public async Task GetChallenge_WhenCalled_ReturnsChallenge()
        {
            const string testChallenge = "{\n    \"message\": \"39b17f62-8cee-429b-a4e5-75b85b32fc0001e8a6cd-5306-48e2-a0c8-216113acea09\",\n    \"token\": \"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJtZXNzYWdlIjoiMzliMTdmNjItOGNlZS00MjliLWE0ZTUtNzViODViMzJmYzAwMDFlOGE2Y2QtNTMwNi00OGUyLWEwYzgtMjE2MTEzYWNlYTA5IiwibmJmIjoxNjQ4MzYwOTMzLCJleHAiOjE2NDgzNjE1NDMsImlzcyI6InJlY2VpdmVyYXBpIiwiYXVkIjoicmVjZWl2ZXJhcGkifQ.XDF0iC9NBEt4MTGY2VvQaGMDy-kj2RABzg_UvVzjKeE\"\n}";
            _mockHttp.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(testChallenge)
                });
            var challenge = await _service.GetChallenge();
            Assert.Equal("39b17f62-8cee-429b-a4e5-75b85b32fc0001e8a6cd-5306-48e2-a0c8-216113acea09", challenge.Message);
            Assert.Equal("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJtZXNzYWdlIjoiMzliMTdmNjItOGNlZS00MjliLWE0ZTUtNzViODViMzJmYzAwMDFlOGE2Y2QtNTMwNi00OGUyLWEwYzgtMjE2MTEzYWNlYTA5IiwibmJmIjoxNjQ4MzYwOTMzLCJleHAiOjE2NDgzNjE1NDMsImlzcyI6InJlY2VpdmVyYXBpIiwiYXVkIjoicmVjZWl2ZXJhcGkifQ.XDF0iC9NBEt4MTGY2VvQaGMDy-kj2RABzg_UvVzjKeE", challenge.Token);
        }
        
        [Fact]
        public async Task GetChallenge_WhenHttpCallReturnsUnsuccessfulResponse_ThrowsHttpExceptionAsync()
        {
            _mockHttp.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage {
                    StatusCode = HttpStatusCode.NotFound
                });
            await Assert.ThrowsAsync<HttpRequestException>(() => _service.GetChallenge());
        }
        
        [Fact]
        public async Task SignChallengeWithPrivateKey_WhenProvidedAMessage_ReturnsSignedMessage()
        {
            _mockStorage.Setup(p => p.GetClientPrivateKey(It.IsAny<string>())).ReturnsAsync(_privateKey);
            var challenge = await _service.SignMessageWithPrivateKey("hello");
            Assert.True(!string.IsNullOrWhiteSpace(challenge));
        }

        private async Task<string> GetClientPrivateKey()
        {
            await using var stream = File.Open(Path.Combine(Directory.GetCurrentDirectory(), "keys", "client.key"),
                FileMode.Open);
            var sr = new StreamReader(stream);
            return await sr.ReadToEndAsync();
        }

        public async Task InitializeAsync()
        {
            _privateKey = await GetClientPrivateKey();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}