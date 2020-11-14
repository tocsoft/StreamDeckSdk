using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Tocsoft.StreamDeck
{
    internal class StreamDeckConfigHost : IHostedService
    {
        private readonly IStreamDeckConfiguration configuration;
        private readonly string path;
        private readonly string codePath;
        private readonly IHostApplicationLifetime applicationLifetime;

        public StreamDeckConfigHost(IStreamDeckConfiguration configuration, ExportConfigArguments arguments, IHostApplicationLifetime applicationLifetime)
        {
            this.configuration = configuration;
            this.path = arguments.ManifestExportPath;
            this.codePath = arguments.CodePath;
            this.applicationLifetime = applicationLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // force ouput of 
            var fields = typeof(FontFamilies).GetFields();
            string Description<T>(T? value) where T : struct, Enum
            {
                if (!value.HasValue)
                {
                    return null;
                }

                var name = value.ToString();

                return fields.Single(x => x.Name == name)
                    .GetCustomAttribute<DescriptionAttribute>()?.Description ?? name;
            }

            var directory = Path.GetDirectoryName(this.codePath);

            string DefaultConfigIcon()
            {
                if (!string.IsNullOrEmpty(configuration.Icon))
                {
                    return configuration.Icon;
                }

                var asemblyNameIcon = Path.GetFileNameWithoutExtension(codePath);
                var assemblyIcon = $"Icons\\{asemblyNameIcon}";
                if (IsValidIcon(asemblyNameIcon))
                {
                    return asemblyNameIcon;
                }

                if (configuration.Actions.Count == 1)
                {
                    var action = configuration.Actions.Single();
                    if (!string.IsNullOrEmpty(action.Icon))
                    {
                        return action.Icon;
                    }

                    var handlerIcon = $"Icons\\{action.Handler.Name}";
                    if (IsValidIcon(handlerIcon))
                    {
                        return handlerIcon;
                    }
                }

                var genericName = $"Icons\\Plugin";
                if (IsValidIcon(genericName))
                {
                    return genericName;
                }

                return @"fallback";
            }

            var configIcon = DefaultConfigIcon();

            bool IsValidPath(string path)
            {
                if (string.IsNullOrEmpty(path))
                {
                    return false;
                }

                var filePath = Path.Combine(directory, path);
                if (!File.Exists(filePath))
                {
                    return false;
                }
                return true;
            }

            bool IsValidIcon(string icon)
            {
                if (string.IsNullOrEmpty(icon))
                {
                    return false;
                }

                var filePath = Path.Combine(directory, $"{icon}.png");
                if (!File.Exists(filePath))
                {
                    return false;
                }
                return true;
            }

            string IconDefault(ActionDefinition action)
            {
                if (!string.IsNullOrEmpty(action.Icon))
                {
                    return action.Icon;
                }

                var handlerIcon = $"Icons\\{action.Handler.Name}";
                if (IsValidIcon(handlerIcon))
                {
                    return handlerIcon;
                }

                if (IsValidIcon(configIcon))
                {
                    return configIcon;
                }

                return null;
            }
            string ActionPropertyInspectorDefault(ActionDefinition action)
            {

                if (!string.IsNullOrEmpty(action.PropertyInspector))
                {
                    return action.PropertyInspector;
                }

                var handlerIcon = $"PropertyInspectors\\{action.Handler.Name}.html";
                if (IsValidPath(handlerIcon))
                {
                    return handlerIcon;
                }

                return null;
            }



            var metadata = new
            {
                Author = configuration.Author ?? "",
                CodePath = Path.GetFileName(codePath).Replace(".dll", ".exe"),
                Description = configuration.Description ?? configuration.Name,
                Icon = configIcon,
                Name = configuration.Name,
                Version = configuration.Version,
                PropertyInspectorPath = configuration.PropertyInspector,
                SDKVersion = 2,
                OS = configuration.OS.Select(x => new
                {
                    Platform = x.Platform.ToString().ToLower(),
                    MinimumVersion = x.MinimumVersion
                }),
                Software = new
                {
                    MinimumVersion = configuration.MinimumStreamDeckVersion ?? "4.1"
                },
                Actions = configuration.Actions.Select(x => new
                {
                    Icon = IconDefault(x),
                    Name = x.Name,
                    UUID = x.UUID,
                    PropertyInspectorPath = ActionPropertyInspectorDefault(x),
                    States = x.States.Select(s => new
                    {
                        FontFamily = Description(s.FontFamily),
                        FontStyle = Description(s.FontStyle),
                        s.FontSize,
                        Image = s.Image ?? IconDefault(x),
                        s.MultiActionImage,
                        s.Name,
                        s.ShowTitle,
                        s.Title,
                        s.TitleAlignment,
                        s.TitleColor
                    })
                })
            };
            bool hasErrors = false;
            void ValidateIcon(string icon)
            {
                if (!IsValidIcon(icon))
                {
                    Console.Error.WriteLine($"{icon}.png is missing");
                    hasErrors = true;
                }
            }
            void ValidatePath(string path)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    if (!IsValidPath(path))
                    {
                        Console.Error.WriteLine($"{path} is missing");
                        hasErrors = true;
                    }
                }
            }
            //lets validate the files exists
            ValidateIcon(metadata.Icon);
            ValidatePath(metadata.PropertyInspectorPath);

            foreach (var action in metadata.Actions)
            {
                ValidateIcon(action.Icon);
                ValidatePath(action.PropertyInspectorPath);
                foreach (var state in action.States)
                {
                    ValidateIcon(state.Image);
                    if (!string.IsNullOrEmpty(state.MultiActionImage))
                    {
                        ValidateIcon(state.MultiActionImage);
                    }
                }
            }

            if (hasErrors)
            {
                Environment.ExitCode = 1;
            }

            var json = System.Text.Json.JsonSerializer.Serialize(metadata, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                IgnoreNullValues = true
            });

            File.WriteAllText(path, json);

            applicationLifetime.StopApplication();

            //Environment.Exit(0);
            return Task.CompletedTask;
        }

        private bool IsValidPath(object propertyInspector)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
