using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// Sent to update the instance settins of an action.
    /// </summary>
    public class SetSettingsEvent : StreamDeckOutboundActionEvent<JObject>
    {

    }
}
