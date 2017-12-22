using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Autofac;
using ServiceAnt.Handler.Subscription.Handler;
using ServiceAnt.Request.Handler;
using ServiceAnt.Handler;

namespace ServiceAnt.IocInstaller.Autofac.Test
{
    [TestClass]
    public class ServiceAntModule_Test
    {
        private static string RESULT_CONTAINER = "";

        public ServiceAntModule_Test()
        {
        }

        [TestMethod]
        public async Task CanHandleEventByIocHandler()
        {
            var testValue = "HelloWorld";
            var newContainer = new ContainerBuilder();
            newContainer.RegisterModule(new ServiceAntModule(System.Reflection.Assembly.GetExecutingAssembly()));
            var autofacContainer = newContainer.Build();
            ServiceAntModule.RegisterHandlers(autofacContainer);

            await autofacContainer.Resolve<IServiceBus>().Publish(new TestTray() { Result = testValue });

            Assert.AreEqual(testValue, RESULT_CONTAINER);
        }

        [TestMethod]
        public async Task CanHandleRequestByIocHandler()
        {
            var testValue = "HelloWorld2";
            var newContainer = new ContainerBuilder();
            newContainer.RegisterModule(new ServiceAntModule(System.Reflection.Assembly.GetExecutingAssembly()));
            var autofacContainer = newContainer.Build();
            ServiceAntModule.RegisterHandlers(autofacContainer);

            var result = await autofacContainer.Resolve<IServiceBus>().SendAsync<string>(new TestTray() { Result = testValue });

            Assert.AreEqual(testValue, result);
        }

        public class TestTray : TransportTray
        {
            public string Result { get; set; }
        }

        public class IocEventHandler : IEventHandler<TestTray>
        {
            public Task HandleAsync(TestTray param)
            {
                RESULT_CONTAINER = param.Result;

                return Task.Delay(1);
            }
        }

        public class IocRequestHandler : IRequestHandler<TestTray>
        {
            public Task HandleAsync(TestTray param, IRequestHandlerContext handlerContext)
            {
                handlerContext.Response = param.Result;
                return Task.Delay(1);
            }
        }
    }
}
