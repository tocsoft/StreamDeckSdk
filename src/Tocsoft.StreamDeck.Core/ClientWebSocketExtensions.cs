using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tocsoft.StreamDeck
{
    internal static class ClientWebSocketExtensions
    {
        public static Task SendJsonAsync(this WebSocket client, object message, JsonSerializer serializer = null, CancellationToken cancellationToken = default)
        {
            serializer = serializer ?? JsonSerializer.CreateDefault();

            using (var tw = new StringWriter())
            {
                serializer.Serialize(tw, message);
                var data = Encoding.UTF8.GetBytes(tw.ToString());
                //var data = JsonSerializer.SerializeToUtf8Bytes(message, message.GetType(), serializer);
                return client.SendAsync(data, WebSocketMessageType.Text, true, cancellationToken);
            }
        }

        public static async Task<(T message, bool closed)> ReceiveJsonAsync<T>(this WebSocket client, JsonSerializer serializer = null, CancellationToken cancellationToken = default)
        {
            var (json, closed) = await client.ReceiveTextAsync(cancellationToken);

            serializer = serializer ?? JsonSerializer.CreateDefault();
            using (var reader = new StringReader(json))
            {
                var data = serializer.Deserialize(reader, typeof(T));
                return ((T)data, closed);
            }
        }

        public static async Task<(string message, bool closed)> ReceiveTextAsync(this WebSocket client, CancellationToken cancellationToken = default)
        {
            // this is wrong but can't be botherd to figure an alternative right now!!
            string json = "";
            var buffer = new Memory<byte>(new byte[1024 * 4]);
            var result = await client.ReceiveAsync(buffer, cancellationToken);

            while (true)
            {
                var slice = buffer.Slice(0, result.Count);
                json += Encoding.UTF8.GetString(slice.ToArray());
                if (result.EndOfMessage)
                {
                    break;
                }
                result = await client.ReceiveAsync(buffer, cancellationToken);
            }


            return (json, result.MessageType == WebSocketMessageType.Close);
        }
    }
}
