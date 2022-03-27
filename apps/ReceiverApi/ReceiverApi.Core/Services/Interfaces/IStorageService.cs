using System.Threading.Tasks;

namespace ReceiverApi.Core.Services.Interfaces
{
    public interface IStorageService
    {
        Task<string> GetClientPublicKey(string clientId);
    }
}