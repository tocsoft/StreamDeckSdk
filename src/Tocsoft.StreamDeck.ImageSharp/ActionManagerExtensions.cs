using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.Threading.Tasks;

namespace Tocsoft.StreamDeck
{
    public static class ActionManagerExtensions
    {
        public static Task SetImageAsync(this Tocsoft.StreamDeck.IActionManager actionManager, Image image)
        {
            return actionManager.SetImageAsync(image.ToBase64String(PngFormat.Instance));
        }
    }
}
