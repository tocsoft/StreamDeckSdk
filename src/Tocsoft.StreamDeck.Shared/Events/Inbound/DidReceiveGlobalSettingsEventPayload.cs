﻿using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Tocsoft.StreamDeck.Events
{
    public class DidReceiveGlobalSettingsEventPayload
    {
        public JObject Settings { get; set; }
    }
}
