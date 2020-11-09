namespace Tocsoft.StreamDeck.Events
{
    /// <summary>
    /// update the state of an action
    /// </summary>
    public class SetStateEvent : StreamDeckOutboundActionEvent<SetStateEvent.SetStateOutput>
    {
        /// <summary>
        /// the payload
        /// </summary>
        public class SetStateOutput
        {
            /// <summary>
            /// the state to set the actino to
            /// </summary>
            public ActionState State { get; set; }
        }
    }
}
