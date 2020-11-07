using System.Threading.Tasks;

namespace Tocsoft.StreamDeck.Sample
{
    public abstract class StreamDeckActionHandler
    {
        public virtual Task OnKeyDownAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnKeyUpAsync()
        {
            return Task.CompletedTask;
        }
    }
}
