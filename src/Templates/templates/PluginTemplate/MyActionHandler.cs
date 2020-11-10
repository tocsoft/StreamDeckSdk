using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Tocsoft.StreamDeck;

namespace PluginTemplate
{
    public class MyActionHandler
    {
        private readonly IActionManager<MyActionSettings> manager;

        public MyActionHandler(IActionManager<MyActionSettings> manager)
        {
            this.manager = manager;
        }

        public Task OnSettingsChanged(MyActionSettings settings)
        {
            return Task.CompletedTask;
        }

        public Task OnSendToPlugin(JToken settings)
        {
            return Task.CompletedTask;
        }

        public async Task OnKeyUpAsync()
        {
            await manager.ShowOkAsync();
        }

        public class MyActionSettings
        {
            public string Value1 { get; set; }
            public string Value2 { get; set; }
        }
    }
}
