using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tocsoft.StreamDeck.Events;

namespace Tocsoft.StreamDeck
{
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
            var executionDirectory = Path.GetDirectoryName(typeof(StreamDeckEmulator).Assembly.Location);


            // find the 'deps.json' files
            // find the 'runtime.dev.config'
            // if the type == project then use 'dotnet run' to the project.
            // if the type == 'package' then find the tools folder in the package folder and run the tool from there!

            // find the package store location
            var command = Environment.GetEnvironmentVariable("StreamDeckEmulatorCommand") ?? "dotnet tool run StreamDeckEmulator -- ";
            var parts = command.TrimEnd().Split(new[] { ' ' });
            var info = new ProcessStartInfo(parts[0]);
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (var a in parts.Skip(1))
            {
                info.ArgumentList.Add(a);
            }
            info.ArgumentList.Add("--Pid");
            info.ArgumentList.Add(Process.GetCurrentProcess().Id.ToString());
            info.ArgumentList.Add("--Plugin");
            info.ArgumentList.Add(dir);
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = false;

            this.process = Process.Start(info);
            this.process.EnableRaisingEvents = true;
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
                    break;
                }
                else if (line == "--STREAMDECK_REG_START--")
                {
                    capturelines = true;
                }
            }
        }

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
