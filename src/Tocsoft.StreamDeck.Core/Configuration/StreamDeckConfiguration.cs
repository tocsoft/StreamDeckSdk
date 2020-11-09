using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Tocsoft.StreamDeck
{
    internal class StreamDeckConfiguration : IStreamDeckConfiguration
    {
        /// <summary>
        /// Creates a configuratino from the builder.
        /// </summary>
        /// <param name="builder"></param>
        public StreamDeckConfiguration(StreamDeckConfigurationBuilder builder)
        {
            this.Actions = builder.Actions.Select(x => new ActionDefinition(x)).ToList();
            this.Author = builder.Author;
            this.CategoryIcon = builder.CategoryIcon;
            this.CategoryName = builder.CategoryName;
            this.Description = builder.CategoryName;
            this.Icon = builder.Icon;
            this.MinimumStreamDeckVersion = builder.MinimumStreamDeckVersion;
            this.Name = builder.Name;
            this.OS = builder.OS;
            this.Url = builder.Url;
            this.Version = builder.Version;
        }

        public string Author { get; }

        public string Description { get; }

        public string Name { get; }

        public string Icon { get; }

        public string CategoryName { get; }

        public string CategoryIcon { get; }

        public string Version { get; }
        public string MinimumStreamDeckVersion { get; } = "4.1";
        public string PropertyInspector { get; }
        public string Url { get; }
        public IReadOnlyList<ActionDefinition> Actions { get; } = new List<ActionDefinition>();
        public IReadOnlyList<OSVerison> OS { get; } = new List<OSVerison> {
            //new OSVerison{
            //    Platform = OSPlatform.Mac,
            //    MinimumVersion = "10.11"
            //},
            new OSVerison{
                Platform = OSPlatform.Windows,
                MinimumVersion = "10"
            },
        };

    }

}
