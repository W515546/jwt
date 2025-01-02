using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Core.Resolving;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JWTTesting.UnitTests.Helpers
{
    public class FakeContainer : IContainer
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private readonly FakeComponentRegistry _fakeRegistry = new FakeComponentRegistry();

        public event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning;
        public event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding;
        public event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning;

        public FakeContainer()
        {
            
        }

        // Expose a helper to register typed services in your dictionary
        public void Register(Type serviceType, object instance)
        {
            _services[serviceType] = instance;

            // Also add a corresponding IComponentRegistration to the registry
            var typedService = new TypedService(serviceType);
            var fakeRegistration = new FakeComponentRegistration(serviceType, instance);
            _fakeRegistry.Register(typedService, fakeRegistration);
        }

        // Must implement ResolveComponent(ResolveRequest) in Autofac 5.0
        public object ResolveComponent(ResolveRequest request)
        {
            if (request.Service is TypedService typed &&
                _services.TryGetValue(typed.ServiceType, out var instance))
            {
                return instance;
            }
            throw new DependencyResolutionException($"No registration for {request.Service}");
        }

        public IComponentRegistry ComponentRegistry => _fakeRegistry;

        public object Tag => "FakeContainer";

        public IDisposer Disposer => new FakeDisposer();

        public ILifetimeScope BeginLifetimeScope()
        {
            // Fire the ChildLifetimeScopeBeginning event if needed:
            ChildLifetimeScopeBeginning?.Invoke(this, new LifetimeScopeBeginningEventArgs(this));
            return this;
        }

        public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction)
        {
            ChildLifetimeScopeBeginning?.Invoke(this, new LifetimeScopeBeginningEventArgs(this));
            return this;
        }

        public ILifetimeScope BeginLifetimeScope(object tag)
        {
            ChildLifetimeScopeBeginning?.Invoke(this, new LifetimeScopeBeginningEventArgs(this));
            return this;
        }

        public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction)
        {
            ChildLifetimeScopeBeginning?.Invoke(this, new LifetimeScopeBeginningEventArgs(this));
            return this;
        }

        public void Dispose()
        {
            CurrentScopeEnding?.Invoke(this, new LifetimeScopeEndingEventArgs(this));
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return new ValueTask();
        }
    }

    // A minimal IDisposer:
    public class FakeDisposer : IDisposer
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly List<IAsyncDisposable> _asyncDisposables = new List<IAsyncDisposable>();

        public void AddInstanceForDisposal(IDisposable instance)
        {
            if (instance != null)
                _disposables.Add(instance);
        }

        public void AddInstanceForAsyncDisposal(IAsyncDisposable instance)
        {
            if (instance != null)
                _asyncDisposables.Add(instance);
        }

        public void Dispose()
        {
            foreach (var disp in _disposables)
                disp.Dispose();
            _disposables.Clear();
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var asyncDisp in _asyncDisposables)
                await asyncDisp.DisposeAsync();
            _asyncDisposables.Clear();
            Dispose();
        }
    }
}
