using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PowerArgs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Tocsoft.StreamDeck
{
    public static class IHostBuilderExtensions
    {

        public static IHostBuilder ConfigureStreamDeck(this IHostBuilder builder, string[] args, Action<StreamDeckConfigurationBuilder> configure)
        {
            var callingAssembly = Assembly.GetCallingAssembly();

            // switch between local development host that provides an embedded web service, api and application to debug/test application against
            // add option to also detect build time helpers calls to do things like expost manifests etc early

            var parsedArguments = Args.Parse<CommandlineArguments>(args);

            if (parsedArguments.Break == true && !Debugger.IsAttached)
            {
                Debugger.Launch();
            }

            var runInMemory = !parsedArguments.TryGetStartupArguments(out var registrationArgs);
            var exportConfig = parsedArguments.TryGetExportConfigArguments(out var exportConfigSettings);

            if (exportConfig)
            {
                builder.ConfigureLogging(c => c.ClearProviders());
            }


            // we detect is we want to export manifest if we do we add the manifest host so when run executes it write out the manifest file and then exits the process
            // lest just scan for all actions
            builder.ConfigureServices(s =>
            {
                if (exportConfig)
                {
                    exportConfigSettings.CodePath = exportConfigSettings.CodePath ?? Assembly.GetEntryAssembly().Location;
                    s.AddSingleton(exportConfigSettings);
                    s.AddSingleton<IHostedService, StreamDeckConfigHost>();
                }
                else
                {
                    s.AddSingleton<IHostedService, StreamDeckHost>();
                }

                s.AddTransient<EventManager>();

                s.AddSingleton<StreamDeckPluginManager>();
                s.AddSingleton<IPluginManager>(c => c.GetRequiredService<StreamDeckPluginManager>());
                s.AddSingleton(typeof(IPluginManager<>), typeof(StreamDeckPluginManager<>));

                s.AddScoped<IActionManagerProvider, StreamDeckActionManagerProvider>();

                s.AddScoped(c => c.GetRequiredService<IActionManagerProvider>().CurrrentActionManager);
                s.AddScoped(typeof(IActionManager<>), typeof(StreamDeckActionManager<>));

                var configBuilder = new StreamDeckConfigurationBuilder(s, callingAssembly);
                configure?.Invoke(configBuilder);
                s.AddSingleton(sp =>
                {
                    return configBuilder.Build();
                });

                if (runInMemory)
                {
                    s.AddSingleton<IStreamDeckConnection, StreamDeckEmulator>();
                }
                else
                {
                    s.AddSingleton(registrationArgs);
                    s.AddSingleton<IStreamDeckConnection, StreamDeckConnection>();
                }
            });

            return builder;
        }
    }

    public class StartupArguments
    {
        public int? Port { get; set; }

        public string PluginUUID { get; set; }

        public string RegisterEvent { get; set; }

        public string Info { get; set; }
    }

    public class ExportConfigArguments
    {
        public string ManifestExportPath { get; set; }

        public string CodePath { get; set; }

        public string SdkPath { get; set; }
    }
}
