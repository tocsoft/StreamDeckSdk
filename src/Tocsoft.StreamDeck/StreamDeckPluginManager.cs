using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Tocsoft.StreamDeck
{
    public class StreamDeckPluginManager : IPluginManager
    {
        public StreamDeckPluginManager()
        {
        }

        public Task LogMessageAsync(string message)
        {
            throw new NotImplementedException();
        }

        public Task OpenUrlAsync(string url)
        {
            throw new NotImplementedException();
        }

        public Task SendEvent(string message)
        {
            throw new NotImplementedException();
        }

        public JsonDocument CurrentSettings { get; private set; }

        public void OnSettingChange(Action<JsonDocument> action)
        {
        }
    }

    public class StreamDeckPluginManager<TGlobalSettings> : IPluginManager<TGlobalSettings>
    {
        public StreamDeckPluginManager(StreamDeckPluginManager manager)
        {
            UpdateSettings(manager.CurrentSettings);
            manager.OnSettingChange(x =>
            {
                UpdateSettings(x);
            });
        }

        private void UpdateSettings(JsonDocument json)
        {
            var jsonText = json.RootElement.GetRawText();
            var settings = JsonSerializer.Deserialize<TGlobalSettings>(jsonText);
            CurrentSettings = settings;

            //trigger callbacks here
        }

        public TGlobalSettings CurrentSettings { get; private set; }

        public void OnSettingChange(Action<TGlobalSettings> action)
        {

        }
    }
}
