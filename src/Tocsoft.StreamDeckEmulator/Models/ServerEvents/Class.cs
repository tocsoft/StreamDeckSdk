using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tocsoft.StreamDeck.Events;

namespace StreamDeckEmulator.Models.ServerEvents
{
    public class RegisterPluginEvent : RegisterEvent
    {
        static RegisterPluginEvent()
        {
            RegisterEventType<RegisterPluginEvent>();
        }
    }

    public class RegisterPropertyInspectorEvent : RegisterEvent
    {
        static RegisterPropertyInspectorEvent()
        {
            RegisterEventType<RegisterPropertyInspectorEvent>();
        }
    }

    public abstract class RegisterEvent : StreamDeckEvent
    {

        public string uuid { get; set; }
    }

}
