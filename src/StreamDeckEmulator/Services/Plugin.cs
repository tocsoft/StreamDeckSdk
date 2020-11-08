using StreamDeckEmulator.Models;
using StreamDeckEmulator.Models.ServerEvents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tocsoft.StreamDeck;
using Tocsoft.StreamDeck.Events;

namespace StreamDeckEmulator.Services
{

    public class Plugin
    {
        private readonly Manifest manifest;
        private readonly PluginManager manager;
        private Process process;
        SocketConnection pluginConnection = null;
        public List<Action> actions = new List<Action>();

        public IReadOnlyList<ActionManifest> ActionsTypes { get; }
        public IReadOnlyList<Action> Actions => actions;

        public string RootFolder { get; }

        public string UUID { get; }

        public string SettingsDirectory { get; }

        private string settingsPath;

        public string Name => manifest.Name;
        
        public string PropertyInspectorPath => manifest.PropertyInspectorPath;
        
        public string Icon => manifest.Icon;

        public Plugin(PluginManager manager, string path, Process process = null)
        {
            this.manager = manager;
            this.RootFolder = path;

            var manifestPath = Path.Combine(path, "manifest.json");
            this.manifest = System.Text.Json.JsonSerializer.Deserialize<Models.Manifest>(File.ReadAllText(manifestPath));
            this.process = process;
            this.ActionsTypes = this.manifest.Actions;
            // hash the path for getting storage id!!

            this.UUID = GetHashString(path.ToLower().TrimEnd('\\', '/'));

            // create an action for 
            // load from appdata some peristant storage for this plugin
            this.SettingsDirectory = Path.Combine(manager.SettingsDirectory, this.UUID);
            Directory.CreateDirectory(this.SettingsDirectory);
            this.settingsPath = Path.Combine(SettingsDirectory, "global.json");

            var loaded = LoadPluginData();
            foreach (var la in loaded.Actions)
            {
                var m = this.manifest.Actions.SingleOrDefault(s => s.UUID.Equals(la.Value, StringComparison.OrdinalIgnoreCase));
                if (m != null)
                {
                    this.actions.Add(new Action(la.Key, m, this));
                }
            }
        }
        private void UpdatePluginData(Action<PluginData> updater)
        {
            var data = LoadPluginData();
            updater(data);
            SavePluginData(data);
        }
        private PluginData LoadPluginData()
        {
            if (File.Exists(this.settingsPath))
            {
                var json = File.ReadAllText(this.settingsPath);
                return JsonSerializer.Deserialize<PluginData>(json);
            }
            return new PluginData();
        }

        private void SavePluginData(PluginData data)
        {
            var json = JsonSerializer.Serialize(data);
            File.WriteAllText(this.settingsPath, json);
        }

        private async Task EnsueProcessRegistered()
        {
            if (pluginConnection == null)
            {
                bool canKillProcess = false;
                // process is also off start it!!

                // skip javascript plugins!!???
                if (process == null)
                {
                    var startinfo = new ProcessStartInfo
                    {
                        ArgumentList = {
                            "-port", manager.WebSocketPort.ToString(),
                            "-pluginUUID", this.UUID,
                            "-registerEvent", new RegisterPluginEvent().Event,
                            "-info", "{}"
                        },
                        FileName = Path.Combine(this.RootFolder, manifest.CodePathWin ?? manifest.CodePath),
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        
                    };

                    if (manager.LaunchSettings.AdditionalPluginLaunchArguments != null)
                    {
                        foreach (var a in manager.LaunchSettings.AdditionalPluginLaunchArguments)
                        {
                            startinfo.ArgumentList.Add(a);
                        }
                    }

                    canKillProcess = true;
                    this.process = Process.Start(startinfo);

                    this.process.Exited += (s, e) =>
                    {
                        this.process = null;
                    };
                }

                var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(60)); // 1 min (loads of time) for the plugin process to start and for us to receive the registration
                var token = cancellationTokenSource.Token;
                while (pluginConnection == null)
                {
                    if (token.IsCancellationRequested || this.process?.HasExited != false)
                    {
                        if (canKillProcess)
                        {
                            process?.Kill();
                            process = null;
                        }

                        throw new Exception("Registration timeout out");
                    }

                    await Task.Delay(100);
                }
            }
        }

        public Action GetActionByContext(string contextId)
        {
            return this.actions.SingleOrDefault(x => x.ContextId.Equals(contextId, StringComparison.OrdinalIgnoreCase));
        }

        public Action CreateAction(string uuid)
        {
            var actionDef = this.ActionsTypes.SingleOrDefault(x => x.UUID.Equals(uuid, StringComparison.OrdinalIgnoreCase));
            if (actionDef == null)
            {
                return null;
            }

            var act = new Action(Guid.NewGuid().ToString("N"), actionDef, this);
            actions.Add(act);

            UpdatePluginData(d =>
            {
                d.Actions = actions.ToDictionary(x => x.ContextId, x => x.UUID);
            });

            return act;
        }

        public void DeleteAction(Action action)
        {
            actions.Remove(action);
            action.DeleteSettings();
            UpdatePluginData(d =>
            {
                d.Actions = actions.ToDictionary(x => x.ContextId, x => x.UUID);
            });
        }

        public async Task Send<T>(T eventMessage) where T : Tocsoft.StreamDeck.Events.StreamDeckInboundEvent
        {
            await EnsueProcessRegistered();
            await pluginConnection.Send(eventMessage);
        }

        private static byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        private static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        private EventManager eventManager = new EventManager();

        internal void AttachPluginConnection(SocketConnection connection)
        {
            pluginConnection = connection;
            connection.OnClose(() =>
            {
                pluginConnection = null;
                return Task.CompletedTask;
            });

            connection.Listen<StreamDeckOutboundEvent>(e => eventManager.TriggerEvent(e));
        }

        public IDisposable Listen<T>(Func<T, Task> callback) where T : Tocsoft.StreamDeck.Events.StreamDeckOutboundEvent
        {
            return this.eventManager.Register<T>(callback);
        }

    }
}
