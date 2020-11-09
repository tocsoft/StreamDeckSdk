using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Tocsoft.StreamDeck.Events;

namespace Tocsoft.StreamDeck
{
    internal class StreamDeckActionManager<TSettings> : IActionManager, IActionManager<TSettings>
    {
        IActionManager innerActionManager;

        public StreamDeckActionManager(IActionManager manager)
        {
            innerActionManager = manager;
            UpdateSettings(manager.CurrentSettings);
            manager.OnSettingsChanged(UpdateSettings);
        }

        private Task UpdateSettings(JObject json)
        {
            if (json == null)
            {
                CurrentSettings = default;
                return Task.CompletedTask;
            }

            var settings = json.ToObject<TSettings>(StreamDeckEvent.EventSerilizer);
            CurrentSettings = settings;
            return Task.CompletedTask;
        }

        public TSettings CurrentSettings { get; private set; }


        public ActionState CurrentState => innerActionManager.CurrentState;

        JObject IActionManager.CurrentSettings => innerActionManager.CurrentSettings;

        public void OnStateChange(Action<ActionState> action)
        {
            innerActionManager.OnStateChange(action);
        }

        public Task SetImageAsync(string path)
        {
            return innerActionManager.SetImageAsync(path);
        }

        public Task SetStateAsync(ActionState state)
        {
            return innerActionManager.SetStateAsync(state);
        }

        public Task ShowAlertAsync()
        {
            return innerActionManager.ShowAlertAsync();
        }

        public Task ShowOkAsync()
        {
            return innerActionManager.ShowOkAsync();
        }

        public void OnSettingsChanged(Func<JObject, Task> action)
        {
            innerActionManager.OnSettingsChanged(action);
        }

        public Task SendToPropertyInspector<T>(T payload)
        {
            return innerActionManager.SendToPropertyInspector(payload);
        }

        public void OnSettingsChanged(Func<TSettings, Task> action)
        {
            innerActionManager.OnSettingsChanged(async (e) =>
            {
                await UpdateSettings(e);
                await action(this.CurrentSettings);
            });
        }
    }
}
