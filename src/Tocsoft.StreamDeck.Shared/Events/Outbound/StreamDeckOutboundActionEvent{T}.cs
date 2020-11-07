namespace Tocsoft.StreamDeck.Events
{
    public abstract class StreamDeckOutboundActionEvent<TPayload> : StreamDeckOutboundActionEvent
    {
        public TPayload Payload { get; set; }
    }
}
