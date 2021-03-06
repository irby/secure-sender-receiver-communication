using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
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
        private readonly IServiceCollection _serviceCollection;
        private readonly IServiceProvider _serviceProvider;
        private readonly ChallengeService _service;
        
        private readonly Mock<IStorageService> _mockStorageService;

        private string _privateKey;
        private string _publicKey;
        
        public ChallengeServiceTests()
        {
            _serviceCollection = new ServiceCollection();

            _serviceCollection.AddSingleton(x => new AppConfiguration()
            {
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetSymmetricKey().Result)),
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
            Assert.NotNull(result.Token);
        }
        
        [Fact]
        public async Task ValidateClientMessage_WhenValidSignedMessageIsProvided_ReturnsTrue()
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

        [Fact]
        public async Task ValidateClientMessage_WhenTokenIsExpired_ThrowsExceptionAsync()
        {
            _mockStorageService.Setup(p => p.GetClientPublicKey(It.IsAny<string>())).ReturnsAsync(_publicKey);

            var message = "hello";
            var token = await GenerateSessionToken(message, DateTime.UtcNow.AddSeconds(-1));
            
            var signedMessage = SignMessageWithPrivateKey(message);

            await Assert.ThrowsAsync<Exception>(() => _service.ValidateClientMessage("client", signedMessage, token));
        }

        private static async Task<string> GetClientPublicKey()
        {
            await using var pubKey = File.Open(Path.Combine(Directory.GetCurrentDirectory(), "keys", "client.pub"),
                FileMode.Open);
            var sr = new StreamReader(pubKey);
            return await sr.ReadToEndAsync();
        }
        
        private static async Task<string> GetClientPrivateKey()
        {
            await using var privKey = File.Open(Path.Combine(Directory.GetCurrentDirectory(), "keys", "client.key"),
                FileMode.Open);
            var sr = new StreamReader(privKey);
            return await sr.ReadToEndAsync();
        }

        private static async Task<string> GetSymmetricKey()
        {
            await using var secretKey = File.Open(Path.Combine(Directory.GetCurrentDirectory(), "keys", "receiver.secret.key"),
                FileMode.Open);
            var sr = new StreamReader(secretKey);
            return await sr.ReadToEndAsync();
        }

        private async Task<string> GenerateSessionToken(string message, DateTime expirationTime)
        {
            var appConfig = _serviceProvider.GetService<AppConfiguration>();
            var claims = new[]
            {
                new Claim("message", message)
            };

            var token = new JwtSecurityToken("test", 
                "test", 
                claims, 
                DateTime.UtcNow.AddSeconds(-10), 
                expirationTime, 
                appConfig!.SigningCredentials);
            
            return new JwtSecurityTokenHandler().WriteToken(token);
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