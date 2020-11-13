using Newtonsoft.Json.Linq;
using StreamDeckEmulator.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Tocsoft.StreamDeck;
using Tocsoft.StreamDeck.Events;

namespace StreamDeckEmulator.Services
{
    public class Action
    {
        private EventManager eventManager = new EventManager();
        public Action(string contextId, ActionManifest actionManifest, Plugin plugin)
        {
            this.ContextId = contextId;
            this.actionManifest = actionManifest;
            this.plugin = plugin;

            this.settingsPath = Path.Combine(plugin.SettingsDirectory, contextId + ".json");
            var data = LoadActionData();
            Settings = data.Settings;
            State = data.State;
            Icon = data.Icon;
            plugin.Listen<StreamDeckOutboundActionEvent>(e =>
            {
                if (e.Context == contextId)
                {
                    return eventManager.TriggerEvent(e);
                }

                return Task.CompletedTask;
            });

            if (string.IsNullOrWhiteSpace(Icon))
            {
                var path = Path.Combine(plugin.RootFolder, actionManifest.Icon + ".png");
                Icon = $"data:image/png;base64," + Convert.ToBase64String(File.ReadAllBytes(path));
            }

            eventManager.Register<SetImageEvent>(c =>
            {
                this.Icon = c.Payload.Image;
                UpdateActionData(d => d.Icon = this.Icon);
                return Task.CompletedTask;
            });

            eventManager.Register<SetSettingsEvent>(SyncSettings);

            // proxy send to property inspector event
            eventManager.Register<SendToPropertyInspectorEvent>(c => propertyEditorConnection?.Send(c) ?? Task.CompletedTask);
        }


        private Task SyncSettings(SetSettingsEvent setttingsEvent)
        {
            this.Settings = setttingsEvent.Payload;
            UpdateActionData(d => d.Settings = this.Settings);
            return this.Send(new DidReceiveSettingsEvent
            {
                Payload = new StreamDeckInboundActionCommonPayload
                {
                    State = this.State,
                    Settings = this.Settings
                }
            });
        }
        public JObject Settings { get; private set; }
        public ActionState State { get; }

        public bool IsVisible { get; private set; } = false;

        SocketConnection propertyEditorConnection = null;
        private readonly ActionManifest actionManifest;
        private readonly Plugin plugin;
        private readonly string settingsPath;

        public string UUID => actionManifest.UUID;
        public string Name => actionManifest.Name;
        public string PropertyInspectorPath => actionManifest.PropertyInspectorPath ?? plugin.PropertyInspectorPath;
        public string Icon { get; private set; }
        public string ContextId { get; }

        public async Task Show()
        {
            await this.Send(new WillAppearEvent()
            {
                Payload = new StatePayload
                {
                    Settings = Settings,
                    Coordinates = new ActionLocation
                    {
                        Column = 1,
                        Row = 1
                    },
                    IsInMultiAction = false,
                    State = State,
                    UserDesiredState = State,
                }
            });
            this.IsVisible = true;
        }

        public async Task Hide()
        {
            await this.Send(new WillDisappearEvent()
            {
                Payload = new StatePayload
                {
                    Settings = Settings,
                    Coordinates = new ActionLocation
                    {
                        Column = 1,
                        Row = 1
                    },
                    IsInMultiAction = false,
                    State = State,
                    UserDesiredState = State,
                }
            });
            this.IsVisible = true;
        }


        public async Task Send<T>(T eventMessage) where T : Tocsoft.StreamDeck.Events.StreamDeckInboundActionEvent
        {
            eventMessage.Action = actionManifest.UUID;
            eventMessage.Context = this.ContextId;
            eventMessage.Device = "";// handle fake devices etc later!!!
            await plugin.Send(eventMessage);
            if (propertyEditorConnection != null)
            {
                await propertyEditorConnection.Send(eventMessage);
            }
        }

        internal async Task AttachPropertyInspectorConnection(SocketConnection connection)
        {
            propertyEditorConnection = connection;

            await plugin.Send(new PropertyInspectorDidAppearEvent());

            var listener = connection.Listen<SendToPluginEvent>(e =>
            {
                return this.Send(e);
            });

            connection.Listen<SetSettingsEvent>(SyncSettings);

            connection.OnClose(async () =>
            {
                propertyEditorConnection = null;

                await plugin.Send(new PropertyInspectorDidDisappearEvent());
            });
        }

        private void UpdateActionData(Action<ActionData> updater)
        {
            var data = LoadActionData();
            updater(data);
            SaveActionData(data);
        }
        private ActionData LoadActionData()
        {
            try
            {
                if (File.Exists(this.settingsPath))
                {
                    using (var fr = File.OpenText(this.settingsPath))
                    {
                        return (ActionData)StreamDeckEvent.EventSerilizer.Deserialize(fr, typeof(ActionData));
                    }
                }
            }
            catch
            {
                // ignore maybe we should log!!!
            }
            return new ActionData();

        }

        private void SaveActionData(ActionData data)
        {
            using (var tr = File.CreateText(this.settingsPath))
            {
                StreamDeckEvent.EventSerilizer.Serialize(tr, data);
            }
        }

        public IDisposable Listen<T>(Func<T, Task> callback) where T : Tocsoft.StreamDeck.Events.StreamDeckOutboundActionEvent
        {
            return this.eventManager.Register<T>(callback);
        }
        public async Task KeyDown()
        {
            var desiredState = this.State;
            if (this.actionManifest.States.Length > 1)
            {
                if (desiredState == ActionState.Default)
                {
                    desiredState = ActionState.Alternative;
                }
                else
                {
                    desiredState = ActionState.Default;
                }
            }

            await this.Send(new KeyDownEvent
            {
                Payload = new StatePayload
                {
                    State = this.State,
                    Settings = this.Settings,
                    UserDesiredState = desiredState,
                    Coordinates = new ActionLocation { Column = 1, Row = 1 },
                    IsInMultiAction = false
                }
            });
        }

        internal void DeleteSettings()
        {
            try
            {
                File.Delete(this.settingsPath);
            }
            catch
            {
                //dont really care
            }
        }

        public async Task KeyUp()
        {
            var desiredState = this.State;
            if (this.actionManifest.States.Length > 1)
            {
                if (desiredState == ActionState.Default)
                {
                    desiredState = ActionState.Alternative;
                }
                else
                {
                    desiredState = ActionState.Default;
                }
            }

            await this.Send(new KeyUpEvent
            {
                Payload = new StatePayload
                {
                    State = this.State,
                    Settings = this.Settings,
                    UserDesiredState = desiredState,
                    Coordinates = new ActionLocation { Column = 1, Row = 1 },
                    IsInMultiAction = false
                }
            });
        }
    }
}
