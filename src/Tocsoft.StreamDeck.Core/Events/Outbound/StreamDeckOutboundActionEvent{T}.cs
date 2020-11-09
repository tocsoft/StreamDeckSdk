namespace Tocsoft.StreamDeck.Events
{

    /// <summary>
    /// strongly action sepcific event
    /// </summary>
    public abstract class StreamDeckOutboundActionEvent<TPayload> : StreamDeckOutboundActionEvent
    {
        /// <summary>
        /// the payload for the action.
        /// </summary>
        public TPayload Payload { get; set; }
    }
}
