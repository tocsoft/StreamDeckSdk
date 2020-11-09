namespace Tocsoft.StreamDeck
{
    /// <summary>
    /// A combination of an OS an version number
    /// </summary>
    public class OSVerison
    {
        /// <summary>
        /// the os platform
        /// </summary>
        public OSPlatform Platform { get; set; }

        /// <summary>
        /// the minimum version number that the OS must be for the plugin to work
        /// </summary>
        public string MinimumVersion { get; set; }
    }

}
