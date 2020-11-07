using PowerArgs;

namespace Tocsoft.StreamDeck
{
    [AllowUnexpectedArgs]
    public class CommandlineArguments
    {
        public int? Port { get; set; }

        public string pluginUUID { get; set; }

        public string registerEvent { get; set; }

        public string info { get; set; }

        public bool Break { get; set; }

        [ArgShortcut("export-config")]
        public string ManifestExportPath { get; set; }

        [ArgShortcut("export-config-sdk")]
        public string ManifestExportSdkPath { get; set; }

        public bool TryGetStartupArguments(out StartupArguments startupArguments)
        {
            if (!Port.HasValue)
            {
                startupArguments = null;
                return false;
            }

            startupArguments = new StartupArguments()
            {
                Info = this.info,
                PluginUUID = this.pluginUUID,
                Port = this.Port,
                RegisterEvent = this.registerEvent
            };
            return true;
        }
        public bool TryGetExportConfigArguments(out ExportConfigArguments arguments)
        {
            if (string.IsNullOrEmpty(ManifestExportPath))
            {
                arguments = null;
                return false;
            }

            arguments = new ExportConfigArguments()
            {
                ManifestExportPath = this.ManifestExportPath,
                SdkPath = this.ManifestExportSdkPath,
            };
            return true;
        }
    }
}
