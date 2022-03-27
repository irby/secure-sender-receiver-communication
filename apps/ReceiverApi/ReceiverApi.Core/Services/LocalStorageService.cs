using System.IO;
using System.Threading.Tasks;
using ReceiverApi.Core.Services.Interfaces;

namespace ReceiverApi.Core.Services
{
    public class LocalStorageService : IStorageService
    {
        public async Task<string> GetClientPublicKey(string clientId)
        {
            await using var privateKeyFile = File.Open(Path.Combine(Directory.GetCurrentDirectory(), $"{clientId}.pub"), FileMode.Open);
            var sr = new StreamReader(privateKeyFile);
            return await sr.ReadToEndAsync();
        }
    }
}