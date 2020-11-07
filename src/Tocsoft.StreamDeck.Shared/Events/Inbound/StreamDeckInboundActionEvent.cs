namespace Tocsoft.StreamDeck.Events
{
    public abstract class StreamDeckInboundActionEvent : StreamDeckInboundEvent
    {
        public string Context { get; set; }
        public string Device { get; set; }
        public string Action { get; set; }
    }

    public abstract class StreamDeckInboundActionEvent<T> : StreamDeckInboundActionEvent
    {
        public T Payload { get; set; }
    }
}
