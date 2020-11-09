namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// details of a devices
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// the name of the devices as configured by the uses
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// the type of the devices
        /// </summary>
        public DeviceType Type { get; set; }
        
        /// <summary>
        /// the size of the device.
        /// </summary>
        public DeviceSize Size { get; set; }
    }
}
