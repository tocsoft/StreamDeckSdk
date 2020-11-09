using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// Event set from a property inspector to facilitate 2 way coms between inspectore and plugin
    /// </summary>
    public class SendToPluginEvent : StreamDeckInboundActionEvent<JToken>
    {

    }
}
