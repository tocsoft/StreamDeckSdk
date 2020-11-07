using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Tocsoft.StreamDeck.Events;

namespace Tocsoft.StreamDeck
{
    public abstract class StreamDeckActionHandler
    {
        //public void Initialize(StreamDeckActionManager streamDeckActionManager)
        //{
        //}
    }


    internal class StreamDeckActionManagerProvider : IActionManager, IActionManagerProvider, IDisposable
    {
        private EventManager eventManager;
        private readonly IStreamDeckConnection connection;
        private readonly IServiceProvider serviceProvider;

        public string ContextId { get; private set; }
        public ActionDefinition ActionDefinition { get; private set; }

        private IDisposable listener;

        public StreamDeckActionManagerProvider(IStreamDeckConnection connection, IServiceProvider serviceProvider, EventManager eventManager)
        {
            this.eventManager = eventManager; ;
            this.connection = connection;
            this.serviceProvider = serviceProvider;
        }

        private void Bind(object handler)
        {
            // using the actino definition type lets bind methods to event callbacks with async support!!!

            // lets special case settings changes!!!

            // all others are evetn handlers jsut passing the event type right through

            // async postfix is optional

            var mapping = typeof(StreamDeckEvent).Assembly
             .GetTypes()
             .Where(x => !x.IsAbstract)
             .Where(x => x.IsSubclassOf(typeof(Events.StreamDeckEvent)))
             .Select(x =>
             {
                 var n = x.Name;
                 if (n.EndsWith("Event"))
                 {
                     n = n.Substring(0, n.Length - "Event".Length);
                 }
                 return (name: "On" + n, eventType: x);
             });

            var methods = ActionDefinition.Handler.GetMethods();

            var onSettingsChangeMethodNames = new[] { "OnSettingsChanged", "OnSettingsChangedAsync" };

            var settingsChangedMethod = methods.Where(x => onSettingsChangeMethodNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase)).ToList();

            if (settingsChangedMethod.Any())
            {
                this.OnSettingsChanged(async (s) =>
                {
                    foreach (var m in settingsChangedMethod)
                    {
                        var param = m.GetParameters().Single();

                        object objectData = s;
                        if (objectData != null && !typeof(JObject).IsAssignableFrom(param.ParameterType))
                        {
                            objectData = s.ToObject(param.ParameterType, StreamDeckEvent.EventSerilizer);
                        }

                        var result = m.Invoke(handler, new[] { objectData });

                        if (typeof(Task).IsAssignableFrom(m.ReturnType))
                        {
                            await (Task)result;
                        }
                    }
                });
            }

            var onSendToPluginMethodNames = new[] { "OnSendToPlugin", "OnSendToPluginAsync" };

            var onSendToPluginMethods = methods.Where(x => onSendToPluginMethodNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase)).ToList();

            if (onSendToPluginMethods.Any())
            {
                this.eventManager.Register<SendToPluginEvent>(async (s) =>
               {
                   foreach (var m in onSendToPluginMethods)
                   {
                       var param = m.GetParameters().Single();
                       object objectData = s.Payload;
                       if (objectData != null && !typeof(JToken).IsAssignableFrom(param.ParameterType))
                       {
                           objectData = s.Payload.ToObject(param.ParameterType, StreamDeckEvent.EventSerilizer);
                       }

                       var result = m.Invoke(handler, new[] { objectData });

                       if (typeof(Task).IsAssignableFrom(m.ReturnType))
                       {
                           await (Task)result;
                       }
                   }
               });
            }


            // add a copy with the Async Postfix
            var fullMap = mapping.Select(x => (name: x.name + "Async", x.eventType)).Concat(mapping);

            foreach (var m in fullMap)
            {
                var method = methods.SingleOrDefault(x => x.Name == m.name);
                if (method == null)
                {
                    continue;
                }

                var paramaters = method.GetParameters();
                if (paramaters.Length != 0)
                {
                    if (paramaters.Length != 1)
                    {
                        continue;
                    }
                    if (!paramaters[0].ParameterType.IsAssignableFrom(m.eventType))
                    {
                        continue;
                    }

                    if (typeof(Task).IsAssignableFrom(method.ReturnType))
                    {
                        eventManager.Register(m.eventType, evnt =>
                        {
                            var t = (Task)method.Invoke(handler, new[] { evnt });
                            return t;
                        });
                    }
                    else
                    {
                        eventManager.Register(m.eventType, (evnt) =>
                        {
                            method.Invoke(handler, new[] { evnt });

                            return Task.CompletedTask;
                        });
                    }
                }
                else
                {
                    if (typeof(Task).IsAssignableFrom(method.ReturnType))
                    {
                        eventManager.Register(m.eventType, evnt =>
                        {
                            var t = (Task)method.Invoke(handler, new object[0]);
                            return t;
                        });
                    }
                    else
                    {
                        eventManager.Register(m.eventType, (evnt) =>
                        {
                            method.Invoke(handler, new object[0]);

                            return Task.CompletedTask;
                        });
                    }
                }
            }
        }

        public async Task Initialize(ActionDefinition actionDefinition, StreamDeckInboundActionEvent triggeringEvent)
        {
            if (this.ContextId != null || this.ActionDefinition != null)
            {
                throw new InvalidOperationException($"{nameof(Initialize)} may only be called once");
            }

            this.ContextId = triggeringEvent.Context;
            this.ActionDefinition = actionDefinition;

            this.listener = connection.Listen<StreamDeck.Events.StreamDeckInboundActionEvent>(e =>
            {
                if (e.Context == this.ContextId && e.Action.Equals(this.ActionDefinition.UUID, StringComparison.OrdinalIgnoreCase))
                {
                    return ProcessEventsAsync(e);
                }

                return Task.CompletedTask;
            });

            var handler = serviceProvider.GetRequiredService(this.ActionDefinition.Handler);

            Bind(handler);

            await ProcessEventsAsync(triggeringEvent);
        }

        private Task ProcessEventsAsync(StreamDeckInboundActionEvent evnt)
            => eventManager.TriggerEvent(evnt);

        public JObject CurrentSettings { get; private set; }

        public ActionState CurrentState { get; private set; }

        public IActionManager CurrrentActionManager => this;

        public void OnSettingsChanged(Func<JObject, Task> action)
        {
            this.eventManager.Register<DidReceiveSettingsEvent>(c =>
            {
                CurrentState = c.Payload.State;
                CurrentSettings = c.Payload.Settings;
                return action(c.Payload.Settings);
            });
        }

        public Task SetImageAsync(string image)
        {
            if (!image.StartsWith("data:"))
            {
                image = $"data:image/png;base64," + Convert.ToBase64String(File.ReadAllBytes(image));
            }

            return this.connection.SendEvent(new Events.SetImageEvent()
            {
                Context = this.ContextId,
                Payload = new SetImagePayload
                {
                    Image = image,
                }
            });
        }
        public Task SendToPropertyInspector<T>(T payload)
        {
            return this.connection.SendEvent(new Events.SendToPropertyInspectorEvent()
            {
                Context = this.ContextId,
                Payload = payload == null ? null : JToken.FromObject(payload, StreamDeckEvent.EventSerilizer)
            });
        }

        public Task ShowAlertAsync()
         => this.connection.SendEvent(new Events.ShowAlertEvent()
         {
             Context = this.ContextId
         });

        public Task ShowOkAsync()
         => this.connection.SendEvent(new Events.ShowOkEvent()
         {
             Context = this.ContextId
         });

        public Task SetStateAsync(ActionState state)
        => this.connection.SendEvent(new Events.SetStateEvent()
        {
            Context = this.ContextId,
            Payload = new SetStateEvent.SetStateOutput
            {
                State = state
            }
        });

        public void OnStateChange(Action<ActionState> action)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            this.listener?.Dispose();
        }
    }
}
