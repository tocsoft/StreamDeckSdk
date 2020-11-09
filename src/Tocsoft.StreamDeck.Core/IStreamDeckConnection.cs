using System;
using System.Text.Json;
using System.Threading.Tasks;
using Tocsoft.StreamDeck.Events;

namespace Tocsoft.StreamDeck
{
    /// <summary>
    /// Encapulates communicating with the streamdeck host application
    /// </summary>
    public interface IStreamDeckConnection
    {
        /// <summary>
        /// Called during startup to connect back to the host
        /// </summary>
        /// <returns></returns>
        public Task Connect();
        
        /// <summary>
        /// disconnect from the host gracefully
        /// </summary>
        /// <returns></returns>
        public Task Disconnect();

        /// <summary>
        /// Register callbacks for inbound events recieved from the host
        /// </summary>
        /// <typeparam name="TEventType">the event (or a base class) of the evetn you wish to listen for.</typeparam>
        /// <param name="callback">callback that will be triggered when an event of thea t type or a type assigneanle to the request type is recieved</param>
        /// <returns></returns>
        public IDisposable Listen<TEventType>(Func<TEventType, Task> callback) where TEventType : StreamDeckInboundEvent;

        /// <summary>
        /// Send a event to the streamdeck host for it to action.
        /// </summary>
        /// <typeparam name="TEventType">type of the event to send</typeparam>
        /// <param name="details">the event details to send.</param>
        /// <returns></returns>
        public Task SendEvent<TEventType>(TEventType details) where TEventType : StreamDeckOutboundEvent;
    }
}
