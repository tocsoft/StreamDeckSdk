using System.Threading.Tasks;
using Tocsoft.StreamDeck.Events;

namespace Tocsoft.StreamDeck
{
    /// <summary>
    /// provides access to the current actino manager for the current action.
    /// </summary>
    public interface IActionManagerProvider
    {
        /// <summary>
        /// called during action startup to initailize the action handler and wire the current manager to the correct context id.
        /// </summary>
        /// <param name="actionDefinition"></param>
        /// <param name="triggeringEvent"></param>
        /// <returns></returns>
        Task Initialize(ActionDefinition actionDefinition, StreamDeckInboundActionEvent triggeringEvent);

        /// <summary>
        /// The current manager.
        /// </summary>
        IActionManager CurrrentActionManager { get; }
    }
}
