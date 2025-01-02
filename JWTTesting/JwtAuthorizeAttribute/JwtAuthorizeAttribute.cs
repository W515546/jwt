using System;
using System.Web.Http;  // Correct namespace for AuthorizeAttribute in Web API
using System.IdentityModel.Tokens.Jwt;
//using System.Linq;
//using System.Security.Claims;

using Microsoft.IdentityModel.Tokens;

namespace JJWTTesting.JwtAuthorizeAttribute
{
    public class JwtAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var authorizationHeaderValue = actionContext.Request.Headers.Authorization;
            if (authorizationHeaderValue == null || !authorizationHeaderValue.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            var token = authorizationHeaderValue.Parameter;
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("secret"));
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidIssuer = "https://test-authority.com",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidAudiences = new[] { "test-audience" }
            };

            try
            {
                handler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                return securityToken is JwtSecurityToken;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
