using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddOptions<Settings>()
                .Bind(Configuration)
                .Configure(s =>
                {
                    if (string.IsNullOrEmpty(s.Plugin))
                    {
                        // if windows 
                        s.Plugin = "%APPDATA%\\Elgato\\StreamDeck\\Plugins";
                    }
                });

            services.AddSingleton<PluginManager>();
            services.AddControllers();
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<IConsole, ConsoleWrapper>();

            services.AddSingleton<CircuitHandler, CircuitHandlerR>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            lifetime.ApplicationStarted.Register(() =>
            {
                var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
                var address = serverAddressesFeature.Addresses.SingleOrDefault();
                var port = new Uri(address).Port;
                Process.Start(new ProcessStartInfo($"http://localhost:{port}") { UseShellExecute = true });
            });

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
    public class CircuitHandlerR : CircuitHandler
    {
        public CircuitHandlerR(IHostApplicationLifetime lifetime)
        {
            this.lifetime = lifetime;
        }
        int refCoutner = 0;
        private readonly IHostApplicationLifetime lifetime;

        public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref refCoutner);

            return base.OnCircuitOpenedAsync(circuit, cancellationToken);
        }
        public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            if (Interlocked.Decrement(ref refCoutner) <= 0)
            {
                lifetime.StopApplication();
            }
            return base.OnCircuitOpenedAsync(circuit, cancellationToken);
        }
    }
}
