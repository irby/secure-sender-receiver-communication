using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using ReceiverApi.Core.Configurations;
using ReceiverApi.Core.Services;
using ReceiverApi.Core.Services.Interfaces;
using Xunit;

namespace ReceiverApi.Core.Tests.Services
{
    public class ChallengeServiceTests : IAsyncLifetime
    {
        private IServiceCollection _serviceCollection;
        private IServiceProvider _serviceProvider;
        private ChallengeService _service;
        
        private Mock<IStorageService> _mockStorageService;

        private string _privateKey;
        private string _publicKey;
        
        public ChallengeServiceTests()
        {
            _serviceCollection = new ServiceCollection();

            _serviceCollection.AddSingleton(x => new AppConfiguration()
            {
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes("5WKYL7MDMRFY3Z2XDIYRLKPHZ4======")),
                    SecurityAlgorithms.HmacSha256)
            });

            _mockStorageService = new Mock<IStorageService>();
            
            _serviceCollection.AddTransient(x => _mockStorageService.Object);
            _serviceCollection.AddTransient<ChallengeService>();
            
            _serviceProvider = _serviceCollection.BuildServiceProvider();

            _service = _serviceProvider.GetService<ChallengeService>();
        }

        [Fact]
        public async Task GenerateChallenge_WhenCalled_GeneratesChallenge()
        {
            var result = await _service.GenerateChallenge();
            Assert.NotNull(result.Message);
        }
        
        [Fact]
        public async Task ValidateClientMessage_WhenSignedMessageIsProvided_ReturnsTrue()
        {
            _mockStorageService.Setup(p => p.GetClientPublicKey(It.IsAny<string>())).ReturnsAsync(_publicKey);
            
            var challenge = await _service.GenerateChallenge();
            var message = challenge.Message;

            var signedMessage = SignMessageWithPrivateKey(message);
            
            var result = await _service.ValidateClientMessage("client", signedMessage, challenge.Token);
            Assert.True(result);
        }
        
        [Fact]
        public async Task ValidateClientMessage_WhenSignedMessageIsNotValid_ReturnsFalse()
        {
            _mockStorageService.Setup(p => p.GetClientPublicKey(It.IsAny<string>())).ReturnsAsync(_publicKey);
            
            var challenge = await _service.GenerateChallenge();
            var message = challenge.Message;

            var signedMessage = SignMessageWithPrivateKey(message);
            signedMessage = signedMessage.Replace(signedMessage[10], (char) (signedMessage[10] + 1));
            
            var result = await _service.ValidateClientMessage("client", signedMessage, challenge.Token);
            Assert.False(result);
        }
        
        [Fact]
        public async Task ValidateClientMessage_WhenTokenIsInvalid_ThrowsExceptionAsync()
        {
            _mockStorageService.Setup(p => p.GetClientPublicKey(It.IsAny<string>())).ReturnsAsync(_publicKey);
            
            var challenge = await _service.GenerateChallenge();
            var message = challenge.Message;
            challenge.Token = challenge.Token.Replace(challenge.Token[10], (char) (challenge.Token[10] + 1));

            var signedMessage = SignMessageWithPrivateKey(message);
            signedMessage = signedMessage.Replace(signedMessage[10], (char) (signedMessage[10] + 1));
            
            await Assert.ThrowsAsync<Exception>(() =>  _service.ValidateClientMessage("client", signedMessage, challenge.Token)); // TODO: Update exception type
        }

        private async Task<string> GetClientPublicKey()
        {
            await using var pubKey = File.Open(Path.Combine(Directory.GetCurrentDirectory(), "keys", "client.pub"),
                FileMode.Open);
            var sr = new StreamReader(pubKey);
            return await sr.ReadToEndAsync();
        }
        
        private async Task<string> GetClientPrivateKey()
        {
            await using var privKey = File.Open(Path.Combine(Directory.GetCurrentDirectory(), "keys", "client.key"),
                FileMode.Open);
            var sr = new StreamReader(privKey);
            return await sr.ReadToEndAsync();
        }

        private string SignMessageWithPrivateKey(string message)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(_privateKey);
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(message));
            var signedHash = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signedHash);
        }
            
        public async Task InitializeAsync()
        {
            _privateKey = await GetClientPrivateKey();
            _publicKey = await GetClientPublicKey();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}