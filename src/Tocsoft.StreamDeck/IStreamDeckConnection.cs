using System;
using System.Text.Json;
using System.Threading.Tasks;
using Tocsoft.StreamDeck.Events;

namespace Tocsoft.StreamDeck
{
    public interface IStreamDeckConnection
    {
        public Task Connect();
        public Task Disconnect();

        public IDisposable Listen<TEventType>(Func<TEventType, Task> callback) where TEventType : StreamDeckInboundEvent;

        public Task SendEvent<TEventType>(TEventType details) where TEventType : StreamDeckOutboundEvent;
    }
}
