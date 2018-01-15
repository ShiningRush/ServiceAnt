using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceAnt;
using ServiceAnt.Request;
using ServiceAnt.Request.Handler;
using ServiceAnt.Subscription;
using System;
using System.Threading.Tasks;

namespace YiBan.Common.BaseAbpModule.Tests.Events
{
    [TestClass]
    public class InProcessServiceBus_Test
    {
        private class TestEventData : IEventTrigger
        {
            public string Msg { get; set; }
        }

        private class TestRequestData : IRequestTrigger
        {
            public string Msg { get; set; }
        }

        private class TestEventDataWithParam : IEventTrigger
        {
            public TestEventDataWithParam(string Msg) { }

            public string Msg { get; set; }
        }

        private class TestEventDataT<T> : EventTrigger<T>
        {
            public TestEventDataT(T test) : base(test)
            { }

            public string Msg { get; set; }
        }

        private class TestRequestDataT<T> : RequestTrigger<T>
        {
            public TestRequestDataT(T test) : base(test)
            { }

            public string Msg { get; set; }
        }

        #region EventTests
        [TestMethod]
        public async Task DynamicSubscription_ShouldTrigger()
        {
            var eventBus = new InProcessServiceBus();
            var result = "error";

            eventBus.AddDynamicSubscription(typeof(TestEventData).Name, eventData =>
            {
                return Task.Run(() =>
                {
                    result = eventData.Msg;
                });
            });

            var testEventData = new TestEventData() { Msg = "success" };
            await eventBus.Publish(testEventData);

            Assert.AreEqual(testEventData.Msg, result);
        }

        [TestMethod]
        public void DynamicSubscription_ShouldNotTrigger_AfterRemove()
        {
            var eventBus = new InProcessServiceBus();
            var result = "error";

            Func<dynamic, Task> delateFunc = eventData =>
            {
                return Task.Run(() =>
                {
                    result = eventData.Msg;
                });
            };
            eventBus.AddDynamicSubscription(typeof(TestEventData).Name, delateFunc);

            var testEventData = new TestEventData() { Msg = "success" };
            eventBus.PublishSync(testEventData);

            Assert.AreEqual(testEventData.Msg, result);

            result = "error";
            eventBus.RemoveDynamicSubscription(typeof(TestEventData).Name, delateFunc);
            eventBus.PublishSync(testEventData);
            Assert.AreNotEqual(testEventData.Msg, result);
        }

        [TestMethod]
        public void Subscription_ShouldTrigger()
        {
            var eventBus = new InProcessServiceBus();
            var result = "error";

            eventBus.AddSubscription<TestEventData>(eventData =>
            {
                return Task.Run(() =>
                {
                    result = eventData.Msg;
                });
            });

            var testEventData = new TestEventData() { Msg = "success" };
            eventBus.PublishSync(testEventData);


            Assert.AreEqual(testEventData.Msg, result);
        }

        [TestMethod]
        public void Subscription_ShouldNotTrigger_AfterRemove()
        {
            var eventBus = new InProcessServiceBus();
            var result = "error";

            Func<TestEventData, Task> delateFunc = eventData =>
            {
                return Task.Run(() =>
                {
                    result = eventData.Msg;
                });
            };
            eventBus.AddSubscription<TestEventData>(delateFunc);

            var testEventData = new TestEventData() { Msg = "success" };
            eventBus.PublishSync(testEventData);

            Assert.AreEqual(testEventData.Msg, result);

            result = "error";
            eventBus.RemoveSubscription<TestEventData>(delateFunc);
            eventBus.PublishSync(testEventData);
            Assert.AreNotEqual(testEventData.Msg, result);
        }

        [TestMethod]
        public void MutipleSubscription_ShouldTrigger()
        {
            var eventBus = new InProcessServiceBus();
            var result = "error";

            eventBus.AddSubscription<TestEventData>(eventData =>
            {
                return Task.Run(() =>
                {
                    result = eventData.Msg + "1";
                });
            });

            eventBus.AddSubscription<TestEventData>(eventData =>
            {
                return Task.Run(() =>
                {
                    result = eventData.Msg + "2";
                });
            });

            var testEventData = new TestEventData() { Msg = "success" };
            eventBus.PublishSync(testEventData);


            Assert.AreEqual(testEventData.Msg + "2", result);
        }

        [TestMethod]
        public void GenericSubscription_ShouldTrigger()
        {
            var eventBus = new InProcessServiceBus();
            var result = "error";

            eventBus.AddSubscription<TestEventDataT<TestEventData>>(eventData =>
            {
                return Task.Run(() =>
                {
                    result = eventData.Msg;
                });
            });

            var testEventData = new TestEventDataT<TestEventData>(new TestEventData()) { Msg = "success" };
            eventBus.PublishSync(testEventData);


            Assert.AreEqual(testEventData.Msg, result);
        }

        [TestMethod]
        public void SubscriptionEventDataWithParam_ShouldTrigger()
        {
            var eventBus = new InProcessServiceBus();
            var result = "error";

            eventBus.AddSubscription<TestEventDataWithParam>(eventData =>
            {
                return Task.Run(() =>
                {
                    result = eventData.Msg;
                });
            });

            var testEventData = new TestEventDataWithParam("success");
            eventBus.PublishSync(testEventData);


            Assert.AreEqual(testEventData.Msg, result);
        }

        [TestMethod]
        public void ShouldSupport_MutipleSameHandler()
        {
            var eventBus = new InProcessServiceBus();
            var result1 = "error";
            var result2 = "error";

            eventBus.AddSubscription<TestEventData>(eventData =>
            {
                return Task.Run(() =>
                {
                    result1 = eventData.Msg;
                });
            });

            eventBus.AddSubscription<TestEventData>(eventData =>
            {
                return Task.Run(() =>
                {
                    result2 = eventData.Msg;
                });
            });

            var testEventData = new TestEventData() { Msg = "success" };
            eventBus.PublishSync(testEventData);


            Assert.AreEqual(testEventData.Msg, result1);
            Assert.AreEqual(testEventData.Msg, result2);
        }

        [TestMethod]
        public void MutipleGenericSubscription_ByDifferentNameSpace_ShouldTrigger()
        {
            var eventBus = new InProcessServiceBus();
            var result = "error";
            var result2 = "error";

            eventBus.AddSubscription<TestEventDataT<TestEventData>>(eventData =>
            {
                return Task.Run(() =>
                {
                    result = eventData.Msg;
                });
            });

            eventBus.AddSubscription<TestEventDataT<TestDemo.TestEventData>>(eventData =>
            {
                return Task.Run(() =>
                {
                    result2 = eventData.Msg;
                });
            });

            var testEventData = new TestEventDataT<TestEventData>(new TestEventData()) { Msg = "success" };
            eventBus.PublishSync(testEventData);


            Assert.AreEqual(testEventData.Msg, result);
            Assert.AreEqual(testEventData.Msg, result2);
        }
        #endregion

        #region RequestTests

        [TestMethod]
        public void DynamicRequest_ShouldResponse()
        {
            var eventBus = new InProcessServiceBus();

            eventBus.AddDynamicRequestHandler(typeof(TestRequestData).Name, (eventData, context) =>
            {
                return Task.Run(() =>
                {
                    context.Response = (string)eventData.Msg;
                });
            });

            var testRequestData = new TestRequestData() { Msg = "success" };
            var result = eventBus.Send<string>(testRequestData);

            Assert.AreEqual(testRequestData.Msg, result);
        }

        [TestMethod]
        public void DynamicRequest_ShouldNotResponse_AfterRemove()
        {
            var eventBus = new InProcessServiceBus();
            Func<dynamic, IRequestHandlerContext, Task> delateFunc = (eventData, context) =>
            {
                return Task.Run(() =>
                {
                    context.Response = (string)eventData.Msg;
                });
            };

            eventBus.AddDynamicRequestHandler(typeof(TestRequestData).Name, delateFunc);
            eventBus.RemoveDynamicRequestHandler(typeof(TestRequestData).Name, delateFunc);

            var testRequestData = new TestRequestData() { Msg = "success" };
            var result = eventBus.Send<string>(testRequestData);

            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public void Request_ShouldResponse()
        {
            var eventBus = new InProcessServiceBus();

            eventBus.AddRequestHandler<TestRequestData>((eventData, context) =>
            {
                return Task.Run(() =>
                {
                    context.Response = eventData.Msg;
                });
            });

            var testRequestData = new TestRequestData() { Msg = "success" };
            var result = eventBus.Send<string>(testRequestData);

            Assert.AreEqual(testRequestData.Msg, result);
        }

        [TestMethod]
        public void Request_ShouldNotResponse_AfteRemove()
        {
            var eventBus = new InProcessServiceBus();

            Func<dynamic, IRequestHandlerContext, Task> delateFunc = (eventData, context) =>
            {
                return Task.Run(() =>
                {
                    context.Response = (string)eventData.Msg;
                });
            };

            eventBus.AddRequestHandler<TestRequestData>(delateFunc);
            eventBus.RemoveRequestHandler<TestRequestData>(delateFunc);

            var testRequestData = new TestRequestData() { Msg = "success" };
            var result = eventBus.Send<string>(testRequestData);

            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public void MutipleRequestHandler_ShouldResponseForPipeline()
        {
            var eventBus = new InProcessServiceBus();

            eventBus.AddRequestHandler<TestRequestData>((eventData, context) =>
            {
                return Task.Run(() =>
                {
                    context.Response = eventData.Msg + "1";
                });
            });

            eventBus.AddRequestHandler<TestRequestData>((eventData, context) =>
            {
                return Task.Run(() =>
                {
                    context.Response = context.Response + "2";
                });
            });

            var testRequestData = new TestRequestData() { Msg = "success" };
            var result = eventBus.Send<string>(testRequestData);

            Assert.AreEqual(testRequestData.Msg + "12", result);
        }

        [TestMethod]
        public void MutipleRequestHandler_ShouldResponseForPipeline_ReturnFirst()
        {
            var eventBus = new InProcessServiceBus();

            eventBus.AddRequestHandler<TestRequestData>((eventData, context) =>
            {
                return Task.Run(() =>
                {
                    context.Response = eventData.Msg + "1";
                    context.IsEnd = true;
                });
            });

            eventBus.AddRequestHandler<TestRequestData>((eventData, context) =>
            {
                return Task.Run(() =>
                {
                    context.Response = context.Response + "2";
                });
            });

            var testRequestData = new TestRequestData() { Msg = "success" };
            var result = eventBus.Send<string>(testRequestData);

            Assert.AreEqual(testRequestData.Msg + "1", result);
        }


        [TestMethod]
        public void GenericRequest_ShouldResponse()
        {
            var eventBus = new InProcessServiceBus();

            eventBus.AddRequestHandler<TestRequestDataT<TestEventData>>((eventData, context) =>
            {
                return Task.Run(() =>
                {
                    context.Response = eventData.Dto.Msg + eventData.Msg;
                });
            });

            var testRequestData = new TestRequestDataT<TestEventData>(new TestEventData() { Msg = "non" }) { Msg = "success" };
            var result = eventBus.Send<string>(testRequestData);

            Assert.AreEqual(testRequestData.Dto.Msg + testRequestData.Msg, result);
        }

        [TestMethod]
        public void MutipleGenericRequestHandler_ByDifferentNameSpace_ShouldResponse()
        {
            var eventBus = new InProcessServiceBus();

            eventBus.AddRequestHandler<TestRequestDataT<TestEventData>>((eventData, context) =>
            {
                return Task.Run(() =>
                {
                    context.Response = eventData.Msg + "1";
                });
            });

            eventBus.AddRequestHandler<TestRequestDataT<TestDemo.TestEventData>>((eventData, context) =>
            {
                return Task.Run(() =>
                {
                    context.Response = context.Response + "2";
                });
            });

            var testRequestData = new TestRequestDataT<TestEventData>(new TestEventData()) { Msg = "success" };
            var result = eventBus.Send<string>(testRequestData);


            Assert.AreEqual(testRequestData.Msg + "12", result);
        }
        #endregion

        #region CommonFunction

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ShouldRaiseException_WhenPublishSetOption()
        {
            var eventBus = new InProcessServiceBus();

            eventBus.AddDynamicSubscription(typeof(TestEventData).Name, eventData =>
            {
                return Task.Run(() =>
                {
                    throw new Exception("Test Exception");
                });
            });

            var testEventData = new TestEventData() { Msg = "success" };
            await eventBus.Publish(testEventData, new TriggerOption(false));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ShouldRaiseException_WhenSendSetOption()
        {
            var eventBus = new InProcessServiceBus();

            eventBus.AddDynamicRequestHandler(typeof(TestRequestData).Name, (requestData, requetContext) =>
            {
                return Task.Run(() =>
                {
                    throw new Exception("Test Exception");
                });
            });

            var testRequestData = new TestRequestData() { Msg = "success" };
            await eventBus.SendAsync<string>(testRequestData, new TriggerOption(false));
        }

        [TestMethod]
        public async Task ShouldLogMessage_WhenSetLogDelate()
        {
            var eventBus = new InProcessServiceBus();
            var logMsg = "";
            var testException = new Exception("Test Exception");
            Exception catchedException = null;
            eventBus.OnLogBusMessage += (logLevel, msg, ex) =>
            {
                logMsg = msg;
                catchedException = ex;
            };
            eventBus.AddDynamicSubscription(typeof(TestEventData).Name, eventData =>
            {
                return Task.Run(() =>
                {
                    throw testException;
                });
            });

            var testEventData = new TestEventData() { Msg = "success" };
            await eventBus.Publish(testEventData);

            Assert.AreNotEqual(0, logMsg.Length);
            Assert.AreEqual(testException, catchedException);
        }

        #endregion
    }
}

namespace TestDemo
{
    public class TestEventData
    {
        public string Msg { get; set; }
    }
}
