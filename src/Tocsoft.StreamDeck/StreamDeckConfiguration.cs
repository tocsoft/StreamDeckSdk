using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Tocsoft.StreamDeck
{

    public class StreamDeckConfiguration
    {
        public StreamDeckConfiguration()
        {
        }

        // these shoudl be loaded out of the assembly attributes
        public string Author { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public string Icon { get; set; }

        public string CategoryName { get; set; }

        public string CategoryIcon { get; set; }

        public string Version { get; set; }

        public string MinimumStreamDeckVersion { get; set; } = "4.1";
        public string PropertyInspector { get; set; }

        public string Url { get; set; }

        public IReadOnlyList<ActionDefinition> Actions { get; set; } = new List<ActionDefinition>();

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

    }

    public class OSVerison
    {
        public OSPlatform Platform { get; set; }

        public string MinimumVersion { get; set; }
    }

    public enum OSPlatform
    {
        Mac,
        Windows
    }

    public class ActionDefinitionBuilder
    {
        public ActionDefinitionBuilder(Type handler)
        {
            Handler = handler;
        }

        public Type Handler { get; }

        public string Icon { get; set; }

        public string Name { get; set; }

        public bool SupportedInMultiActions { get; set; } = true;

        public bool VisibleInActionsList { get; set; } = true;

        public string Tooltip { get; set; }

        public string UUID { get; set; }

        public ActionStateDefinition DefaultState { get; set; } = new ActionStateDefinition
        {

        };

        public ActionStateDefinition AlternativeState { get; set; } = null;
    }

    public class ActionDefinition
    {
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

        public Type Handler { get; }

        public string Icon { get; }

        public string Name { get; }

        public bool SupportedInMultiActions { get; }

        public bool VisibleInActionsList { get; }

        public string Tooltip { get; }

        public string UUID { get; }

        public IReadOnlyList<ActionStateDefinition> States { get; }
        public string PropertyInspector { get; set; }
    }

    public class ActionStateDefinition : TitleParamaters
    {
        public string Image { get; set; }

        public string MultiActionImage { get; set; }

        public string Name { get; set; }
    }

}
