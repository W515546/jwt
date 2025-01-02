using Autofac;
using Autofac.Core;
using Autofac.Core.Resolving;
using System;
using System.Collections.Generic;

namespace JWTTesting.UnitTests.Helpers
{
    public class FakeComponentRegistration : IComponentRegistration
    {
        private readonly Type _serviceType;
        private readonly object _instance;

        public FakeComponentRegistration(Type serviceType, object instance)
        {
            _serviceType = serviceType;
            _instance = instance;
            Id = Guid.NewGuid();
            Services = new Service[] { new TypedService(serviceType) };
        }

        public Guid Id { get; }
        public IEnumerable<Service> Services { get; }
        public IInstanceActivator Activator => null;
        public IComponentLifetime Lifetime => null;
        public InstanceSharing Sharing => InstanceSharing.None;
        public InstanceOwnership Ownership => InstanceOwnership.OwnedByLifetimeScope;

        // Required by Autofac 5.0
        public bool IsAdapterForIndividualComponent => false;
        public IComponentRegistration Target => null;

        public IDictionary<string, object> Metadata { get; }
            = new Dictionary<string, object>();

        public event EventHandler<PreparingEventArgs> Preparing;
        public event EventHandler<ActivatingEventArgs<object>> Activating;
        public event EventHandler<ActivatedEventArgs<object>> Activated;
        public event EventHandler<InstanceLookupEndingEventArgs> ReleaseInstance;

        public void RaisePreparing(IComponentContext context, ref IEnumerable<Parameter> parameters)
        {
            Preparing?.Invoke(
                this,
                new PreparingEventArgs(context, this /* IComponentRegistration */, parameters)
            );
        }

        public void RaiseActivating(IComponentContext context, IEnumerable<Parameter> parameters, ref object instance)
        {
            Activating?.Invoke(
                this,
                new ActivatingEventArgs<object>(
                    context,
                    this /* IComponentRegistration */,
                    parameters,
                    instance
                )
            );
        }

        public void RaiseActivated(IComponentContext context, IEnumerable<Parameter> parameters, object instance)
        {
            Activated?.Invoke(
                this,
                new ActivatedEventArgs<object>(
                    context,
                    this /* IComponentRegistration */,
                    parameters,
                    instance
                )
            );
        }

        public void Dispose()
        {
            // No-op
        }
    }
}
