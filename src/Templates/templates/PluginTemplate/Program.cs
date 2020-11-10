using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Tocsoft.StreamDeck;

namespace PluginTemplate
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureStreamDeck(args, c =>
                {
                    c.AddAction<MyActionHandler>(a =>
                    {
                        a.Name = "My Action";
                    });
                });
    }
}
