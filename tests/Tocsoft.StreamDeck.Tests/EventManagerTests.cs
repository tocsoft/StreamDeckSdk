using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Tocsoft.StreamDeck.Tests
{
    public class EventManagerTests
    {
        [Fact]
        public async Task CallingDisposeRemovesCallback()
        {
            var manager = new EventManager();

            var super = new SuperClass1();
            bool callbackFired = false;
            var disposable = manager.Register<BaseClass>(c =>
            {
                callbackFired = true;

                Assert.Equal(super, c);

                return Task.CompletedTask;
            });
            disposable.Dispose();

            await manager.TriggerEvent(super);

            Assert.False(callbackFired, $"Callback for {nameof(BaseClass)} called incorrectly");
        }

        [Fact]
        public async Task TriggerBaseClassRegistrationsFromSuperClass()
        {
            var manager = new EventManager();

            var super = new SuperClass1();
            bool callbackFired = false;
            manager.Register<BaseClass>(c =>
            {
                callbackFired = true;

                Assert.Equal(super, c);

                return Task.CompletedTask;
            });

            await manager.TriggerEvent(super);

            Assert.True(callbackFired, $"Callback for {nameof(BaseClass)} never called");
        }

        [Fact]
        public async Task ReturnFromTaskOnlyAfterAllEventsComplete()
        {
            var manager = new EventManager();

            var super = new SuperClass1();

            var firingOrder = new List<string>();
            var tcs1 = new TaskCompletionSource<object>();

            manager.Register<SuperClass1>(c =>
            {
                firingOrder.Add("callback1");
                return tcs1.Task;
            });
            manager.Register<SuperClass1>(c =>
            {
                firingOrder.Add("callback2");
                return tcs1.Task;
            });


            var t1 = manager.TriggerEvent(super).ContinueWith(t => firingOrder.Add("continue"));
            tcs1.SetResult(true);

            await t1;

            Assert.Equal(new[] { "callback1", "callback2", "continue" }, firingOrder);
        }



        private class BaseClass
        {

        }

        private class SuperClass1 : BaseClass
        {

        }
    }
}
