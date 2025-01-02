using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;
using Autofac;
using Autofac.Core;
using Autofac.Integration.Web;
using JWTTesting.Attrs;
using JWTTesting.UnitTests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace JWTTesting.UnitTests
{

    public class JwtAuthorizeAttributeTests
    {
        private readonly JwtAuthorizeAttribute _attribute;
        private const string ValidSecret = "rudolphtherednosedreindeerrudolphtherednosedreindeer";
        private const string ValidIssuer = "https://test-authority.com";
        private const string ValidAudience = "test-audience";

        private delegate void TryResolveCallback(Type type, out object result);


        public JwtAuthorizeAttributeTests()
        {
            // Setup configuration
            var configData = new Dictionary<string, string>
            {
                { "JwtSettings:Secret", ValidSecret },
                { "JwtSettings:Issuer", ValidIssuer },
                { "JwtSettings:Audience", ValidAudience }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // First tested with static, no DI
            // JwtAuthorizeAttribute.Configuration = configuration;

            // Set up Fakes for DI resolve

            var fakeContainer = new FakeContainer();
            fakeContainer.Register(typeof(IConfiguration), configuration);

            var mockContainerProvider = new Mock<IContainerProvider>();
            mockContainerProvider
                .Setup(cp => cp.ApplicationContainer)
                .Returns(fakeContainer);

            var mockApplicationInstance = new Mock<System.Web.HttpApplication>();
            mockApplicationInstance
                .As<IContainerProviderAccessor>()
                .Setup(app => app.ContainerProvider)
                .Returns(mockContainerProvider.Object);

            System.Web.HttpContext.Current = new System.Web.HttpContext(
                new System.Web.HttpRequest("", "http://test.com", ""),
                new System.Web.HttpResponse(null)
            )
            {
                ApplicationInstance = mockApplicationInstance.Object
            };

            // quick sanity check - all should be not null
            var accessor = HttpContext.Current.ApplicationInstance as IContainerProviderAccessor;
            Assert.NotNull(accessor);

            var containerProvider = accessor.ContainerProvider;
            Assert.NotNull(containerProvider);

            var container = containerProvider.ApplicationContainer;
            Assert.NotNull(container);

            var resolvedConfig = ((IComponentContext)fakeContainer).Resolve<IConfiguration>();
            Assert.NotNull(resolvedConfig);
            

            _attribute = new JwtAuthorizeAttribute();
        }

        [Fact]
        public void IsAuthorized_ValidToken_ReturnsTrue()
        {
            var token = GenerateJwtToken(ValidSecret, ValidIssuer, ValidAudience);
            var actionContext = CreateActionContextWithToken(token);

            var result = InvokeIsAuthorized(actionContext);

            Assert.True(result);
        }

        [Fact]
        public void IsAuthorized_NoAuthorizationHeader_ThrowsUnauthorizedAccessException()
        {

            var actionContext = CreateActionContextWithoutToken();

            var result = InvokeIsAuthorized(actionContext);

            Assert.False(result);

            //tests with exceptions
            //var exception = Assert.Throws<TargetInvocationException>(() => InvokeIsAuthorized(actionContext));
            //Assert.IsType<UnauthorizedAccessException>(exception.InnerException);
            //Assert.Equal("Authorization header is missing or has an invalid scheme", exception.InnerException.Message);
        }

        [Fact]
        public void IsAuthorized_InvalidToken_ThrowsUnauthorizedAccessException()
        {
            var token = "invalid-token";

            var actionContext = CreateActionContextWithToken(token);

            var result = InvokeIsAuthorized(actionContext);

            Assert.False(result);

            //var exception = Assert.Throws<TargetInvocationException>(() => InvokeIsAuthorized(actionContext));
            //Assert.IsType<UnauthorizedAccessException>(exception.InnerException);
            //Assert.Equal("Token validation failed", exception.InnerException.Message);
        }

        [Fact]
        public void IsAuthorized_InvalidScheme_ThrowsUnauthorizedAccessException()
        {

            var request = new HttpRequestMessage();

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "invalid-token");

            var actionContext = new HttpActionContext { ControllerContext = new HttpControllerContext { Request = request } };

            var result = InvokeIsAuthorized(actionContext);

            Assert.False(result);

            //var exception = Assert.Throws<TargetInvocationException>(() => InvokeIsAuthorized(actionContext));
            //var innerException = Assert.IsType<UnauthorizedAccessException>(exception.InnerException);
            //Assert.Equal("Authorization header is missing or has an invalid scheme", innerException.Message);
        }

        [Fact]
        public void IsAuthorized_InvalidSignature_ThrowsUnauthorizedAccessException()
        {
            var token = GenerateJwtToken(
                secret: "wrong-secret-key-wrong-secret-key-wrong-secret-key", // must have enough bytes for a 'valid' wrong key
                issuer: ValidIssuer,
                audience: ValidAudience
            );

            var actionContext = CreateActionContextWithToken(token);

            var result = InvokeIsAuthorized(actionContext);

            Assert.False(result);

            //var exception = Assert.Throws<TargetInvocationException>(() => InvokeIsAuthorized(actionContext));
            //Assert.IsType<UnauthorizedAccessException>(exception.InnerException);
            //Assert.Equal("Token validation failed", exception.InnerException.Message);
        }


        // TODO: test bad config from DI, not static

        //[Fact]
        //public void IsAuthorized_NullConfiguration_ThrowsInvalidOperationException()
        //{
        //    // Backup the original configuration
        //    var originalConfiguration = JwtAuthorizeAttribute.Configuration;

        //    try
        //    {
        //        // Arrange: Set Configuration to null
        //        JwtAuthorizeAttribute.Configuration = null;

        //        var token = GenerateJwtToken(ValidSecret, ValidIssuer, ValidAudience);
        //        var actionContext = CreateActionContextWithToken(token);

        //        var exception = Assert.Throws<TargetInvocationException>(() => InvokeIsAuthorized(actionContext));
        //        Assert.IsType<InvalidOperationException>(exception.InnerException);
        //        Assert.Equal("Configuration is not set", exception.InnerException.Message);
        //    }
        //    finally
        //    {
        //        // Restore the original configuration
        //        JwtAuthorizeAttribute.Configuration = originalConfiguration;
        //    }
        //}

        [Fact]
        public void IsAuthorized_ExpiredToken_ThrowsUnauthorizedAccessException()
        {
            var token = GenerateJwtToken(
                secret: ValidSecret,
                issuer: ValidIssuer,
                audience: ValidAudience,
                expires: DateTime.UtcNow.AddMinutes(-35)  // Expired
            );

            var actionContext = CreateActionContextWithToken(token);
            
            var result = InvokeIsAuthorized(actionContext);

            Assert.False(result);

        }


        private bool InvokeIsAuthorized(HttpActionContext actionContext)
        {
            var methodInfo = typeof(JwtAuthorizeAttribute).GetMethod("IsAuthorized", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return (bool)methodInfo.Invoke(_attribute, new object[] { actionContext });
        }

        private string GenerateJwtToken(string secret, string issuer, string audience, DateTime? expires = null)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: new[] { new Claim("scp", "test-scope") },
                expires: expires ?? DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private HttpActionContext CreateActionContextWithToken(string token)
        {
            var request = new HttpRequestMessage();
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var controllerContext = new HttpControllerContext { Request = request };
            return new HttpActionContext { ControllerContext = controllerContext };
        }

        private HttpActionContext CreateActionContextWithoutToken()
        {
            var request = new HttpRequestMessage();
            var controllerContext = new HttpControllerContext { Request = request };
            return new HttpActionContext { ControllerContext = controllerContext };
        }
    }
}
