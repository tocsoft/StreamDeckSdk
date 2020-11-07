namespace Tocsoft.StreamDeck.Events
{
    public class ApplicationDidTerminateEvent : StreamDeckInboundEvent
    {
        public ApplicationLaunchPayload Payload { get; set; }
    }
}
