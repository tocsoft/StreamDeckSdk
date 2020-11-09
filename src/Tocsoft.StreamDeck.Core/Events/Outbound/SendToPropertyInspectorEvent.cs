using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// Event set from a plgin to a property inspector to facilitate 2 way coms between inspector and plugin
    /// </summary>
    public class SendToPropertyInspectorEvent : StreamDeckOutboundActionEvent<JToken>
    {

    }
}
