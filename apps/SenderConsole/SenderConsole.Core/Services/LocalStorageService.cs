using System.IO;
using System.Text;
using System.Threading.Tasks;
using SenderConsole.Core.Services.Interfaces;

namespace SenderConsole.Core.Services
{
    public class LocalStorageService : IStorageService
    {
        public async Task<string> GetClientPrivateKey(string clientId)
        {
            await using var stream = File.Open(Path.Combine(Directory.GetCurrentDirectory(), $"{clientId}.key"), FileMode.Open);
            var sr = new StreamReader(stream);
            return await sr.ReadToEndAsync();
        }

        public async Task SaveApiResponse(string content)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "SenderConsole.Core", "api_response.txt");
            // await File.WriteAllTextAsync(path, content);
            await using (StreamWriter Writer = new StreamWriter(
                new FileStream(path, 
                    FileMode.Create, 
                    FileAccess.Write))) {
                await Writer.WriteLineAsync(content);
                await Writer.FlushAsync();
            } 
            // var sw = new StreamWriter(file);
            // await sw.WriteAsync(content);
            // await file.WriteAsync(Encoding.UTF8.GetBytes(content), 0, Encoding.UTF8.GetBytes(content).Length);
        }
    }
}