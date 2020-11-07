namespace Tocsoft.StreamDeck.Events
{
    public class SetStateEvent : StreamDeckOutboundActionEvent<SetStateEvent.SetStateOutput>
    {
        public class SetStateOutput
        {
            public ActionState State { get; set; }
        }
    }
}
