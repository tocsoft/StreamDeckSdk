using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// base class for all streamdeck events
    /// </summary>
    public abstract class StreamDeckEvent
    {
        /// <summary>
        /// json serilizer configured to match the required conventions of a streamdeck
        /// </summary>
        public static JsonSerializer EventSerilizer { get; }
        
        /// <summary>
        /// Provides easy access to the serializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serialize<T>(T obj)
        {
            using (var tw = new StringWriter())
            {
                EventSerilizer.Serialize(tw, obj);
                return tw.ToString();
            }
        }
        /// <summary>
        /// provides easy access to the deseilizer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string json)
        {
            using (var tw = new StringReader(json))
            {
                var obj = (T)EventSerilizer.Deserialize(tw, typeof(T));
                return obj;
            }
        }

        private static Dictionary<string, Type> events = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        static StreamDeckEvent()
        {
            EventSerilizer = new JsonSerializer()
            {
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy
                    {
                        OverrideSpecifiedNames = true,
                    }
                }
            };

            var defultEvents = typeof(StreamDeckEvent).Assembly
                .GetTypes()
                .Where(x => !x.IsAbstract)
                .Where(x => x.IsSubclassOf(typeof(Events.StreamDeckEvent)))
                .ToList();
            foreach (var e in defultEvents)
            {
                RegisterEventType(e);
            }
        }

        private static void RegisterEventType(Type t)
        {
            var n = t.Name;
            if (n.EndsWith("Event"))
            {
                n = n.Substring(0, n.Length - "Event".Length);
            }
            events.Add(n, t);
        }

        internal static void RegisterEventType<T>() where T : StreamDeckEvent
            => RegisterEventType(typeof(T));

        internal static bool TryParse(JObject jsonDocument, out StreamDeckEvent streamDeckEvent, out string eventType)
        {
            eventType = jsonDocument.GetValue("event")?.Value<string>();
            if (events.TryGetValue(eventType, out var targetType))
            {
                streamDeckEvent = (StreamDeckEvent)jsonDocument.ToObject(targetType, EventSerilizer);
                return streamDeckEvent != null;
            }

            streamDeckEvent = null;
            return false;
        }

        private string DefaultEventName()
        {
            var n = this.GetType().Name;
            if (n.EndsWith("Event"))
            {
                n = char.ToLower(n[0]) + n.Substring(1, n.Length - "Event".Length - 1);
            }

            return n;
        }

        private bool eventNameSet = false;
        private string eventName = null;
        
        /// <summary>
        /// the name of the event.
        /// </summary>
        public string Event
        {
            get
            {
                if (!eventNameSet)
                {
                    return eventName = DefaultEventName();
                }
                return eventName;
            }
            set
            {
                eventNameSet = true;
                eventName = value;
            }
        }
    }
}
