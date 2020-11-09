namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// base class of action specific output events
    /// </summary>
    public abstract class StreamDeckOutboundActionEvent : StreamDeckOutboundEvent
    {
        /// <summary>
        /// the opaque context id for the action the event relates to.
        /// </summary>
        public string Context { get; set; }
    }
}
