using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tocsoft.StreamDeck.Events;
using Tocsoft.StreamDeck.Tests.Fakes;
using Xunit;

namespace Tocsoft.StreamDeck.Tests
{
    public class StreamDeckHostTests
    {
        private Mock<IStreamDeckConnection> connectionMock = new Mock<IStreamDeckConnection>();
        private Mock<IDisposable> willAppearListenerMock = new Mock<IDisposable>();
        private Mock<IDisposable> willDisappearListenerMock = new Mock<IDisposable>();
        private StreamDeckConfiguration configuration = new StreamDeckConfiguration();
        private FakeServiceProvider services = new FakeServiceProvider();
        private FakeLogger<StreamDeckHost> logger = new FakeLogger<StreamDeckHost>();
        private StreamDeckHost host;

        public StreamDeckHostTests()
        {
            this.connectionMock.Setup(x => x.Listen(It.IsAny<Func<WillAppearEvent, Task>>())).Returns(willAppearListenerMock.Object);
            this.connectionMock.Setup(x => x.Listen(It.IsAny<Func<WillDisappearEvent, Task>>())).Returns(willDisappearListenerMock.Object);
            this.host = new StreamDeckHost(this.connectionMock.Object, this.configuration, this.services, logger);
        }

        [Fact]
        public async Task StopDisposesListeners()
        {
            await this.host.StartAsync(default);
            await this.host.StopAsync(default);

            this.willAppearListenerMock.Verify(x => x.Dispose());
            this.willDisappearListenerMock.Verify(x => x.Dispose());
            connectionMock.Verify(x => x.Disconnect());
        }

        [Fact]
        public async Task StartingRegistersEvents()
        {
            await this.host.StartAsync(default);

            this.connectionMock.VerifyAll();
            connectionMock.Verify(x => x.Connect());
        }

        [Fact]
        public async Task WillAppearEventTriggersStartsActionMethod()
        {
            Func<WillAppearEvent, Task> cb = null;
            this.connectionMock.Setup(x => x.Listen(It.IsAny<Func<WillAppearEvent, Task>>()))
              .Callback<Func<WillAppearEvent, Task>>(callback =>
              {
                  cb = callback;
              })
              .Returns(willAppearListenerMock.Object);

            await this.host.StartAsync(default);

            await cb.Invoke(new WillAppearEvent()
            {
                Action = "fakeAction.foo.bar"
            });

            Assert.Contains("No action handler registered for 'fakeAction.foo.bar'", this.logger.Messages);
        }

        [Fact]
        public async Task WillDisappearEventEventTriggersStartsActionMethod()
        {
            Func<WillDisappearEvent, Task> cb = null;
            this.connectionMock.Setup(x => x.Listen(It.IsAny<Func<WillDisappearEvent, Task>>()))
              .Callback<Func<WillDisappearEvent, Task>>(callback =>
              {
                  cb = callback;
              })
              .Returns(willAppearListenerMock.Object);

            await this.host.StartAsync(default);

            await cb.Invoke(new WillDisappearEvent()
            {
                Action = "fakeAction.foo.bar",
                Context = "ctxID"
            });

            Assert.Contains("No active scope for 'ctxID'", this.logger.Messages);
        }

        [Fact]
        public async Task StartActionInitializesManager()
        {
            var actionDef = new ActionDefinition(new ActionDefinitionBuilder(typeof(StreamDeckHostTests)));
            var triggerEvent = new WillAppearEvent
            {
                Action = "action",
                Context = "ctx"
            };
            this.configuration.Actions = new[] { actionDef };

            var moqProvider = new Mock<IActionManagerProvider>();
            this.services.AddService(moqProvider.Object);
            await this.host.StartAction(triggerEvent);

            var scope = Assert.Single(this.services.Scopes);
            Assert.False(scope.Disposed);
            moqProvider.Verify(x => x.Initialize(actionDef, triggerEvent));
        }

        [Fact]
        public async Task StopActionInitializesManager()
        {
            var actionDef = new ActionDefinition(new ActionDefinitionBuilder(typeof(StreamDeckHostTests)));
            this.configuration.Actions = new[] { actionDef };

            var moqProvider = new Mock<IActionManagerProvider>();
            this.services.AddService(moqProvider.Object);

            await this.host.StartAction(new WillAppearEvent
            {
                Action = "action",
                Context = "ctx"
            });
            await this.host.StopAction(new WillDisappearEvent
            {
                Action = "action",
                Context = "ctx"
            });
            var scope = Assert.Single(this.services.Scopes);
            Assert.True(scope.Disposed);
        }
    }
}
