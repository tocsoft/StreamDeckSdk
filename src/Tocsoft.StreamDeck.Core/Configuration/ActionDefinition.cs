using System;
using System.Collections.Generic;
using System.Linq;

namespace Tocsoft.StreamDeck
{
    /// <summary>
    /// The definition of a single action.
    /// </summary>
    public class ActionDefinition
    {
        /// <summary>
        /// Create a definition from a builder.
        /// </summary>
        /// <param name="builder"></param>
        public ActionDefinition(ActionDefinitionBuilder builder)
        {
            // fallback to attributes and clas names etc
            this.Icon = builder.Icon;
            this.Name = builder.Name;
            this.SupportedInMultiActions = builder.SupportedInMultiActions;
            this.VisibleInActionsList = builder.VisibleInActionsList;
            this.Tooltip = builder.Tooltip;
            this.UUID = builder.UUID?.ToLower();
            this.Handler = builder.Handler;
            this.States = new[] {
                        builder.DefaultState,
                        builder.AlternativeState
                    }.Where(s => s != null).ToList();
        }

        /// <summary>
        /// The type defined as handling this action.
        /// </summary>
        public Type Handler { get; }

        /// <summary>
        /// The actions icon
        /// </summary>
        public string Icon { get; }

        /// <summary>
        /// Name of the action
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Whether the action is supported in multi-actions.
        /// </summary>
        public bool SupportedInMultiActions { get; }

        /// <summary>
        /// Whether the actino shoudl sho in the action list.
        /// </summary>
        public bool VisibleInActionsList { get; }

        /// <summary>
        /// The action tooltip.
        /// </summary>
        public string Tooltip { get; }

        /// <summary>
        /// Globally uniquie ID for this action
        /// </summary>
        public string UUID { get; }

        /// <summary>
        /// Collection of the various states this action supports
        /// </summary>
        public IReadOnlyList<ActionStateDefinition> States { get; }
        
        /// <summary>
        /// Path to the actino specific property inspector
        /// </summary>
        public string PropertyInspector { get; set; }
    }

}
