using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Castle.Windsor;
using ServiceAnt.Handler.Subscription.Handler;
using ServiceAnt.Handler;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using ServiceAnt.Request.Handler;

namespace ServiceAnt.IocInstaller.Castle.Test
{
    /// <summary>
    /// ServiceAntInstaller_Test
    /// </summary>
    [TestClass]
    public class ServiceAntInstaller_Test
    {
        private static string RESULT_CONTAINER = "";

        public ServiceAntInstaller_Test()
        {
        }

        [TestMethod]
        public async Task CanHandleEventByIocHandler()
        {
            var testValue = "HelloWorld";
            var newContainer = new WindsorContainer();
            newContainer.Install(new ServiceAntInstaller());

            newContainer.Register(Component.For<IocEventHandler>().LifestyleTransient());

            await newContainer.Resolve<IServiceBus>().Publish(new TestTray() { Result = testValue });

            Assert.AreEqual(testValue, RESULT_CONTAINER);
        }

        [TestMethod]
        public async Task CanHandleRequestByIocHandler()
        {
            var testValue = "HelloWorld2";
            var newContainer = new WindsorContainer();
            newContainer.Install(new ServiceAntInstaller());

            newContainer.Register(Component.For<IocRequestHandler>().LifestyleTransient());

            var result = await newContainer.Resolve<IServiceBus>().SendAsync<string>(new TestTray() { Result = testValue });

            Assert.AreEqual(testValue, result);
        }

        internal class TestTray : TransportTray
        {
            public string Result { get; set; }
        }

        internal class IocEventHandler : IEventHandler<TestTray>
        {
            public Task HandleAsync(TestTray param)
            {
                RESULT_CONTAINER = param.Result;

                return Task.Delay(1);
            }
        }

        internal class IocRequestHandler : IRequestHandler<TestTray>
        {
            public Task HandleAsync(TestTray param, IRequestHandlerContext handlerContext)
            {
                handlerContext.Response = param.Result;
                return Task.Delay(1);
            }
        }
    }
}
