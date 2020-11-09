namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// payload for the title change event
    /// </summary>
    public class TitleParametersDidChangePayload : StreamDeckInboundActionCommonPayload
    {
        /// <summary>
        /// the updated title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// the rendering paramaters
        /// </summary>
        public TitleParamaters TitleParamaters { get; set; }
    }
}
