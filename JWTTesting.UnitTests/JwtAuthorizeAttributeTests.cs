using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using Autofac;
using Autofac.Core;
using Autofac.Integration.Web;
using JWTTesting.Attrs;
using JWTTesting.DependencyInjection;
using JWTTesting.UnitTests.Helpers;
using JWTTesting.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace JWTTesting.UnitTests
{

    public class JwtAuthorizeAttributeTests
    {

        private const bool UseMockConfiguration = true;

        private readonly JwtAuthorizeAttribute _attribute;

        private readonly Mock<IConfigurationManagerWrapper> _mockConfigurationManagerWrapper;

        private readonly OpenIdConnectConfiguration _mockOpenIdConfig;

        private const string ValidSecret = "rudolphtherednosedreindeerrudolphtherednosedreindeer";
        private const string ValidIssuer = "https://localhost:5000";
        private const string ValidAudience = "test-audience";

        private const string ValidRSAXML = @"<RSAKeyValue>
  <Modulus>vi0K2iHnY0ZV7ICyAah0DNGw8CVsJhUFkoARkXTEHxoX44NnBSSatsXf3fiuU9NeXBxyFOcjWsl39KAGKupfaxHeungrmaR/H8nMNAzsQD3/0cpogJPmab0eeCzcjgCjTm9YIxBP6sfVVNAwOsygCND6GFpgQ/WOj09NCfW9rQO1F98BhqAPOdw1tIzhKgGKGVE8+dBpwKtVBlifhaQVqBuIU7K0/QXGqIzLbnb7YUnggysd5HWHhOG91ftRPRJgbZvkA9lqvrBIwAEXAV7W99/47npMP+RdOCqeAYOtqfzcfGtI0WyfJjsT/S+rw9dOiT8V2R1eHHhVv/d7MHD8UQ==</Modulus>
  <Exponent>AQAB</Exponent>
  <P>/aJXTltyw1P0PiUkUpJJCnhqrMRzsXAGw9/9Nh3bs2Gz6yubMVffob5A3ChlF4Y0KSKMJifgWZzGcTfTnv80Cmpcr/E7giZWIIEzuOswPqgoYK6oqnLQNfPtpCKUBnCRxUKebPMz+okugX2/Wv4AOEHEisOMtZjmQUs7I0Q+Jm0=</P>
  <Q>v/MrA5APmiFmUay4pPWoe2yIMXrXf0zTwAcxqHiq6cereg+hOhtRmI6gpTGkOKrXasbiDKmSwnEGQnESh5qFjOeM4cNoO83P8GnqILasaX7/gspFIsn0xPsztFa4NFe5DwXAdNHDbfsQPuKQjovtUQu2Z0hHqizw4ytO7cGlTvU=</Q>
  <DP>MFtWR/M5gzIre/m63h9T27IvrHbcE/Vm5Mt8zGeO7rlvAyRSN2sokP7kANWWyJBmu30BuXia1psEXqulJkhD1MSInWbh0KOcgiAAC36TuS54XE6Qi8bOGatDIKsTC9NEh5Z2/BC2VUr4WenupNuOkStA3LAB0NGw/LvqL4QgUVU=</DP>
  <DQ>pf9BH5+iHoTr244nUGEyByrgzQmS+4ART2vpQey4wdvvK2wA3iNp+Si/PJ0li+wSx0CflJvBU24zH5pxj9vNywEkEMnQhoeCYEL4Oro/eCxgA1exWliU2RT0reyg+IM0gw1SVrRg8efBMOD2apAP95rNb5xOfZjH6W02oN5qe1E=</DQ>
  <InverseQ>gh/bFtDSPG+RkJIAKsb91xyE0xZMZXyhCQ3Toy3k5QVWLtpwpAQSHyflgIIYkexMkWhUWgx7D4aUZy6FeQf89TaM6g+1blokXDuw2yU7Gk5xgvQWmWtKZ9kgVHNNQrWMzDd0dmWtT0Ow0zPr+Rxhw7mR+YgqmKenikdjKZa5NDE=</InverseQ>
  <D>HH0/aa+YDANypkgeQojTf8MfBE663ZabG9gDKbzqD3W3e+4IrhjOc5B/ia9yIB0jPt3FClQY9AYH6hXZ7q5nJnFKIjmT4IOnxqGSrX59Gt+e1fUCRDbD1vXNnXes/7nhPQQJmlEcg8G1d+vrObfDRqbVvHW3g57C4T2iPxp49HS9uSH87ZocciFw8h1jvNM06gp0GvGPub5bwZUd8cRCs4uoH0dDG3qDn0zw/0/xi/vdOk+2kIW1hySX/mp0FX3cvMExOuwBy2K62/HB1S+5pytYjaBDO7rFHMdeivoxg9WJjJCKFG/vxAL6nJ7ZqBbY5OvsaBml/mDN8x/IvoCupQ==</D>
</RSAKeyValue>";

        private const string InvalidRSAXML = @"<RSAKeyValue>
  <Modulus>kg1FyQsuJKhOpbuu6nOLD9nf6WQaLMit3mIqgiIdvfzxKY5r21qX+VSIY9CYEtG6vvXdy3U1nu1LXelbTw0hPVxCNUUM+m6ZFjgHdf46zSFEzXXEnurkQW+yvgbxhOf0d0pBea7EEPW2Z65hRNaIq4N5jChMu1VzU/kgWU4PuRIMBi/3yQyyAvEQgizwz7IYWYYz31+XmqFIdgCnXToYO3N6O2P8E/ZyD5M9uNBNwxxbdrwB1JXyD+Oi+EazYC88hy9doNonkZoM4lYa1K6EHwfmaaDdul/EHpodhq2du2VNclknF4LJzMMcXIZE6Ro665e/3Vo61KtcNghGtW7Jzw==</Modulus>
  <Exponent>AQAB</Exponent>
  <P>y+JxCygFYQOLe2Iqf6PuRicatqrhOo2m4zNgGGie8//7qX04e9Tb0wL3d+KrIxTCpTl7B3TUww254z63URD2zPP3M0Vz3CfkvImnIusTi6SHjEgNojeZEafTEwYKxBQa9cC4SUs2KBrdQl7z8x8FhiqsRR92UWyy4bw1OQnGTU0=</P>
  <Q>t2JxXzo5gdi6EXkfp2s9/4xS3AJya0niqA/1tUDgdKQag9awTlI6XTNXpCOl4cW+jQAeiXdwIiA3nGMLUTEn0Sadsze6jNhp+eOqMs7kCjMvCfgY7hG95QcxHmDJlEYcjcU9bUd5imZo0UsFdU2PPPzgEXjKaHnLagYtg4xZlYs=</Q>
  <DP>uVSkN2TwxsgHIWgRg6dLL7/aG5PnWmdq/Xo07tsjKl9VrzKhhobuL7ixpOuJb0Eu0iwW9qcvxAKVJ8lZr7oA7jsSSSvn7obg+LUUFbykLrqncFAK/JsXbCImz1MdtDEmdJ/zpMRWfUw2Nl/D1wsq5VOi9s5rqgZp8KI5qxiEhr0=</DP>
  <DQ>ep3MCEw6H2P2JbDDRYIjVjdRP+Iy9yHT92OUerkCW3tOb1ChWf0JIUAvHYToaWl623R09p1x0J+SD3L6IjOn1+TmFOryRTJ+yaM8LfS/7UID9bp5kNU0D2sboKA+qHGorfJv/B2KWsvW0fpDkAdn+vNqLsSBMhIdvKqczBSP4bk=</DQ>
  <InverseQ>TYEiAR/KCLwqlw4WBVHCRaappK7aubq0AH09R1Pf++uyf+CllDLP8552UDIXDrj6URSmza+kg5Kaz7tk6QRXQKIpiIH4uKF+c4qoZf/GJ/sjTAhtH79cQGRj47SMoOR+UWSs3amdBa5N4S7+/iPz9vckHtY5Xy4PuolRMpWcH0w=</InverseQ>
  <D>AU3NoZgp28yGEzqEB8M4HenPv9lgcHKENt5afpnuX3InXW8dLTKqwhOZBu2BH6qwR6DE6UDNInOZsidxfljhwv0uLCQtk8jL/hcW95AC/cgK9LVuwXQOzW5a9d5bAD92S8a/PRmSor7tPd5Tlck/V1F2Etdc3Zx+lv48GMzQUB8pKv/VwAuDV3C9kEGP4POkG06Dlefl/lwzMlIbKnZQUTVVVYjtVUeO3vEzedDJsyXurgVYmIJf7155jH3jJzxC8p/e0DpqAG/gDQbM+bDINNCX7h7ege1WfcukjyFuwKNBHVncuQFBDa/6lWZNJTsN0Pnz54vtCwzrWJT4RDcXBQ==</D>
</RSAKeyValue>";


        private delegate void TryResolveCallback(Type type, out object result);


        public JwtAuthorizeAttributeTests()
        {

            // ignore SSL
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;


            // Setup configuration
            var configData = new Dictionary<string, string>
            {
                { "JwtSettings:Secret", ValidSecret },
                { "JwtSettings:Issuer", ValidIssuer },
                { "JwtSettings:Audience", ValidAudience },

                { "WexHealth.CDH.Mobile.ConsumerService:OpenIdConnectOptions:Audience", ValidAudience },
                { "WexHealth.CDH.Mobile.ConsumerService:OpenIdConnectOptions:Authority", ValidIssuer },
                { "WexHealth.CDH.Mobile.ConsumerService:OpenIdConnectOptions:RequireHttpsMetadata", "true" },
                { "WexHealth.CDH.Mobile.ConsumerService:OpenIdConnectOptions:Scope", "test-scope" }

            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // First tested with static, no DI
            // JwtAuthorizeAttribute.Configuration = configuration;

            var dependencyInjector = new DependencyInjector();
            var container = dependencyInjector.BuildDependencyContainer(builder =>
            {
                // reg mocked or fake services
                builder.RegisterInstance(configuration).As<IConfiguration>();
            });

            // Set up Fakes for DI resolve

            //don't need the fake container if we use the Dep Inj
            //and mock the BeginLifetimeScope
            //
            //var fakeContainer = new FakeContainer();
            //fakeContainer.Register(typeof(IConfiguration), configuration);

            var mockContainerProvider = new Mock<IContainerProvider>();
            mockContainerProvider
                .Setup(cp => cp.ApplicationContainer)
                //.Returns(fakeContainer);
                .Returns(container);

            var mockApplicationInstance = new Mock<System.Web.HttpApplication>();
            mockApplicationInstance
                .As<IContainerProviderAccessor>()
                .Setup(app => app.ContainerProvider)
                .Returns(mockContainerProvider.Object);

            mockContainerProvider
                .Setup(cp => cp.RequestLifetime)
                .Returns(container.BeginLifetimeScope());

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

            var cp_ = accessor.ContainerProvider;
            Assert.NotNull(cp_);

            var container_ = cp_.ApplicationContainer;
            Assert.NotNull(container_);

            //var resolvedConfig = ((IComponentContext)fakeContainer).Resolve<IConfiguration>();
            //Assert.NotNull(resolvedConfig);

            var resolvedConfig = ((IComponentContext)container).Resolve<IConfiguration>();
            Assert.NotNull(resolvedConfig);

            _attribute = new JwtAuthorizeAttribute();

            if (UseMockConfiguration)
            {
                _mockConfigurationManagerWrapper = new Mock<IConfigurationManagerWrapper>();

                _mockConfigurationManagerWrapper
                    .Setup(wrapper => wrapper.GetConfigurationAsync())
                    .ReturnsAsync(new OpenIdConnectConfiguration
                    {
                        Issuer = ValidIssuer,
                        SigningKeys = { new RsaSecurityKey(CreateRsaPublicKey(ValidRSAXML)) }
                    });

                InjectConfigurationManager();

            }

        }

        [Fact]
        public void IsAuthorized_ValidToken_ReturnsTrue()
        {

            string token = GenerateJwtToken(ValidRSAXML, ValidIssuer, ValidAudience);

            var actionContext = CreateActionContextWithToken(token, "https://localhost:5000/api/test");

            var result = InvokeIsAuthorized(actionContext);

            Assert.True(result);
        }

        [Fact]
        //public void IsAuthorized_NoAuthorizationHeader_ThrowsUnauthorizedAccessException()
        public void IsAuthorized_NoAuthorizationHeader_ReturnsFalse()
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
        public void IsAuthorized_InvalidToken_ReturnsFalse()
        //public void IsAuthorized_InvalidToken_ThrowsUnauthorizedAccessException()
        {
            var token = "invalid-token";

            var actionContext = CreateActionContextWithToken(token, "https://localhost:5000/api/test");

            var result = InvokeIsAuthorized(actionContext);

            Assert.False(result);

            //var exception = Assert.Throws<TargetInvocationException>(() => InvokeIsAuthorized(actionContext));
            //Assert.IsType<UnauthorizedAccessException>(exception.InnerException);
            //Assert.Equal("Token validation failed", exception.InnerException.Message);
        }

        [Fact]
        public void IsAuthorized_InvalidScheme_ReturnsFalse()
        //public void IsAuthorized_InvalidScheme_ThrowsUnauthorizedAccessException()
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
        public void IsAuthorized_InvalidSignature_ReturnsFalse()
        //public void IsAuthorized_InvalidSignature_ThrowsUnauthorizedAccessException()
        {

            string token = GenerateJwtToken(InvalidRSAXML, ValidIssuer, ValidAudience);

            var actionContext = CreateActionContextWithToken(token, "https://localhost:5000/api/test");

            var result = InvokeIsAuthorized(actionContext);

            Assert.False(result);

            //var exception = Assert.Throws<TargetInvocationException>(() => InvokeIsAuthorized(actionContext));
            //Assert.IsType<UnauthorizedAccessException>(exception.InnerException);
            //Assert.Equal("Token validation failed", exception.InnerException.Message);
        }

        [Fact]
        public void IsAuthorized_ExpiredToken_ReturnsFalse()
        //public void IsAuthorized_ExpiredToken_ThrowsUnauthorizedAccessException()
        {
            var token = GenerateJwtToken(
                ValidRSAXML,
                issuer: ValidIssuer,
                audience: ValidAudience,
                expires: DateTime.UtcNow.AddMinutes(-35)  // Expired
            );

            var actionContext = CreateActionContextWithToken(token, "https://localhost:5000/api/test");

            var result = InvokeIsAuthorized(actionContext);

            Assert.False(result);

        }

        [Fact]
        public void IsAuthorized_NotHTTPS_SetsForbiddenResponse_ReturnsTrue()
        {

            string token = GenerateJwtToken(ValidRSAXML, ValidIssuer, ValidAudience);

            var actionContext = CreateActionContextWithToken(token, "http://localhost:5000/api/test");

            InvokeIsAuthorized(actionContext);

            Assert.NotNull(actionContext.Response);

            Assert.Equal(HttpStatusCode.Forbidden, actionContext.Response.StatusCode);

            var errM = actionContext.Response.Content.ReadAsStringAsync().Result;
            Assert.Contains("HTTPS is required", errM);

        }

        private bool InvokeIsAuthorized(HttpActionContext actionContext)
        {
            var methodInfo = typeof(JwtAuthorizeAttribute).GetMethod("IsAuthorized", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return (bool)methodInfo.Invoke(_attribute, new object[] { actionContext });
        }

        private string GenerateJwtToken(string privateKeyXml, string issuer, string audience, DateTime? expires = null)
        {

            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privateKeyXml);

            var key = new RsaSecurityKey(rsa);

            var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

            // Generate a JWT token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: new[] { new Claim("scp", "test-scope") },
                expires: expires ?? DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            // Return the token string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private HttpActionContext CreateActionContextWithToken(string token, string uri)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(uri)
            };
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

        private RSA CreateRsaPublicKey(string rsaXml)
        {
            var rsa = RSA.Create();
            rsa.FromXmlString(rsaXml);
            return rsa;
        }

        private void InjectConfigurationManager()
        {
            var configurationManagerField = typeof(JwtAuthorizeAttribute)
                .GetField("_configurationManagerWrapper", BindingFlags.NonPublic | BindingFlags.Instance);

            if (configurationManagerField != null)
            {
                configurationManagerField.SetValue(_attribute, _mockConfigurationManagerWrapper.Object);
            }
            else
            {
                throw new InvalidOperationException("Could not find _configurationManagerWrapper");
            }
        }


    }
}
