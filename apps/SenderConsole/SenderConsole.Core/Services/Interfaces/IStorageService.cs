using System.Threading.Tasks;

namespace SenderConsole.Core.Services.Interfaces
{
    public interface IStorageService
    {
        Task<string> GetClientPrivateKey(string clientId);
        Task SaveApiResponse(string content);
    }
}