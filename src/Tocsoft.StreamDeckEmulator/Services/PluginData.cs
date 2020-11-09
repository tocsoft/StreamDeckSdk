using System.Collections.Generic;
using System.Text.Json;

namespace StreamDeckEmulator.Services
{
    public class PluginData
    {
        public Dictionary<string, string> Actions { get; set; } = new Dictionary<string, string>();

        public JsonDocument Settings { get; set; } = null;
    }
}
