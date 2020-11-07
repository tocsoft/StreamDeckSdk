namespace Tocsoft.StreamDeck.Events
{
    public class SetTitlePayload
    {
        public string Title { get; set; }
        public ActionState State { get; set; }
        public SetValueTarget Target { get; set; }
    }
}
