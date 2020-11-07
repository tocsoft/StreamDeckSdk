using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using StreamDeckEmulator.Models.ServerEvents;
using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tocsoft.StreamDeck;
using Tocsoft.StreamDeck.Events;

namespace StreamDeckEmulator.Services
{
    public class SocketConnection
    {
        private readonly HttpContext context;
        private readonly WebSocket webSocket;
        private EventManager eventManager = new EventManager();
        // each connection has an outbound queue of messages to send
        // each 

        public SocketConnection(HttpContext context, WebSocket webSocket)
        {
            this.context = context;
            this.webSocket = webSocket;
        }

        private void ProcessMessage(JObject json)
        {
            // process and handle event parsing here!!
            if (Tocsoft.StreamDeck.Events.StreamDeckEvent.TryParse(json, out var evnt, out var type))
            {
                Task.Run(() => eventManager.TriggerEvent(evnt));
            }
        }

        public Task Close()
            => webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "closed", default);

        public async Task<RegisterEvent> WaitForRegistration(CancellationToken cancellationToken)
        {
            // loop waiting for register event or timeout (from token)

            var (msg, closed) = await webSocket.ReceiveTextAsync(cancellationToken);
            while (!closed && !cancellationToken.IsCancellationRequested)
            {
                var doc = JObject.Parse(msg);
                if (Tocsoft.StreamDeck.Events.StreamDeckEvent.TryParse(doc, out var evnt, out var type))
                {
                    if (evnt is RegisterEvent re)
                    {
                        return re;
                    }
                }
                (msg, closed) = await webSocket.ReceiveTextAsync(cancellationToken);
            }

            return null;
        }

        public async Task Listen(CancellationToken cancellationToken)
        {
            var (msg, closed) = await webSocket.ReceiveTextAsync(cancellationToken);
            while (!closed && !cancellationToken.IsCancellationRequested)
            {
                ProcessMessage(JObject.Parse(msg));
                (msg, closed) = await webSocket.ReceiveTextAsync(cancellationToken);
            }

            // we eached here porcess closed
            await eventManager.TriggerEvent(new SocketConnectionClosed());
        }

        public async Task Send<T>(T eventMessage) where T : Tocsoft.StreamDeck.Events.StreamDeckEvent
        {
            await webSocket.SendJsonAsync(eventMessage, StreamDeckEvent.EventSerilizer, default);
        }

        public IDisposable Listen<T>(Func<T, Task> callback) where T : Tocsoft.StreamDeck.Events.StreamDeckEvent
        {
            return this.eventManager.Register<T>(callback);
        }

        internal void OnClose(Func<Task> callback)
        {
            eventManager.Register<SocketConnectionClosed>(async e =>
            {
                if (callback != null)
                {
                    await callback();
                }
            });
        }
        private class SocketConnectionClosed
        {

        }
    }
}
