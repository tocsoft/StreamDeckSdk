using System;
using System.Threading.Tasks;
using Tocsoft.StreamDeck.Events;

namespace Tocsoft.StreamDeck
{
    internal interface IInProcessStreamDeckConnection
    {
        IDisposable Listen<TEventType>(Func<TEventType, Task> callback) where TEventType : StreamDeckEvent;
        Task SendEvent<TEventType>(TEventType details) where TEventType : StreamDeckOutboundEvent;
        Task TriggerEvent<TEventType>(TEventType details) where TEventType : StreamDeckInboundEvent;
    }
}