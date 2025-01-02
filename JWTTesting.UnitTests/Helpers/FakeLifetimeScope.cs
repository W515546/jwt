using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Resolving;

namespace JWTTesting.UnitTests.Helpers
{
    public class FakeLifetimeScope : ILifetimeScope
    {
        private readonly Dictionary<Type, object> _services;

        public FakeLifetimeScope()
            : this(new Dictionary<Type, object>())
        {
        }

        public FakeLifetimeScope(Dictionary<Type, object> services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public void Register(Type serviceType, object instance)
        {
            _services[serviceType] = instance;
        }

        public object Tag => "FakeLifetimeScope";

        public event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning;
        public event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding;
        public event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning;

        public IDisposer Disposer => new FakeDisposer();

        public IComponentRegistry ComponentRegistry => null;

        public object ResolveComponent(ResolveRequest request)
        {
            if (request.Service is TypedService typedService && _services.TryGetValue(typedService.ServiceType, out var service))
            {
                return service;
            }

            throw new DependencyResolutionException($"Service of type {request.Service} is not registered.");
        }

        public ILifetimeScope BeginLifetimeScope()
        {
            return this; 
        }

        public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction)
        {
            return this; 
        }

        public ILifetimeScope BeginLifetimeScope(object tag)
        {
            return this; 
        }

        public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction)
        {
            return this; 
        }

        public void Dispose()
        {
           
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return new ValueTask();
        }
    }
}
