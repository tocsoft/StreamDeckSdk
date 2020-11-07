namespace Tocsoft.StreamDeck.Events
{
    public class DidReceiveGlobalSettingsEvent : StreamDeckInboundEvent
    {
        public DidReceiveGlobalSettingsEventPayload Settings { get; set; }
    }
}
