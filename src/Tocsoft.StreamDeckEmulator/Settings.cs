using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamDeckEmulator
{
    public class Settings
    {
        public string StorageRoot { get; set; } = "%APPDATA%\\Tocsoft\\StreamDeckEmulator";

        // this could be a folder of a single plugin, of a manifest or of a folder 
        public string Plugin { get; set; }

        // the process id of the already running process
        // if this is set then we need to write out a blob 
        public string Pid { get; set; }

        public List<string> AdditionalPluginLaunchArguments { get; set; } = new List<string>();

        
    }
}
