using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReceiverApi.Core.Models;
using SenderConsole.Core.Services.Interfaces;

namespace SenderConsole.Core.Services
{
    public class ChallengeService
    {
        private readonly HttpClient _httpClient;
        private readonly IStorageService _storageService;
        
        public ChallengeService(HttpClient httpClient, IStorageService storageService)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:5000");

            _storageService = storageService;
        }

        public async Task<Challenge> GetChallenge()
        {
            Console.WriteLine("Inside get challenge");
            var response = await _httpClient.PostAsync("/api/challenge", null, CancellationToken.None);
            response.EnsureSuccessStatusCode();
            
            var body = await response.Content.ReadAsStringAsync();
            
            var responseModel = new ReceiverResponse()
            {
                StatusCode = (int) response.StatusCode,
                Content = body
            };
            
            await _storageService.SaveApiResponse(JsonConvert.SerializeObject(responseModel));
            var challenge = JsonConvert.DeserializeObject<Challenge>(body);
            return challenge;
        }

        public async Task<string> SignMessageWithPrivateKey(string message)
        {
            var privateKey = await _storageService.GetClientPrivateKey("client");
            var rsa = RSA.Create();
            rsa.ImportFromPem(privateKey);

            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(message));

            var signedHash = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signedHash);
        }
    }
}