namespace Tocsoft.StreamDeck.Events
{
    public abstract class StreamDeckOutboundActionEvent : StreamDeckOutboundEvent
    {
        public string Context { get; set; }
    }
}
