using System.IO;
using System.Threading.Tasks;
using ReceiverApi.Core.Services.Interfaces;

namespace ReceiverApi.Core.Services
{
    public class LocalStorageService : IStorageService
    {
        public async Task<string> GetClientPublicKey(string clientId)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "keys", $"{clientId}.pub");
            await using var privateKeyFile = File.Open(path, FileMode.Open);
            var sr = new StreamReader(privateKeyFile);
            return await sr.ReadToEndAsync();
        }
    }
}