using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReceiverApi.Core.Models;
using SenderConsole.Core.Services.Interfaces;

namespace SenderConsole.Core.Services
{
    public class DocumentService
    {
        
        private readonly HttpClient _httpClient;
        private readonly IStorageService _storageService;
        
        public DocumentService(HttpClient httpClient, IStorageService storageService)
        {
            _httpClient = httpClient;
            _storageService = storageService;
        }
        
        public async Task<bool> SendDocument(string signedMessage, string token, string fileName, string clientId)
        {
            var requestParams = GetRequestParams(signedMessage, fileName, clientId);
            var paramsString = requestParams.Any() ? 
                "?" + string.Join("&", requestParams.Select(p => $"{p.Key}={p.Value}")) 
                : string.Empty;
            
            var message = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                Content = new StringContent("{\"message\": \"hello world!\"}", Encoding.UTF8, "application/json"),
                RequestUri = new Uri($"https://localhost:5001/api/process{paramsString}")
            };
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(message);
            var body = await response.Content.ReadAsStringAsync();
            
            var responseModel = new ReceiverResponse()
            {
                StatusCode = (int) response.StatusCode,
                Content = body
            };
            
            await _storageService.SaveApiResponse(JsonConvert.SerializeObject(responseModel));

            return response.IsSuccessStatusCode;
        }

        private List<KeyValuePair<string, string>> GetRequestParams(string signedMessage, string fileName, string clientId)
        {
            var requestParams = new List<KeyValuePair<string, string>>();
            if (!string.IsNullOrWhiteSpace(signedMessage))
            {
                requestParams.Add(new KeyValuePair<string, string>("signedMessage", signedMessage));
            }
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                requestParams.Add(new KeyValuePair<string, string>("fileName", fileName));
            }
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                requestParams.Add(new KeyValuePair<string, string>("clientId", clientId));
            }

            return requestParams;
        }
    }
}