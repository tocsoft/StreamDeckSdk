namespace Tocsoft.StreamDeck.Events
{
    public class ApplicationDidLaunchEvent : StreamDeckInboundEvent
    {
        public ApplicationLaunchPayload Payload { get; set; }
    }
}
