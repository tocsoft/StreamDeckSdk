using Newtonsoft.Json.Linq;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Tocsoft.StreamDeck
{
    public interface IActionManager
    {
        // we infer all the stuff we can here by magic, settings being one!!!
        Task SetImageAsync(string path); // additional extensions wanted for various image platforms, imagesharp, system.drawing, skiasharp etc

        Task ShowAlertAsync();

        Task ShowOkAsync();

        Task SendToPropertyInspector<T>(T payload);

        public Task SetStateAsync(ActionState state);

        public ActionState CurrentState { get; }

        void OnStateChange(Action<ActionState> action);

        JObject CurrentSettings { get; }

        void OnSettingsChanged(Func<JObject, Task> action);
    }

    public interface IActionManager<TSettings> : IActionManager
    {
        new TSettings CurrentSettings { get; }

        void OnSettingsChanged(Func<TSettings, Task> action);
    }

}
