namespace Tocsoft.StreamDeck.Events
{
    public class StatePayload : StreamDeckInboundActionCommonPayload
    {
        public ActionState UserDesiredState { get; set; }
        public bool IsInMultiAction { get; set; }
    }
}
