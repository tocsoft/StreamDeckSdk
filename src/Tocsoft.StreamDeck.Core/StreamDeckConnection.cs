using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json.Linq;
using PowerArgs.Cli;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using Tocsoft.StreamDeck.Events;

namespace Tocsoft.StreamDeck
{
    internal class StreamDeckConnection : IStreamDeckConnection
    {
        private readonly StartupArguments arguments;
        private readonly ILogger<StreamDeckConnection> logger;
        private readonly EventManager eventManager;
        private readonly IHostApplicationLifetime applicationLifetime;
        private ClientWebSocket client;
        private CancellationTokenSource tcs;

        // this is the thing that managest

        public StreamDeckConnection(StartupArguments arguments, ILogger<StreamDeckConnection> logger, EventManager eventManager, IHostApplicationLifetime applicationLifetime)
        {
            this.arguments = arguments;
            this.logger = logger;
            this.eventManager = eventManager;
            this.applicationLifetime = applicationLifetime;

            // find all event types and map out
        }

        public async Task Connect()
        {
            this.client = new ClientWebSocket();
            await client.ConnectAsync(new Uri($"ws://localhost:{arguments.Port}"), default);

            this.tcs = new CancellationTokenSource();

            _ = Task.Run(async () =>
            {
                try
                {
                    var token = this.tcs.Token;
                    while (!token.IsCancellationRequested)
                    {
                        var (json, closed) = await client.ReceiveTextAsync(token);

                        if (closed)
                        {
                            applicationLifetime.StopApplication();
                        }
                        else
                        {
                            var eventJson = JObject.Parse(json);
                            ProcessEvent(eventJson);
                        }
                    }
                }
                catch (Exception ex)
                {
                    applicationLifetime.StopApplication();
                    throw ex;
                }
            }, this.tcs.Token);


            await client.SendJsonAsync(new
            {
                Uuid = arguments.PluginUUID,
                Event = arguments.RegisterEvent
            }, StreamDeckEvent.EventSerilizer, this.tcs.Token);

        }

        private void ProcessEvent(JObject document)
        {
            if (StreamDeckEvent.TryParse(document, out var evnt, out var eventType))
            {
                Task.Run(() =>
                {
                    return eventManager.TriggerEvent(evnt);
                });
            }
            else
            {
                logger.LogWarning("Unrecongnised event type recieved '{EventType}'", eventType);
            }
        }

        public Task Disconnect()
        {
            client?.Dispose();
            client = null;

            tcs?.Cancel();

            return Task.CompletedTask;
        }

        public IDisposable Listen<TEventType>(Func<TEventType, Task> callback)
            where TEventType : StreamDeckInboundEvent
        {
            return eventManager.Register(callback);
        }

        public Task SendEvent<TEventType>(TEventType details)
            where TEventType : StreamDeckOutboundEvent
        {
            // this is sending an event out to the streamdeck
            return client.SendJsonAsync(details, StreamDeckEvent.EventSerilizer);
        }
    }
}
