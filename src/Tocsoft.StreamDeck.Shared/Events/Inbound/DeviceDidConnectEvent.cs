namespace Tocsoft.StreamDeck.Events
{
    public class DeviceDidConnectEvent : StreamDeckInboundEvent
    {
        public DeviceInfo Device { get; set; }
    }
}
