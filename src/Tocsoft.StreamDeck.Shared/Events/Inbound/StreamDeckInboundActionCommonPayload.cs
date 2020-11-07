using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Tocsoft.StreamDeck.Events
{
    public class StreamDeckInboundActionCommonPayload
    {
        public ActionLocation Coordinates { get; set; }
        public JObject Settings { get; set; }
        public ActionState State { get; set; }
    }
}
