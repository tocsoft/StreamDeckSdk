using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json.Linq;
using PowerArgs.Cli;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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
        private readonly Dictionary<string, Type> events;
        private ClientWebSocket client;
        private CancellationTokenSource tcs;
        private Task task;

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

            this.task = Task.Run(async () =>
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

    internal class StreamDeckEmulator : IStreamDeckConnection, IDisposable
    {
        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly ILogger<StreamDeckConnection> logger;
        private readonly EventManager eventManager;

        // this is the thing that managest
        public StreamDeckEmulator(IHostApplicationLifetime applicationLifetime, ILogger<StreamDeckConnection> logger, EventManager eventManager)
        {
            this.applicationLifetime = applicationLifetime;
            this.logger = logger;
            this.eventManager = eventManager;
        }

        public async Task Connect()
        {
            // start emulator process if availible else lets logout that global tool is unavailible
            // emulator starting with a specific argument will get it to write out 
            var command = Environment.GetEnvironmentVariable("StreamDeckEmulatorCommand") ?? "dotnet tool StreamDeckEmulator";
            var parts = command.Split(new[] { ' ' });
            var info = new ProcessStartInfo(parts[0]);
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (var a in parts.Skip(1))
            {
                info.ArgumentList.Add(a);
            }
            info.ArgumentList.Add("--Pid");
            info.ArgumentList.Add(Process.GetCurrentProcess().Id.ToString());
            info.ArgumentList.Add("--Plugins");
            info.ArgumentList.Add(dir);
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = false;

            this.process = Process.Start(info);

            this.process.Exited += (s, e) =>
            {
                applicationLifetime.StopApplication();
            };

            StringBuilder dataReceived = new StringBuilder();
            Regex matcher = new Regex(@"(.*?)--STREAMDECK_REG_END--", RegexOptions.Singleline);
            bool capturelines = false;
            while (true)
            {
                var line = await process.StandardOutput.ReadLineAsync();
                if (capturelines)
                {
                    var settings = StreamDeckEvent.Deserialize<StartupArguments>(line);
                    this.connection = new StreamDeckConnection(settings, logger, eventManager, applicationLifetime);
                    await this.connection.Connect();
                    Process.Start(new ProcessStartInfo($"http://localhost:{settings.Port}") { UseShellExecute = true });
                    break;
                }
                else if (line == "--STREAMDECK_REG_START--")
                {
                    capturelines = true;
                }
            }
        }

        private string dataReceived = "";
        private Process process;
        private IStreamDeckConnection connection;

        public Task Disconnect()
        {
            // kill emulator process too here
            var con = connection;
            connection = null;
            return con?.Disconnect() ?? Task.CompletedTask;
        }

        public IDisposable Listen<TEventType>(Func<TEventType, Task> callback) where TEventType : StreamDeckInboundEvent
        {
            return connection.Listen(callback);
        }

        public Task SendEvent<TEventType>(TEventType details) where TEventType : StreamDeckOutboundEvent
        {
            return connection.SendEvent(details);
        }

        public void Dispose()
        {
            process?.Kill();
            process = null;
        }
    }
}
