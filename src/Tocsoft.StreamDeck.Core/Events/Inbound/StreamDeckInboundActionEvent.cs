namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// Base class for inbound action sepcific events
    /// </summary>
    public abstract class StreamDeckInboundActionEvent : StreamDeckInboundEvent
    {
        /// <summary>
        /// the opaque context id for the action instance.
        /// </summary>
        public string Context { get; set; }
        
        /// <summary>
        /// the id for the specific device
        /// </summary>
        public string Device { get; set; }

        /// <summary>
        /// the action uuid that this event is for.
        /// </summary>
        public string Action { get; set; }
    }

    /// <summary>
    /// base inbound action event with a strongly typed payload
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class StreamDeckInboundActionEvent<T> : StreamDeckInboundActionEvent
    {
        /// <summary>
        /// the payload
        /// </summary>
        public T Payload { get; set; }
    }
}
