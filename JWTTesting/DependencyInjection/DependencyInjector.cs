using Autofac;
using Autofac.Integration.Web;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Web;

namespace JWTTesting.DependencyInjection
{
    public class DependencyInjector
    {
        private static IContainer _container;

        public IContainer Container => _container;

        public IContainer BuildDependencyContainer(Action<ContainerBuilder> registerTypes)
        {
            var containerBuilder = new ContainerBuilder();

            // Register IConfiguration
            var configuration = BuildConfiguration();
            containerBuilder.RegisterInstance(configuration).As<IConfiguration>().SingleInstance();

            // Register other types
            registerTypes(containerBuilder);

            return _container = containerBuilder.Build();
        }

        public static ILifetimeScope GetLifetimeScope()
        {
            if (HttpContext.Current == null)
            {
                return null;
            }
            else
            {
                var containerProviderAccessor = (IContainerProviderAccessor)HttpContext.Current.ApplicationInstance;
                if (containerProviderAccessor != null)
                {
                    return containerProviderAccessor.ContainerProvider.RequestLifetime;
                }

                // Fallback to container
                return _container.BeginLifetimeScope();
            }
        }

        private static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
