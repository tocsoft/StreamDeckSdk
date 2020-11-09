using Newtonsoft.Json.Linq;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Tocsoft.StreamDeck
{
    /// <summary>
    /// Exposes the API to communication with the stream deck on behalf of the current action
    /// </summary>
    public interface IActionManager
    {
        /// <summary>
        /// Sets the action image as displayed.
        /// </summary>
        /// <param name="data">Eather a path or a data uri to use as the image.</param>
        /// <returns></returns>
        Task SetImageAsync(string data); // additional extensions wanted for various image platforms, imagesharp, system.drawing, skiasharp etc

        /// <summary>
        /// Displays for a short time the standard streamdeck alert overlay.
        /// </summary>
        /// <returns></returns>
        Task ShowAlertAsync();

        /// <summary>
        /// Displays for a short time the standard streamdeck ok overlay.
        /// </summary>
        /// <returns></returns>
        Task ShowOkAsync();
        
        /// <summary>
        /// Sends a payload to the currently active property inspector.
        /// </summary>
        /// <typeparam name="T">the type fo the payload to send</typeparam>
        /// <param name="payload">The details that will be recieved by the property inspector</param>
        /// <returns></returns>
        Task SendToPropertyInspector<T>(T payload);

        /// <summary>
        /// Set the currently active state.
        /// </summary>
        /// <param name="state">The state to set the action insatance to.</param>
        /// <returns></returns>
        Task SetStateAsync(ActionState state);

        /// <summary>
        /// The current state the action is set to.
        /// </summary>
        ActionState CurrentState { get; }

        /// <summary>
        /// Registers a callback to be fired whenever the state is updated.
        /// </summary>
        /// <param name="callback"></param>
        void OnStateChange(Action<ActionState> callback);

        /// <summary>
        /// Get the currently configured settings as a JObject.
        /// </summary>
        JObject CurrentSettings { get; }

        /// <summary>
        /// Fired whenever settings are updated.
        /// </summary>
        /// <param name="callback">funcion to be call when updated settings are recieved.</param>
        void OnSettingsChanged(Func<JObject, Task> callback);
    }

    /// <summary>
    /// A version of the action manager with strongly typed settings.
    /// </summary>
    /// <typeparam name="TSettings">the type of the settings</typeparam>
    public interface IActionManager<TSettings> : IActionManager
    {
        /// <summary>
        /// Get the currently configured settings
        /// </summary>
        new TSettings CurrentSettings { get; }

        /// <summary>
        /// Fired whenever settings are updated.
        /// </summary>
        /// <param name="callback">funcion to be call when updated settings are recieved.</param>
        void OnSettingsChanged(Func<TSettings, Task> callback);
    }

}
