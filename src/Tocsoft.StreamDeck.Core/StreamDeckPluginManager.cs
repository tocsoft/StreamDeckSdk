using Newtonsoft.Json.Linq;
using PowerArgs;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Tocsoft.StreamDeck.Events;

namespace Tocsoft.StreamDeck
{
    internal class StreamDeckPluginManager : IPluginManager
    {
        private readonly IStreamDeckConnection streamDeckConnection;

        public StreamDeckPluginManager(IStreamDeckConnection streamDeckConnection)
        {
            this.streamDeckConnection = streamDeckConnection;

            streamDeckConnection.Listen<DidReceiveGlobalSettingsEvent>(s =>
            {
                this.CurrentSettings = s.Settings.Settings;
                return Task.CompletedTask;
            });
        }

        public Task LogMessageAsync(string message) =>
            streamDeckConnection.SendEvent(new LogMessageEvent
            {
                Payload = new LogMessagePayload
                {
                    Message = message
                }
            });

        public Task OpenUrlAsync(string url) =>
            streamDeckConnection.SendEvent(new OpenUrlEvent
            {
                Payload = url
            });

        public JObject CurrentSettings { get; private set; } = new JObject();

        public IDisposable OnSettingChange(Action<JObject> action) =>
            streamDeckConnection.Listen<DidReceiveGlobalSettingsEvent>(s =>
            {
                action(s.Settings.Settings);
                return Task.CompletedTask;
            });
    }

    internal class StreamDeckPluginManager<TGlobalSettings> : IPluginManager<TGlobalSettings>
    {
        private readonly StreamDeckPluginManager manager;

        public StreamDeckPluginManager(StreamDeckPluginManager manager)
        {
            this.manager = manager;
        }

        public TGlobalSettings CurrentSettings => manager.CurrentSettings.ToObject<TGlobalSettings>();

        public IDisposable OnSettingChange(Action<TGlobalSettings> action)
            => manager.OnSettingChange(s =>
                {
                    action(s.ToObject<TGlobalSettings>());
                });
    }
}
