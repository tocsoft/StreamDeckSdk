using System;
using System.Threading.Tasks;

namespace Tocsoft.StreamDeck
{
    public interface IPluginManager
    {
        Task OpenUrlAsync(string url);

        Task LogMessageAsync(string message);
    }

    public interface IPluginManager<TGlobalSettings>
    {
        TGlobalSettings CurrentSettings { get; }

        void OnSettingChange(Action<TGlobalSettings> action);
    }
}
