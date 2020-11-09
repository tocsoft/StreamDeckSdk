namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// the payload for setting an actions title.
    /// </summary>
    public class SetTitlePayload
    {
        /// <summary>
        /// the title to set
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// the state to target
        /// </summary>
        public ActionState State { get; set; }

        /// <summary>
        /// the device type to target
        /// </summary>
        public SetValueTarget Target { get; set; }
    }
}
