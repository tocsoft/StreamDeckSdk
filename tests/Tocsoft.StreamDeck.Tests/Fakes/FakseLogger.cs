using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tocsoft.StreamDeck.Tests.Fakes
{
    public class FakeLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public List<(LogLevel logLevel, EventId eventId, object state, Exception exception, string formatted)> Logs { get; } = new List<(LogLevel logLevel, EventId eventId, object state, Exception exception, string formatted)>();
        public IEnumerable<string> Messages => Logs.Select(x => x.formatted);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Logs.Add((logLevel, eventId, state, exception, formatter(state, exception)));
        }
    }
}
