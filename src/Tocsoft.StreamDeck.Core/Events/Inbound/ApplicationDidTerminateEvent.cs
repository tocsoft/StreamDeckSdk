namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// Event that is triggered when a monitored application terminat.
    /// </summary>
    public class ApplicationDidTerminateEvent : StreamDeckInboundEvent
    {
        /// <summary>
        /// Details about the application that terminated
        /// </summary>
        public ApplicationLaunchPayload Payload { get; set; }
    }
}
