namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// Represents an outbound event a plugin can fire to the streamdeck
    /// </summary>
    public abstract class StreamDeckOutboundEvent : StreamDeckEvent
    {

    }

    /// <summary>
    /// Represents an outbound event a plugin can fire to the streamdeck with a strongly typed payload
    /// </summary>
    /// <typeparam name="T">payload type</typeparam>
    public abstract class StreamDeckOutboundEvent<T> : StreamDeckEvent
    {
        /// <summary>
        /// the event payload
        /// </summary>
        public T Payload { get; set; }
    }
}
