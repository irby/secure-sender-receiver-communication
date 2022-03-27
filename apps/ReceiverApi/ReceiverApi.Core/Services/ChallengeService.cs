using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using ReceiverApi.Core.Configurations;
using ReceiverApi.Core.Models;
using ReceiverApi.Core.Services.Interfaces;

namespace ReceiverApi.Core.Services
{
    public sealed class ChallengeService
    {
        private const string Issuer = "receiverapi";
        private const int TokenDurationMinutes = 10;
        
        private readonly AppConfiguration _appConfig;
        private readonly IStorageService _storageService;

        public ChallengeService(AppConfiguration appConfig, IStorageService storageService)
        {
            _appConfig = appConfig;
            _storageService = storageService;
        }
        
        public async Task<Challenge> GenerateChallenge()
        {
            var message = $"{Guid.NewGuid()}{Guid.NewGuid()}";
            
            var challenge = new Challenge
            {
                Message = message,
                Token = GenerateTokenForChallenge(message)
            };

            return challenge;
        }

        public async Task<bool> ValidateClientMessage(string clientId, string encryptedMessage, string token)
        {
            var message = await GetMessageFromToken(token);
            var publicKey = await _storageService.GetClientPublicKey(clientId);
            
            var rsa = RSA.Create();
            rsa.ImportFromPem(publicKey);
            
            byte[] data = Encoding.UTF8.GetBytes(message);
            byte[] signature = Convert.FromBase64String(encryptedMessage);
            bool isValid = rsa.VerifyData(data, 
                signature, 
                HashAlgorithmName.SHA256, 
                RSASignaturePadding.Pkcs1);

            return isValid;
        }

        private string GenerateTokenForChallenge(string message)
        {
            var claims = new[]
            {
                new Claim("message", message)
            };

            var token = new JwtSecurityToken(Issuer, 
                Issuer, claims, 
                DateTime.UtcNow.AddSeconds(-10), 
                DateTime.UtcNow.AddMinutes(TokenDurationMinutes), 
                _appConfig.SigningCredentials);
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string> GetMessageFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var result = await tokenHandler.ValidateTokenAsync(token, new TokenValidationParameters()
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                IssuerSigningKey = _appConfig.SigningCredentials.Key
            });

            if (!result.IsValid)
            {
                throw new Exception(); // TODO: Change exception type
            }
            return result.Claims["message"].ToString();
        }
    }
}