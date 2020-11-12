using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tocsoft.StreamDeck.Events;

namespace Tocsoft.StreamDeck
{
    internal class StreamDeckEmulator : IStreamDeckConnection, IDisposable
    {
        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly ILogger<StreamDeckConnection> logger;
        private readonly EventManager eventManager;

        // this is the thing that managest
        public StreamDeckEmulator(IHostApplicationLifetime applicationLifetime, ILogger<StreamDeckConnection> logger, EventManager eventManager)
        {
            this.applicationLifetime = applicationLifetime;
            this.logger = logger;
            this.eventManager = eventManager;
        }

        public async Task Connect()
        {
#if DEBUG
            Debugger.Launch();
#endif
            var cmd = "dotnet";
            var arguments = new string[] {
                "tool",
                "run",
                "StreamDeckEmulator",
                "--"
            };

            // default commands, lets fallback to the emulators directly 
            var envCmd = Environment.GetEnvironmentVariable("StreamDeckEmulatorCommand");
            if (!string.IsNullOrEmpty(envCmd))
            {
                var parts = envCmd.TrimEnd().Split(new[] { ' ' });
                cmd = parts[0];
                arguments = parts.Skip(1).ToArray();
            }
            // start emulator process if availible else lets logout that global tool is unavailible
            // emulator starting with a specific argument will get it to write out 
            var executionDirectory = Path.GetDirectoryName(typeof(StreamDeckEmulator).Assembly.Location);

            var depsPath = Directory.GetFiles(executionDirectory, "*.deps.json", SearchOption.TopDirectoryOnly).SingleOrDefault();

            if (!string.IsNullOrEmpty(depsPath))
            {
                var file = Newtonsoft.Json.JsonConvert.DeserializeObject<SimpleDepsFile>(File.ReadAllText(depsPath));
                var lib = file.libraries?.Where(x => x.Key.StartsWith("Tocsoft.StreamDeck/")).Select(X => X.Value).SingleOrDefault();
                if (lib != null)
                {
                    if (lib.type == "project")
                    {
                        var parent = executionDirectory;
                        while (parent != null && !Directory.Exists(Path.Combine(parent, ".git")))
                        {
                            parent = Path.GetDirectoryName(parent);
                        }
                        if (!string.IsNullOrEmpty(parent))
                        {
                            var emulatorProject = Path.GetFullPath(Path.Combine(parent, "src/Tocsoft.StreamDeckEmulator/Tocsoft.StreamDeckEmulator.csproj"));

                            if (File.Exists(emulatorProject))
                            {
                                cmd = "dotnet";
                                arguments = new[] {
                                    "run",
                                    "-p",
                                    emulatorProject,
                                    "--"
                                };
                            }
                        }
                        // sln root 
                        // this is running from reference to a project rather than the package
                        // lets walkt the tree looking for the csproj and look for the 
                    }
                    else
                    {

                        var probPaths = Directory.GetFiles(executionDirectory, "*.runtimeconfig.dev.json", SearchOption.TopDirectoryOnly).SingleOrDefault();
                        if (!string.IsNullOrEmpty(probPaths))
                        {
                            var obj = JObject.Parse(File.ReadAllText(probPaths));
                            var paths = obj["runtimeOptions"]?["additionalProbingPaths"]?.Values<string>();
                            if (paths != null)
                            {
                                foreach (var p in paths)
                                {
                                    var emulatorPath = Path.Combine(p, lib.path, "tools\\emulator\\Tocsoft.StreamDeckEmulator.dll");
                                    if (File.Exists(emulatorPath))
                                    {
                                        cmd = $"dotnet";
                                        arguments = new[] {
                                            emulatorPath
                                        };
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // find the 'deps.json' files
            // find the 'runtime.dev.config'
            // if the type == project then use 'dotnet run' to the project.
            // if the type == 'package' then find the tools folder in the package folder and run the tool from there!

            // find the package store location
            var info = new ProcessStartInfo(cmd);
            foreach (var a in arguments)
            {
                info.ArgumentList.Add(a);
            }
            info.ArgumentList.Add("--Pid");
            info.ArgumentList.Add(Process.GetCurrentProcess().Id.ToString());
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = false;

            this.process = Process.Start(info);
            this.process.EnableRaisingEvents = true;
            this.process.Exited += (s, e) =>
            {

                applicationLifetime.StopApplication();
            };

            StringBuilder dataReceived = new StringBuilder();
            Regex matcher = new Regex(@"(.*?)--STREAMDECK_REG_END--", RegexOptions.Singleline);
            bool capturelines = false;
            while (!this.process.HasExited)
            {
                var line = await process.StandardOutput.ReadLineAsync();
                if (capturelines)
                {
                    var settings = StreamDeckEvent.Deserialize<StartupArguments>(line);
                    this.connection = new StreamDeckConnection(settings, logger, eventManager, applicationLifetime);
                    await this.connection.Connect();
                    break;
                }
                else if (line == "--STREAMDECK_REG_START--")
                {
                    capturelines = true;
                }
            }
        }

        private Process process;
        private IStreamDeckConnection connection;
        public Task Disconnect()
        {
            // kill emulator process too here
            var con = connection;
            connection = null;
            return con?.Disconnect() ?? Task.CompletedTask;
        }

        public IDisposable Listen<TEventType>(Func<TEventType, Task> callback) where TEventType : StreamDeckInboundEvent
        {
            return connection?.Listen(callback);
        }

        public Task SendEvent<TEventType>(TEventType details) where TEventType : StreamDeckOutboundEvent
        {
            return connection.SendEvent(details);
        }

        public void Dispose()
        {
            process?.Kill();
            process = null;
        }

        private class SimpleDepsFile
        {
            public Dictionary<string, SimpleDepsFileLib> libraries { get; set; }
        }
        private class SimpleDepsFileLib
        {
            public string type { get; set; }
            public string path { get; set; }

        }
    }
}
