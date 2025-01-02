using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;  // Correct namespace for AuthorizeAttribute in Web API
using System.Web.Http.Controllers;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Autofac;
using Autofac.Integration.Web;
using System.Web;

namespace JWTTesting.Attrs
{
    public class JwtAuthorizeAttribute : AuthorizeAttribute
    {

        // first tested with static, no DI
        // public static IConfiguration Configuration { get; set; }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {

            try
            {
                var container = (HttpContext.Current?.ApplicationInstance as IContainerProviderAccessor)?.ContainerProvider?.ApplicationContainer;

                if (container == null)
                {
                    throw new InvalidOperationException("Container is null");
                }

                var configuration = container.Resolve<IConfiguration>();

                var authorizationHeaderValue = actionContext.Request.Headers.Authorization;

                if (authorizationHeaderValue == null || !authorizationHeaderValue.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                {
                    throw new UnauthorizedAccessException("Authorization header is missing or has an invalid scheme");
                }

                if (configuration == null)
                {
                    throw new InvalidOperationException("Configuration is not set");
                }

                var token = authorizationHeaderValue.Parameter;
                var handler = new JwtSecurityTokenHandler();

                var secret = configuration["JwtSettings:Secret"];
                var issuer = configuration["JwtSettings:Issuer"];
                var audience = configuration["JwtSettings:Audience"];
                if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
                {
                    throw new InvalidOperationException("Incomplete config for JWT");
                }

                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret))
                {
                    KeyId = "arbitrary-key-id"  // though we are using symmetric, for some reason I still need a kid
                };

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidAudiences = new[] { audience }
                };

                try
                {
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
            catch { 

                // log

                return false;
            }
        }
    }
}
