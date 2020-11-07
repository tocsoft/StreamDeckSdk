namespace Tocsoft.StreamDeck.Events
{
    public class SetImagePayload
    {
        // TODO add helpers around this, or even custom serializers!!!
        public string Image { get; set; }
        public ActionState State { get; set; }
        public SetValueTarget Target { get; set; }
    }
}
