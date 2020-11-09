namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// Global setting updated
    /// </summary>
    public class DidReceiveGlobalSettingsEvent : StreamDeckInboundEvent
    {
        /// <summary>
        /// Details containing the updated settings.
        /// </summary>
        public DidReceiveGlobalSettingsEventPayload Settings { get; set; }
    }
}
