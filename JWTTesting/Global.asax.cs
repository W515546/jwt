using Autofac;
using Autofac.Integration.Web;
using Autofac.Integration.WebApi;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;
using System;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using JWTTesting.Services;
using JWTTesting.Controllers;
using JWTTesting.Attrs;
//using System.Configuration;
//using System.Web.Mvc;

namespace JWTTesting
{
    public class WebApiApplication : HttpApplication, IContainerProviderAccessor
    {
        private static IContainerProvider _containerProvider;

        private static IConfigurationRoot _configurationRoot;

        public IContainerProvider ContainerProvider => _containerProvider;

        protected void Application_Start()
        {
            // Enable PII logging for debugging
            IdentityModelEventSource.ShowPII = true;

            // Configure logging and configuration services
            ConfigureLoggingAndConfigurationService();

            // Setup dependency injection
            _containerProvider = new ContainerProvider(GetDependencyContainer(_configurationRoot, RegisterApplicationResources));

            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(_containerProvider.ApplicationContainer);

            // Register areas and Web API routes
            //AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        public IContainer GetDependencyContainer(IConfiguration configuration, Action<ContainerBuilder> onBuild)
        {
            var builder = new ContainerBuilder();
            onBuild(builder);
            return builder.Build();
        }

        private void RegisterApplicationResources(ContainerBuilder containerBuilder)
        {
            // Register configuration
            containerBuilder.RegisterInstance(_configurationRoot).As<IConfiguration>().SingleInstance();

            // Register Web API controllers
            containerBuilder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            //containerBuilder.RegisterApiControllers(typeof(HomeController).Assembly);

            // Register additional services
            containerBuilder.RegisterType<TestService>().As<ITestService>().InstancePerRequest();

        }

        private void ConfigureLoggingAndConfigurationService()
        {
            try
            {
                // Build configuration
                _configurationRoot = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                .Build();

                //JwtAuthorizeAttribute.Configuration = _configurationRoot;

                //// Example logging setup (optional)
                //Serilog.Log.Logger = new Serilog.LoggerConfiguration()
                //    .WriteTo.Console()
                //    .CreateLogger();

                //// Log application start
                //Serilog.Log.Information("Application has started.");
            }
            catch (Exception ex)
            {
                //Serilog.Log.Error(ex, "Error during configuration or logging setup.");
                //Serilog.Log.CloseAndFlush();
            }
        }
    }
}
