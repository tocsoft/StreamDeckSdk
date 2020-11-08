using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StreamDeckEmulator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((h, l) =>
                {
                    var pid = h.Configuration.GetValue<string>("Pid");

                    if (pid != null)
                    {
                        l.ClearProviders();
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var haswwwroot = Directory.Exists(Path.Combine(location, "wwwroot"));
                    if (haswwwroot)
                    {
                        webBuilder.UseContentRoot(location);
                    }
                    webBuilder.UseUrls("http://*:0");
                    webBuilder.UseStartup<Startup>();
                });
    }
}

/*

each plugin path location gets persistance storage

Each action instance for each plugin path


 */
