namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// payload of the set state event
    /// </summary>
    public class StatePayload : StreamDeckInboundActionCommonPayload
    {
        /// <summary>
        /// the state to update the actino to
        /// </summary>
        public ActionState UserDesiredState { get; set; }

        /// <summary>
        /// whether this is in a multi-action or not
        /// </summary>
        public bool IsInMultiAction { get; set; }
    }
}
