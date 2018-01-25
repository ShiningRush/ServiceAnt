using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceAnt.Request;
using ServiceAnt.Request.Handler;
using ServiceAnt.Subscription;
using ServiceAnt.Subscription.Handler;
using System;
using System.Threading.Tasks;

namespace ServiceAnt.IocInstaller.DotNetCore.Test
{
    [TestClass]
    public class ServiceCollectionExtensions_Test
    {
        private static string RESULT_CONTAINER = "";
        private readonly IServiceProvider _provider;

        public ServiceCollectionExtensions_Test()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddServiceAnt(System.Reflection.Assembly.GetExecutingAssembly());
            _provider = services.BuildServiceProvider();
        }

        [TestMethod]
        public async Task CanHandleEventByIocHandler()
        {
            var testValue = "HelloWorld";

            await _provider.GetService<IServiceBus>().Publish(new TestEventTrigger() { Result = testValue });

            Assert.AreEqual(testValue, RESULT_CONTAINER);
        }

        [TestMethod]
        public async Task CanHandleRequestByIocHandler()
        {
            var testValue = "HelloWorld2";

            var result = await _provider.GetService<IServiceBus>().SendAsync<string>(new TestRequestTrigger() { Result = testValue });

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
