using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using System.Threading.Tasks;
using System.Threading;

namespace JWTTesting.Utilities
{
    public class ConfigurationManagerWrapper : IConfigurationManagerWrapper
    {
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

        public ConfigurationManagerWrapper(string authority, bool requireHttps)
        {
            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{authority}/.well-known/oauth-authorization-server",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = requireHttps }
            );
        }

        public Task<OpenIdConnectConfiguration> GetConfigurationAsync()
        {
            return _configurationManager.GetConfigurationAsync();
        }
    }


}

