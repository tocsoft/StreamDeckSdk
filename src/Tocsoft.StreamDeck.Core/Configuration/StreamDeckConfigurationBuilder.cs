using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Tocsoft.StreamDeck
{
    /// <summary>
    /// Builder for the streamdeck configuration
    /// </summary>
    public class StreamDeckConfigurationBuilder
    {
        private List<ActionDefinitionBuilder> RegisteredActionHandlers = new List<ActionDefinitionBuilder>();
        private readonly IServiceCollection services;
        private readonly Assembly entryAssembly;

        /// <summary>
        /// create a new builder
        /// </summary>
        /// <param name="services"></param>
        /// <param name="entryAssembly"></param>
        public StreamDeckConfigurationBuilder(IServiceCollection services, Assembly entryAssembly)
        {

            this.services = services;
            this.entryAssembly = entryAssembly;
        }

        /// <summary>
        /// the name of the author
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// the plgin description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// plgin name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// icon
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// category to group the plugin into
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// the icon to give the category
        /// </summary>
        public string CategoryIcon { get; set; }

        /// <summary>
        /// plugin version
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// min version of the host streamdeck software
        /// </summary>
        public string MinimumStreamDeckVersion { get; set; } = "4.1";

        /// <summary>
        /// url for more details about the plugin
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// the set of os/versions that can execute this plugin
        /// </summary>
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

        /// <summary>
        /// the set of registerd actions
        /// </summary>
        public IReadOnlyList<ActionDefinitionBuilder> Actions => this.RegisteredActionHandlers;

        /// <summary>
        /// build a final immutable configuration from this builder
        /// </summary>
        /// <returns></returns>
        public IStreamDeckConfiguration Build()
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

            return new StreamDeckConfiguration(this);
        }

        /// <summary>
        /// add a new action handler and register it in the service collection.
        /// </summary>
        /// <typeparam name="TAction"></typeparam>
        /// <param name="configureAction"></param>
        public void AddAction<TAction>(Action<ActionDefinitionBuilder> configureAction = null)
            => this.AddAction(typeof(TAction), configureAction);

        /// <summary>
        /// register a new action handler
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="configureAction"></param>
        public void AddAction(Type actionType, Action<ActionDefinitionBuilder> configureAction = null)
        {
            var definition = new ActionDefinitionBuilder(actionType);
            configureAction?.Invoke(definition);
            this.RegisteredActionHandlers.Add(definition);
            this.services.AddTransient(actionType);
        }
    }
}
