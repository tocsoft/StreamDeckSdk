using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Tocsoft.StreamDeck
{
    public class StreamDeckConfigurationBuilder
    {
        private List<ActionDefinitionBuilder> RegisteredActionHandlers = new List<ActionDefinitionBuilder>();
        private readonly IServiceCollection services;
        private readonly Assembly entryAssembly;

        public StreamDeckConfigurationBuilder(IServiceCollection services, Assembly entryAssembly)
        {

            this.services = services;
            this.entryAssembly = entryAssembly;
        }

        // these should be loaded out of the assembly attributes
        public string Author { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public string Icon { get; set; }

        public string CategoryName { get; set; }

        public string CategoryIcon { get; set; }

        public string Version { get; set; }

        public string MinimumStreamDeckVersion { get; set; } = "4.1";

        public string Url { get; set; }

        public List<OSVerison> OS { get; set; } = new List<OSVerison> {
            //new OSVerison{
            //    Platform = OSPlatform.Mac,
            //    MinimumVersion = "10.11"
            //},
            new OSVerison{
                Platform = OSPlatform.Windows,
                MinimumVersion = "10"
            },
        };

        public StreamDeckConfiguration Build()
        {
            Dictionary<string, ActionDefinition> actions = new Dictionary<string, ActionDefinition>();

            foreach (var action in this.RegisteredActionHandlers)
            {
                action.UUID = action.UUID ??
                              action.Handler.GetCustomAttribute<UUIDAttribute>()?.UUID ??
                              action.Handler.FullName;

                action.Name = action.Name ??
                              action.Handler.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ??
                              action.Handler.Name;

                action.Icon = action.Icon ??
                              action.Handler.GetCustomAttribute<ImageAttribute>()?.ImagePath ??
                              this.Icon;

                action.DefaultState = action.DefaultState ?? new ActionStateDefinition();
                FixState(action, action.DefaultState);
                if (action.AlternativeState != null)
                {
                    FixState(action, action.AlternativeState);
                }

                void FixState(ActionDefinitionBuilder builder, ActionStateDefinition state)
                {
                    state.Image = state.Image ?? action.Icon;
                }
                //lets fix up missing stuff 
            }

            this.Author = this.Author ?? entryAssembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
            this.Description = this.Description ?? entryAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
            this.Name = this.Name ?? entryAssembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
            this.Icon = this.Icon ?? entryAssembly.GetCustomAttribute<ImageAttribute>()?.ImagePath;
            this.Version = this.Version ?? entryAssembly.GetName().Version?.ToString() ?? entryAssembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version ?? "0.0.0";

            return new StreamDeckConfiguration()
            {
                Actions = this.RegisteredActionHandlers.Select(x => new ActionDefinition(x)).ToList(),
                Author = this.Author,
                CategoryIcon = this.CategoryIcon,
                CategoryName = this.CategoryName,
                Description = this.CategoryName,
                Icon = this.Icon,
                MinimumStreamDeckVersion = this.MinimumStreamDeckVersion,
                Name = this.Name,
                OS = this.OS,
                Url = this.Url,
                Version = this.Version
            };
        }

        public void AddAction<TAction>(Action<ActionDefinitionBuilder> configureAction = null)
            => this.AddAction(typeof(TAction), configureAction);

        public void AddAction(Type actionType, Action<ActionDefinitionBuilder> configureAction = null)
        {
            var definition = new ActionDefinitionBuilder(actionType);
            configureAction?.Invoke(definition);
            this.RegisteredActionHandlers.Add(definition);
            this.services.AddTransient(actionType);
        }
    }
}
