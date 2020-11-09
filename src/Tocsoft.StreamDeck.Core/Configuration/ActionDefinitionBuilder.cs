using System;

namespace Tocsoft.StreamDeck
{
    /// <summary>
    /// mutable class to allow configuraing an action before building the immutable version.
    /// </summary>
    public class ActionDefinitionBuilder
    {
        /// <summary>
        /// create builder with specific handler defined.
        /// </summary>
        /// <param name="handler"></param>
        public ActionDefinitionBuilder(Type handler)
        {
            Handler = handler;
            this.UUID = handler.FullName;
        }

        /// <summary>
        /// The class that will eventually be instansiated and manage the actinos of an action.
        /// </summary>
        public Type Handler { get; }

        /// <summary>
        /// the icon for the action
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// The actions name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// whether the actino is supported inside multi-actions
        /// </summary>
        public bool SupportedInMultiActions { get; set; } = true;

        /// <summary>
        /// is the action visible in the actions list.
        /// </summary>
        public bool VisibleInActionsList { get; set; } = true;

        /// <summary>
        /// the tooltip for the action
        /// </summary>
        public string Tooltip { get; set; }

        /// <summary>
        /// the globally unique id for the action.
        /// This will defautl to the full name of the handler including namespaces.
        /// </summary>
        public string UUID { get; set; }

        /// <summary>
        /// the configuration of the default state
        /// </summary>
        public ActionStateDefinition DefaultState { get; set; } = new ActionStateDefinition
        {

        };

        /// <summary>
        /// the configuration of the optional alternative state. 
        /// </summary>
        public ActionStateDefinition AlternativeState { get; set; } = null;

        /// <summary>
        /// builds an action definition.
        /// </summary>
        /// <returns></returns>
        public ActionDefinition Build()
        {
            return new ActionDefinition(this);
        }
    }

}
