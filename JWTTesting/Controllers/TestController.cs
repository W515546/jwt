using System;
using System.Web;
using System.Web.Http;
using Autofac;
using Autofac.Integration.Web;
using Microsoft.Extensions.Configuration;
using JWTTesting.Attrs;
using JWTTesting.Services;

namespace JWTTesting.Controllers
{
    [JwtAuthorize]
    [RoutePrefix("api/test")]
    public class TestController : ApiController
    {

        private readonly ITestService _testService;
        private readonly IConfiguration _configuration;

        public TestController(ITestService testService)
        {

            _testService = testService;

            // `IConfiguration` is resolved dynamically from the container using `HttpContext`.
            var container = ((IContainerProviderAccessor)HttpContext.Current.ApplicationInstance).ContainerProvider.ApplicationContainer;
            _configuration = container.Resolve<IConfiguration>();
        }

        //[Route("secure")]
        [HttpGet]
        public IHttpActionResult Get()
        {
            return Ok(new
            {
                Message = "Token is valid. You have access to this endpoint.",
                AccessTime = DateTime.UtcNow
            });
        }
    }
}
