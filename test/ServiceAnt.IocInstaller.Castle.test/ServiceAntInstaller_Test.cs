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
using ServiceAnt.Subscription.Handler;
using ServiceAnt.Subscription;
using ServiceAnt.Request;

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
            newContainer.Install(new ServiceAntInstaller(System.Reflection.Assembly.GetExecutingAssembly()));

            await newContainer.Resolve<IServiceBus>().Publish(new TestEventTrigger() { Result = testValue });

            Assert.AreEqual(testValue, RESULT_CONTAINER);
        }

        [TestMethod]
        public async Task CanHandleRequestByIocHandler()
        {
            var testValue = "HelloWorld2";
            var newContainer = new WindsorContainer();
            newContainer.Install(new ServiceAntInstaller(System.Reflection.Assembly.GetExecutingAssembly()));

            var result = await newContainer.Resolve<IServiceBus>().SendAsync<string>(new TestRequestTrigger() { Result = testValue });

            Assert.AreEqual(testValue, result);
        }

        public class TestEventTrigger : IEventTrigger
        {
            public string Result { get; set; }
        }

        public class TestRequestTrigger : IRequestTrigger
        {
            public string Result { get; set; }
        }

        public class IocEventHandler : IEventHandler<TestEventTrigger>
        {
            public Task HandleAsync(TestEventTrigger param)
            {
                RESULT_CONTAINER = param.Result;

                return Task.Delay(1);
            }
        }

        public class IocRequestHandler : IRequestHandler<TestRequestTrigger>
        {
            public Task HandleAsync(TestRequestTrigger param, IRequestHandlerContext handlerContext)
            {
                handlerContext.Response = param.Result;
                return Task.Delay(1);
            }
        }
    }
}
