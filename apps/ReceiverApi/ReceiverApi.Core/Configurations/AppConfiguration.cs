using Microsoft.IdentityModel.Tokens;

namespace ReceiverApi.Core.Configurations
{
    public class AppConfiguration
    {
        public SigningCredentials SigningCredentials { get; set; }
    }
}