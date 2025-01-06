
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;  // Correct namespace for AuthorizeAttribute in Web API
using System.Web.Http.Controllers;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Autofac;
using Autofac.Integration.Web;
using System.Web;
using JWTTesting.Models;
using JWTTesting.Utilities;
using JWTTesting.DependencyInjection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Net.Http;
using System.Net;

namespace JWTTesting.Attrs
{
    public class JwtAuthorizeAttribute : AuthorizeAttribute
    {

        // first tested with static, no DI
        // public static IConfiguration Configuration { get; set; }

        // we'll inject a config wrapper for testing
        private readonly IConfigurationManagerWrapper _configurationManagerWrapper;

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {

            try
            {

                var configuration = DependencyInjector.GetLifetimeScope().Resolve<IConfiguration>();

                if (configuration == null)
                {
                    throw new InvalidOperationException("Configuration is not set");
                }

                var openIdConnectOptions = GetOpenIdConnectOptions(configuration);

                var authorizationHeaderValue = actionContext.Request.Headers.Authorization;

                if (authorizationHeaderValue == null || !authorizationHeaderValue.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                {
                    throw new UnauthorizedAccessException("Authorization header is missing or has an invalid scheme");
                }

                var token = authorizationHeaderValue.Parameter;

                var handler = new JwtSecurityTokenHandler();

                var configurationManager = _configurationManagerWrapper ??
                    new ConfigurationManagerWrapper(
                        openIdConnectOptions.Authority,
                        bool.TryParse(openIdConnectOptions.RequireHttpsMetadata, out var requireHttps) ? requireHttps : true
                    );

                var config = Task.Run(() => configurationManager.GetConfigurationAsync()).Result;

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidIssuer = openIdConnectOptions.Authority,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = config.SigningKeys,
                    ValidAudiences = new List<string>
                {
                    openIdConnectOptions.Audience
                },
                    RequireSignedTokens = true
                };

                try
                {

                    var isNotHttps = (actionContext.Request.RequestUri.Scheme != Uri.UriSchemeHttps);

                    bool.TryParse(openIdConnectOptions.RequireHttpsMetadata, out var requireHttpsMetadata);

                    if (requireHttpsMetadata && isNotHttps)
                    {
                        // hmmmm. this should probably be OnAuthorize
                        actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "HTTPS is required");
                    }

                    handler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                    return securityToken is JwtSecurityToken;

                    // maybe throw more specific for logs / tests
                    throw new SecurityTokenException("Token is not a valid JWT");
                }
                catch (Exception exc)
                {
                    throw new UnauthorizedAccessException("Token validation failed", exc);
                }

            }
            catch
            {

                // log

                return false;
            }
        }

        private OpenIdConnectOptions GetOpenIdConnectOptions(IConfiguration configurationSection)
        {
            //condition to bypass and allow test to run
            //var configurationSection = DependencyInjector.GetLifetimeScope().Resolve<IConfiguration>();

            if (configurationSection == null)
            {
                // Bypass the conditions and return default options
                return new OpenIdConnectOptions();
            }
            else
            {
                return new OpenIdConnectOptions
                {
                    Audience = configurationSection["WexHealth.CDH.Mobile.ConsumerService:OpenIdConnectOptions:Audience"],
                    Authority = configurationSection["WexHealth.CDH.Mobile.ConsumerService:OpenIdConnectOptions:Authority"],
                    RequireHttpsMetadata = configurationSection["WexHealth.CDH.Mobile.ConsumerService:OpenIdConnectOptions:RequireHttpsMetadata"],
                    Scope = configurationSection["WexHealth.CDH.Mobile.ConsumerService:OpenIdConnectOptions:Scope"]
                };
            }
        }

    }
}



//using System;
//using System.Collections.Generic;
//using System.IdentityModel.Tokens.Jwt;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Http.Controllers;
//using Autofac;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Protocols;
//using Microsoft.IdentityModel.Protocols.OpenIdConnect;
//using Microsoft.IdentityModel.Tokens;
//using JWTTesting.Models;
//using JWTTesting.DependencyInjection;
//using JWTTesting.Utilities;

//namespace JWTTesting.Attrs
//{
//    public class JwtAuthorizeAttribute : System.Web.Http.AuthorizeAttribute
//    {

//        private readonly IConfigurationManagerWrapper _configurationManagerWrapper;

//        protected override bool IsAuthorized(HttpActionContext actionContext)
//        {
//            var openIdConnectOptions = GetOpenIdConnectOptions();

//            var authorizationHeaderValue = actionContext.Request.Headers.Authorization;

//            if (authorizationHeaderValue == null ||
//                !authorizationHeaderValue.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
//            {
//                return false;
//            }

//            var token = authorizationHeaderValue.Parameter;

//            var handler = new JwtSecurityTokenHandler();

//            //var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
//            //       $"{openIdConnectOptions.Authority}/.well-known/oauth-authorization-server",
//            //    new OpenIdConnectConfigurationRetriever(),
//            //    new HttpDocumentRetriever());

//            //var configuration = Task.Run(() => configurationManager.GetConfigurationAsync()).Result;

//            var configurationManager = _configurationManagerWrapper ??
//                new ConfigurationManagerWrapper(
//                    openIdConnectOptions.Authority,
//                    bool.TryParse(openIdConnectOptions.RequireHttpsMetadata, out var requireHttps) ? requireHttps : true
//                );

//            var config = Task.Run(() => configurationManager.GetConfigurationAsync()).Result;

//            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

//            var tokenValidationParameters = new TokenValidationParameters
//            {
//                ValidateAudience = true,
//                ValidateLifetime = true,
//                ValidateIssuer = true,
//                ValidIssuer = openIdConnectOptions.Authority,
//                ValidateIssuerSigningKey = true,
//                IssuerSigningKeys = config.SigningKeys,
//                ValidAudiences = new List<string>
//                {
//                    openIdConnectOptions.Audience
//                },
//                RequireSignedTokens = true
//            };

//            try
//            {
//                var isNotHttps = (actionContext.Request.RequestUri.Scheme != Uri.UriSchemeHttps);

//                bool.TryParse(openIdConnectOptions.RequireHttpsMetadata, out var requireHttpsMetadata);

//                if (requireHttpsMetadata && isNotHttps)
//                {
//                    actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "HTTPS is required");
//                }

//                handler.ValidateToken(token, tokenValidationParameters, out var securityToken);

//                var isSecurityToken = securityToken is JwtSecurityToken;

//                var scope = ((JwtSecurityToken)securityToken).Claims.FirstOrDefault(c => c.Type == "scp")?.Value;

//                return isSecurityToken && scope == openIdConnectOptions.Scope;
//            }
//            catch (SecurityTokenException)
//            {
//                return false;
//            }
//            catch
//            { 
//                return false; 
//            }
//        }

//        private OpenIdConnectOptions GetOpenIdConnectOptions()
//        {
//            //condition to bypass and allow test to run
//            var configurationSection = DependencyInjector.GetLifetimeScope().Resolve<IConfiguration>();

//            if (configurationSection == null)
//            {
//                // Bypass the conditions and return default options
//                return new OpenIdConnectOptions();
//            }
//            else
//            {
//                return new OpenIdConnectOptions
//                {
//                    Audience = configurationSection["WexHealth.CDH.Mobile.ConsumerService:OpenIdConnectOptions:Audience"],
//                    Authority = configurationSection["WexHealth.CDH.Mobile.ConsumerService:OpenIdConnectOptions:Authority"],
//                    RequireHttpsMetadata = configurationSection["WexHealth.CDH.Mobile.ConsumerService:OpenIdConnectOptions:RequireHttpsMetadata"],
//                    Scope = configurationSection["WexHealth.CDH.Mobile.ConsumerService:OpenIdConnectOptions:Scope"]
//                };
//            }
//        }
//    }
//}



