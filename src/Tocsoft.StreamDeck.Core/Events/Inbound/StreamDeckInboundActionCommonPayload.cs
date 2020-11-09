using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// base class for inbound actino payloads
    /// </summary>
    public class StreamDeckInboundActionCommonPayload
    {
        /// <summary>
        /// the actions coordinates
        /// </summary>
        public ActionLocation Coordinates { get; set; }

        /// <summary>
        /// the actions settings
        /// </summary>
        public JObject Settings { get; set; }

        /// <summary>
        /// the actions state
        /// </summary>
        public ActionState State { get; set; }
    }
}
