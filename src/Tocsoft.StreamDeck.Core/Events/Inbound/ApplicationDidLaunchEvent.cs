namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// Event that is triggered when a monitored application launches.
    /// </summary>
    public class ApplicationDidLaunchEvent : StreamDeckInboundEvent
    {
        /// <summary>
        /// Details about the application that launched
        /// </summary>
        public ApplicationLaunchPayload Payload { get; set; }
    }
}
