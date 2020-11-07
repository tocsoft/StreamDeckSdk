using System;
using System.Threading.Tasks;
using Tocsoft.StreamDeck.Events;

namespace Tocsoft.StreamDeck
{
    internal class InProcessStreamDeckConnection : IStreamDeckConnection, IInProcessStreamDeckConnection
    {
        private EventManager eventManager;

        internal InProcessStreamDeckConnection(EventManager eventManager)
        {
            this.eventManager = eventManager;
        }
        public InProcessStreamDeckConnection() : this(new EventManager())
        {
        }

        // this is the thing that managest
        IDisposable IStreamDeckConnection.Listen<TEventType>(Func<TEventType, Task> callback)
        {
            return eventManager.Register(callback);
        }

        public IDisposable Listen<TEventType>(Func<TEventType, Task> callback) where TEventType : StreamDeck.Events.StreamDeckEvent
        {
            return eventManager.Register(callback);
        }

        public Task SendEvent<TEventType>(TEventType details) where TEventType : StreamDeckOutboundEvent
            => eventManager.TriggerEvent(details);

        public Task TriggerEvent<TEventType>(TEventType details) where TEventType : StreamDeckInboundEvent
            => eventManager.TriggerEvent(details);

        public Task Connect()
            => Task.CompletedTask;

        public Task Disconnect()
            => Task.CompletedTask;
    }
}
