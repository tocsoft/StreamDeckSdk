using System.Threading.Tasks;
using Tocsoft.StreamDeck.Events;

namespace Tocsoft.StreamDeck
{
    public interface IActionManagerProvider
    {
        Task Initialize(ActionDefinition actionDefinition, StreamDeckInboundActionEvent triggeringEvent);

        IActionManager CurrrentActionManager { get; }
    }
}
