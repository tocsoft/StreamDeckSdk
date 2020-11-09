using System.Collections.Generic;

namespace Tocsoft.StreamDeck
{
    /// <summary>
    /// Fully configured and locked down StreamDeck configuration
    /// </summary>
    public interface IStreamDeckConfiguration
    {
        /// <summary>
        /// The final list of configured actions.
        /// </summary>
        IReadOnlyList<ActionDefinition> Actions { get; }
        /// <summary>
        /// The author as at be defined in the final plugin manifest
        /// </summary>
        string Author { get; }
        /// <summary>
        /// An optional category icon to apply to the <see cref="CategoryName"/>
        /// </summary>
        string CategoryIcon { get; }
        /// <summary>
        /// An optional category name to group the actinos into in the UI on the right hand side.
        /// </summary>
        string CategoryName { get; }
        /// <summary>
        /// The description of the plugin
        /// </summary>
        string Description { get; }
        /// <summary>
        /// The icon to use for the plgin in the full list of actions.
        /// </summary>
        string Icon { get; }

        /// <summary>
        /// Minimum version os the streamdeck software required.
        /// </summary>
        string MinimumStreamDeckVersion { get; }
        /// <summary>
        /// The name of the plugin
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The list of compatable OS's and versions.
        /// </summary>
        IReadOnlyList<OSVerison> OS { get; }

        /// <summary>
        /// Path to a the optional default peoperty inspector that can shared across all actions.
        /// </summary>
        string PropertyInspector { get; }

        /// <summary>
        /// Url to direct users too for more details on the plugin.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// The plugin version number.
        /// </summary>
        string Version { get; }
    }
}