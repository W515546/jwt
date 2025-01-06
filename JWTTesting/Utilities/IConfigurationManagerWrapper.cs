using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Threading;
using System.Threading.Tasks;

namespace JWTTesting.Utilities
{ 
    public interface IConfigurationManagerWrapper
    {
        Task<OpenIdConnectConfiguration> GetConfigurationAsync();
    }
}
