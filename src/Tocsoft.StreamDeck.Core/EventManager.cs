using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tocsoft.StreamDeck
{
    internal class EventManager
    {
        private List<EventHandle> listeners = new List<EventHandle>();

        public Task TriggerEvent(object details)
        {
            var type = details.GetType();
            var tasks = listeners.Where(x => x.EventType.IsAssignableFrom(type))
                .Select(x => x.Callback(details))
                .ToList();

            return Task.WhenAll(tasks);
        }

        public int RegisteredCount(Type type)
        {
            return listeners.Where(x => x.EventType.IsAssignableFrom(type)).Count();
        }

        public IDisposable Register(Type type, Func<object, Task> callback)
        {
            var handler = new EventHandle(this, type, callback);
            listeners.Add(handler);
            return handler;
        }

        public IDisposable Register<TEventType>(Func<TEventType, Task> callback)
            => Register(typeof(TEventType), c => callback((TEventType)c));

        private class EventHandle : IDisposable
        {
            private EventManager manager;

            public EventHandle(EventManager manager, Type type, Func<object, Task> callback)
            {
                this.manager = manager;
                EventType = type;
                Callback = callback;
            }

            public Type EventType { get; }

            public Func<object, Task> Callback { get; }

            public void Dispose()
            {
                manager.listeners.Remove(this);
            }
        }
    }
}
