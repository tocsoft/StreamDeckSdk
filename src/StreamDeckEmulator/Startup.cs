using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Hosting;
using StreamDeckEmulator.Data;
using StreamDeckEmulator.Services;

namespace StreamDeckEmulator
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<Settings>(Configuration);

            services.AddSingleton<PluginManager>();
            services.AddControllers();
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<IConsole, ConsoleWrapper>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseWebSockets();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseMiddleware<WebSocketMiddleware>();

            app.UseMiddleware<PluginResourceMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }

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

    public class PluginResourceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly PluginManager pluginManager;
        private readonly FileExtensionContentTypeProvider fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();
        private readonly IFileProvider fileProvider;

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
