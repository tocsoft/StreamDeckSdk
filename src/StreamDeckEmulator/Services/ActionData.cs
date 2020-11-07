using Newtonsoft.Json.Linq;
using System.Text.Json;
using Tocsoft.StreamDeck;

namespace StreamDeckEmulator.Services
{
    public class ActionData
    {
        public ActionState State { get; set; }

        public string Icon { get; set; }

        // title settings here!!!
        public JObject Settings { get; set; }
    }
}
