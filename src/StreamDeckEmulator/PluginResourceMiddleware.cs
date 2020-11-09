using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using StreamDeckEmulator.Services;

namespace StreamDeckEmulator
{
    public class PluginResourceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly PluginManager pluginManager;
        private readonly FileExtensionContentTypeProvider fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();

        public PluginResourceMiddleware(RequestDelegate next, PluginManager pluginManager)
        {
            _next = next;
            this.pluginManager = pluginManager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/_resources", out var remaining))
            {
                var parts = remaining.Value.Split(new[] { '/' }, 3);
                var pluginid = parts[1];
                var path = parts[2];
                var plugin = pluginManager.GetPlugin(pluginid);

                path = Path.Combine(plugin.RootFolder, path);

                if (File.Exists(path))
                {
                    if (fileExtensionContentTypeProvider.TryGetContentType(path, out var contentPath))
                    {
                        context.Response.ContentType = contentPath;
                    }

                    await context.Response.SendFileAsync(new PluginFile(path));
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


        public class PluginFile : IFileInfo
        {
            private readonly FileInfo info;


            public PluginFile(string path)
            {
                this.info = new FileInfo(path);

            }

            public bool Exists => this.info.Exists;

            public long Length => this.info.Length;

            public string PhysicalPath => this.info.FullName;
            public string Name => this.info.Name;

            public DateTimeOffset LastModified => this.info.LastWriteTime;

            public bool IsDirectory => false;

            public Stream CreateReadStream()
                => this.info.OpenRead();
        }
    }

}
