using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Security.Cryptography.X509Certificates;
using Tocsoft.StreamDeck.Events;

namespace Tocsoft.StreamDeck.Sample
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
