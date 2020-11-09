using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamDeckEmulator.Models
{

    public class Manifest
    {
        public string Author { get; set; }
        public string CodePathWin { get; set; }
        public string CodePath { get; set; }
        public string PropertyInspectorPath { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public int SDKVersion { get; set; }
        public OsManifest[] OS { get; set; }
        public SoftwareManifest Software { get; set; }
        public ActionManifest[] Actions { get; set; }
    }

    public class SoftwareManifest
    {
        public string MinimumVersion { get; set; }
    }

    public class OsManifest
    {
        public string Platform { get; set; }
        public string MinimumVersion { get; set; }
    }

    public class ActionManifest
    {
        public string Icon { get; set; }
        public string Name { get; set; }
        public string UUID { get; set; }
        public string PropertyInspectorPath { get; set; }
        public StateManifest[] States { get; set; }
    }

    public class StateManifest
    {
        public string Image { get; set; }
    }

}
