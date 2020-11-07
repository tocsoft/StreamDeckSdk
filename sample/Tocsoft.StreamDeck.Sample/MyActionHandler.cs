using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading.Tasks;

namespace Tocsoft.StreamDeck.Sample
{
    // if it inherits the base class we will wire up the action manager by calling 'Register'

    //[Image("Images/actionImage")]
    public class MyActionHandler
    {
        private readonly IActionManager<MyActionSettings> manager;

        public MyActionHandler(IActionManager<MyActionSettings> manager)
        {
            this.manager = manager;
            //var method = typeof(MyActionHandler).GetMethod("OnSettingsChanged");
            //manager.OnSettingsChanged(s =>
            //{
            //    var result = method.Invoke(this, new[] { s });
            //    if (result is Task t)
            //    {
            //        return t;
            //    }

            //    return Task.CompletedTask;
            //});
        }

        // auto register these events where we can
        public Task OnSettingsChanged(MyActionSettings settings)
        {
            return Task.CompletedTask;
        }
        public Task OnSendToPlugin(JToken settings)
        {
            return Task.CompletedTask;
        }

        int idx = -1;
        Color[] colors = new[] {
            Color.Red,
            Color.Plum,
            Color.Pink,
            Color.Green
        };

        bool lastShowOk = true;

        public async Task OnKeyUpAsync()
        {
            idx++;
            idx %= colors.Length;
            using (var img = new SixLabors.ImageSharp.Image<Rgba32>(72, 72, colors[idx]))
            {
                await manager.SetImageAsync(img);
            }
            if (lastShowOk)
            {
                await manager.ShowOkAsync();
            }
            else
            {
                await manager.ShowAlertAsync();
            }

            lastShowOk = !lastShowOk;
        }

        public class MyActionSettings
        {
            public string Value1 { get; set; }
            public string Value2 { get; set; }
        }
    }
}
