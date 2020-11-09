using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// The settings details when global settings updated
    /// </summary>
    public class DidReceiveGlobalSettingsEventPayload
    {
        /// <summary>
        /// the settings as defined
        /// </summary>
        public JObject Settings { get; set; }
    }
}
