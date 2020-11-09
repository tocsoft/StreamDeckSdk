using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Tocsoft.StreamDeck.Events;

namespace Tocsoft.StreamDeck
{
    internal class StreamDeckHost : IHostedService
    {
        private readonly IStreamDeckConnection streamDeckConnection;
        private readonly IStreamDeckConfiguration configuration;
        private Dictionary<string, ActionDefinition> actions;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<StreamDeckHost> logger;
        private IDisposable appearListener;
        private IDisposable disappearListener;

        private Dictionary<string, IServiceScope> scopes = new Dictionary<string, IServiceScope>();

        public StreamDeckHost(IStreamDeckConnection streamDeckConnection, IStreamDeckConfiguration configuration, IServiceProvider serviceProvider, ILogger<StreamDeckHost> logger)
        {
            this.streamDeckConnection = streamDeckConnection;
            this.configuration = configuration;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // this listens and orchastrate communication from a streamdesk connection (be it inmemory or remote)

            await streamDeckConnection.Connect();

            this.appearListener = streamDeckConnection.Listen<WillAppearEvent>(StartAction);

            this.disappearListener = streamDeckConnection.Listen<WillDisappearEvent>(StopAction);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            streamDeckConnection?.Disconnect();
            appearListener?.Dispose();
            disappearListener?.Dispose();
            return Task.CompletedTask;
        }

        internal async Task StartAction(WillAppearEvent evt)
        {
            if (actions == null)
            {
                actions = configuration.Actions?.ToDictionary(x => x.UUID, x => x, StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, ActionDefinition>(); ;
            }

            try
            {
                if (!actions.TryGetValue(evt.Action, out var definition))
                {
                    logger.LogError("No action handler registered for '{ActionUUID}'", evt.Action);
                    return;
                }

                var scope = serviceProvider.CreateScope();
                // we find action details here!!

                var manager = scope.ServiceProvider.GetRequiredService<IActionManagerProvider>();

                await manager.Initialize(definition, evt);

                scopes.Add(evt.Context, scope);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal Task StopAction(WillDisappearEvent evt)
        {
            if (!scopes.TryGetValue(evt.Context, out var scope))
            {
                logger.LogWarning("No active scope for '{ContextID}'", evt.Context);
                return Task.CompletedTask;
            }

            scope.Dispose();
            scopes.Remove(evt.Context);
            logger.LogInformation("Scope for '{ContextID}' disposed", evt.Context);
            return Task.CompletedTask;
        }

    }
}
