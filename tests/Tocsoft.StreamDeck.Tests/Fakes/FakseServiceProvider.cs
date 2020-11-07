using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tocsoft.StreamDeck.Tests.Fakes
{
    public class FakeServiceProvider : IServiceProvider, IServiceScopeFactory
    {
        public List<FakeServiceProviderScope> Scopes { get; } = new List<FakeServiceProviderScope>();

        Dictionary<Type, object> services = new Dictionary<Type, object>();

        public void AddService<TImplemenentation>(TImplemenentation instance)
        {
            services[typeof(TImplemenentation)] = instance;
        }

        public IServiceScope CreateScope()
        {
            var s = new FakeServiceProviderScope()
            {
                ServiceProvider = this
            };
            Scopes.Add(s);
            return s;

        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IServiceScopeFactory))
            {
                return this;
            }
            else if (services.TryGetValue(serviceType, out var s))
            {
                return s;
            }

            return null;
        }
    }

    public class FakeServiceProviderScope : IServiceScope
    {
        public bool Disposed { get; set; } = false;
        public IServiceProvider ServiceProvider { get; set; }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}
