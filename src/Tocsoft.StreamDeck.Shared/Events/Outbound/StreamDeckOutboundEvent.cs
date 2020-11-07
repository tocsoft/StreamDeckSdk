namespace Tocsoft.StreamDeck.Events
{
    public abstract class StreamDeckOutboundEvent : StreamDeckEvent
    {

    }

    public abstract class StreamDeckOutboundEvent<T> : StreamDeckEvent
    {
        public T Payload { get; set; }
    }
}
