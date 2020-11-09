using System;
using System.Threading.Tasks;

namespace Tocsoft.StreamDeck
{
    /// <summary>
    /// Access plugin wide shared APIs
    /// </summary>
    public interface IPluginManager
    {
        /// <summary>
        /// have the streamdeck open an URL.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Task OpenUrlAsync(string url);

        /// <summary>
        /// Log a message to the streamdecks logs.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task LogMessageAsync(string message);
    }

    /// <summary>
    /// a plugin manager with access the the plgins strongly typed global settings
    /// </summary>
    /// <typeparam name="TGlobalSettings"></typeparam>
    public interface IPluginManager<TGlobalSettings>
    {
        /// <summary>
        /// the plugins global settings shared across all actions
        /// </summary>
        TGlobalSettings CurrentSettings { get; }

        /// <summary>
        /// register a callback for whenever updated global settings are provided.
        /// </summary>
        /// <param name="callback">action to be called when the global setting are updated.</param>
        void OnSettingChange(Action<TGlobalSettings> callback);
    }
}
