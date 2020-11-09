namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// payload for setting an action image
    /// </summary>
    public class SetImagePayload
    {
        /// <summary>
        /// the image payload as a data uri
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// ther state to apply this image to
        /// </summary>
        public ActionState State { get; set; }

        /// <summary>
        /// the display target this image effects
        /// </summary>
        public SetValueTarget Target { get; set; }
    }
}
