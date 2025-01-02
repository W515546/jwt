using System;
using System.Web;
using System.Web.Http;
using Autofac;
using Autofac.Integration.Web;
using Microsoft.Extensions.Configuration;
using JWTTesting.Services;

namespace JWTTesting.Controllers
{
    [RoutePrefix("api/home")]
    public class HomeController : ApiController
    {
        private readonly ITestService _testService;
        private readonly IConfiguration _configuration;

        public HomeController(ITestService testService)
        {

            _testService = testService;

            var container = ((IContainerProviderAccessor)HttpContext.Current.ApplicationInstance).ContainerProvider.ApplicationContainer;
            _configuration = container.Resolve<IConfiguration>();
        }

        [Route("")]
        [HttpGet]
        public IHttpActionResult Get()
        {

            var testMessage = _testService.GetTestMessages();

            return Ok(new
            {
                Message = testMessage,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
