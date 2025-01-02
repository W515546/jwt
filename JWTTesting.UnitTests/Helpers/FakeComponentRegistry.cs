using Autofac.Core;
using Autofac.Core.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JWTTesting.UnitTests.Helpers
{
    public class FakeComponentRegistry : IComponentRegistry
    {
        private readonly Dictionary<Service, IComponentRegistration> _registrations
            = new Dictionary<Service, IComponentRegistration>();

        public IDictionary<string, object> Properties { get; }
            = new Dictionary<string, object>();

        public FakeComponentRegistry() { }

        public bool Register(IComponentRegistration registration)
        {
            // Decide how to store or handle the registration if needed.
            return true;
        }

        public void Register(Service service, IComponentRegistration registration)
        {
            _registrations[service] = registration;
        }

        public bool TryGetRegistration(Service service, out IComponentRegistration registration)
        {
            return _registrations.TryGetValue(service, out registration);
        }

        public bool IsRegistered(Service service)
        {
            return _registrations.ContainsKey(service);
        }

        // The list of all known registrations
        public IEnumerable<IComponentRegistration> Registrations => _registrations.Values;

        // Typically empty if you don't use custom RegistrationSources
        public IEnumerable<IRegistrationSource> Sources => Enumerable.Empty<IRegistrationSource>();

        public bool HasLocalComponents => false;

        // If you don't need it, return null or throw
        public IComponentRegistryBuilder RegistryBuilder => null;

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service)
        {
            if (_registrations.TryGetValue(service, out var reg))
                return new[] { reg };
            return Enumerable.Empty<IComponentRegistration>();
        }

        // The DecoratorsFor requirement in Autofac 5.0
        // Must return an IReadOnlyList<IComponentRegistration>
        public System.Collections.Generic.IReadOnlyList<IComponentRegistration> DecoratorsFor(IServiceWithType service)
        {
            // If you have no decorators, return an empty list
            return Array.Empty<IComponentRegistration>();
        }

        // IDisposable
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
