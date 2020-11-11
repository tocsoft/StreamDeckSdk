using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StreamDeckEmulator.Models.ServerEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Tocsoft.StreamDeck.Events;

namespace StreamDeckEmulator.Services
{
    public class ActionStorage
    {
        public string Settings { get; set; }

        public int State { get; set; }
    }

    public class PluginManager
    {
        private readonly List<Plugin> plugins = new List<Plugin>();
        public Settings LaunchSettings { get; }
        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly IConsole console;

        public int WebSocketPort { get; private set; }

        public IReadOnlyList<Plugin> Plugins => this.plugins;

        public string SettingsDirectory { get; }

        public PluginManager(IOptions<Settings> settings, IHostApplicationLifetime applicationLifetime, IServer server, IConsole console)
        {
            this.LaunchSettings = settings.Value;
            this.applicationLifetime = applicationLifetime;
            this.console = console;
            applicationLifetime.ApplicationStarted.Register(() =>
            {
                var serverAddressesFeature = server.Features.Get<IServerAddressesFeature>();
                this.Initialise(serverAddressesFeature.Addresses.SingleOrDefault());
            });

            this.SettingsDirectory = Environment.ExpandEnvironmentVariables(settings.Value.StorageRoot);

            Directory.CreateDirectory(this.SettingsDirectory);
        }

        public Plugin GetPlugin(string uuid)
        {
            return this.plugins.SingleOrDefault(x => x.UUID.Equals(uuid, StringComparison.OrdinalIgnoreCase));
        }


        internal void Initialise(string address)
        {
            Debugger.Launch();

            this.WebSocketPort = new Uri(address).Port;
            var pluginPath = LaunchSettings.Plugin ?? "%APPDATA%\\Elgato\\StreamDeck\\Plugins";

            var paths = pluginPath.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            var pluginFolders = paths.SelectMany(p => Expand(p)).ToList();

            if (int.TryParse(LaunchSettings.Pid, out var pid))
            {
                var process = Process.GetProcessById(pid);

                if (process == null)
                {
                    applicationLifetime.StopApplication();
                    return;
                }

                var pluginFolder = Path.GetDirectoryName(process.MainModule.FileName);

                var p = new Plugin(this, pluginFolder, process);
                this.plugins.Add(p);

                Task.Run(async () =>
                {
                    while (true)
                    {
                        if (process.HasExited)
                        {
                            applicationLifetime.StopApplication();
                            break;
                        }
                        await Task.Delay(250);
                    }
                });

                var data = new
                {
                    port = this.WebSocketPort,
                    pluginUUID = p.UUID,
                    registerEvent = new RegisterPluginEvent().Event,
                    info = "{}"
                };

                console.Write($"\n\n--STREAMDECK_REG_START--\n{StreamDeckEvent.Serialize(data)}\n--STREAMDECK_REG_END--\n");
            }
            else
            {
                this.plugins.AddRange(pluginFolders.Select(p => new Plugin(this, p)));
            }
        }

        private IEnumerable<string> Expand(string path)
        {
            path = Environment.ExpandEnvironmentVariables(path);
            if (path.EndsWith("manifest.json"))
            {
                yield return path.Substring(0, path.Length - "manifest.json".Length);
            }
            else
            {
                if (File.Exists(Path.Combine(path, "manifest.json")))
                {
                    yield return path;
                }
                else
                {
                    var pluginFodlers = Directory.GetDirectories(path, "*.sdPlugin");
                    foreach (var f in pluginFodlers)
                    {
                        if (File.Exists(Path.Combine(f, "manifest.json")))
                        {
                            yield return f;
                        }
                    }
                }
            }
        }

        public async Task Connect(HttpContext context, WebSocket webSocket, CancellationToken cancellationToken = default)
        {
            var connection = new SocketConnection(context, webSocket);

            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(60));

            RegisterEvent result = await connection.WaitForRegistration(cancellationTokenSource.Token);
            if (result == null)
            {
                return;
            }


            if (result is RegisterPluginEvent)
            {
                var plugin = plugins.SingleOrDefault(x => x.UUID.Equals(result.uuid, StringComparison.OrdinalIgnoreCase));
                if (plugin != null)
                {
                    plugin.AttachPluginConnection(connection);
                }
            }

            if (result is RegisterPropertyInspectorEvent)
            {
                bool attached = false;
                foreach (var p in plugins)
                {
                    foreach (var a in p.ActionsTypes)
                    {
                        var action = p.GetActionByContext(result.uuid); // this is the actions context id we use as this UUID to ensure they link correctly!!
                        if (action != null)
                        {
                            attached = true;
                            await action.AttachPropertyInspectorConnection(connection);
                            break;
                        }
                    }
                    if (attached) { break; }
                }
                if (!attached)
                {
                    return;
                }
            }

            // we listen to the connection here, once we see an event we connect that up to a plugin
            await connection.Listen(cancellationToken);
        }
    }
}
