using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using StreamDeckEmulator.Services;

namespace StreamDeckEmulator
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly PluginManager pluginManager;

        public WebSocketMiddleware(RequestDelegate next, PluginManager pluginManager)
        {
            _next = next;
            this.pluginManager = pluginManager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
                    {
                        await pluginManager.Connect(context, webSocket);
                    }
                }
                else
                {
                    await _next(context);
                }
            }
            else
            {
                await _next(context);
            }
        }
    }

}
