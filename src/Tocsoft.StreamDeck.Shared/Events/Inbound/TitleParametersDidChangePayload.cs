namespace Tocsoft.StreamDeck.Events
{
    public class TitleParametersDidChangePayload : StreamDeckInboundActionCommonPayload
    {
        public string Title { get; set; }
        public TitleParamaters TitleParamaters { get; set; }
    }
}
