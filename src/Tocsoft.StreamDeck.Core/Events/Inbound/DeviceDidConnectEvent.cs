namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// Fired when a device connects
    /// </summary>
    public class DeviceDidConnectEvent : StreamDeckInboundEvent
    {
        /// <summary>
        /// the details of the connected device.
        /// </summary>
        public DeviceInfo DeviceInfo { get; set; }
    }
}
