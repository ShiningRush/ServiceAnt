using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceAnt;
using ServiceAnt.Handler;
using ServiceAnt.Request.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YiBan.Common.BaseAbpModule.Tests.Events
{
    [TestClass]
    public class InProcessServiceBus_Test
    {
        private class TestEventData : TransportTray
        {
            public string Msg { get; set; }
        }

        private class TestEventDataWithParam : TransportTray
        {
            public TestEventDataWithParam(string Msg) { }

            public string Msg { get; set; }
        }

        private class TestEventDataT<T> : TransportTray<T>
        {
            public TestEventDataT(T test) : base(test) { }

            public string Msg { get; set; }
        }

        #region EventTests
        [TestMethod]
        public async Task DynamicSubscription_ShouldTrigger()
        {
            var eventBus = new InProcessServiceBus();
            var result = "error";

            eventBus.AddDynamicSubScription(typeof(TestEventData).Name, eventData =>
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
            eventBus.AddDynamicSubScription(typeof(TestEventData).Name, delateFunc);

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

            eventBus.AddSubScription<TestEventData>(eventData =>
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
            eventBus.AddSubScription<TestEventData>(delateFunc);

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

            eventBus.AddSubScription<TestEventData>(eventData =>
            {
                return Task.Run(() =>
                {
                    result = eventData.Msg + "1";
                });
            });

            eventBus.AddSubScription<TestEventData>(eventData =>
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

            eventBus.AddSubScription<TestEventDataT<TestEventData>>(eventData =>
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

            eventBus.AddSubScription<TestEventDataWithParam>(eventData =>
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

        //[TestMethod]
        //public void SubscriptionEntityEvent_ShouldTrigger()
        //{
        //    var eventBus = new InProcessServiceBus();
        //    var result = "error";

        //    eventBus.AddSubScription<EntityCreatedEventData<TestEventData>>(eventData =>
        //    {
        //        return Task.Run(() =>
        //        {
        //            result = eventData.TransportEntity.Msg;
        //        });
        //    });

        //    var entity = new TestEventData() { Msg = "success" };
        //    var testEventData = new EntityCreatedEventData<TestEventData>(entity);
        //    eventBus.PublishSync(testEventData);


        //    Assert.AreEqual(testEventData.TransportEntity.Msg, result);
        //}

        [TestMethod]
        public void ShouldSupport_MutipleSameHandler()
        {
            var eventBus = new InProcessServiceBus();
            var result1 = "error";
            var result2 = "error";

            eventBus.AddSubScription<TestEventData>(eventData =>
            {
                return Task.Run(() =>
                {
                    result1 = eventData.Msg;
                });
            });

            eventBus.AddSubScription<TestEventData>(eventData =>
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

            eventBus.AddSubScription<TestEventDataT<TestEventData>>(eventData =>
            {
                return Task.Run(() =>
                {
                    result = eventData.Msg;
                });
            });

            eventBus.AddSubScription<TestEventDataT<TestDemo.TestEventData>>(eventData =>
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

            eventBus.AddDynamicRequestHandler(typeof(TestEventData).Name, (eventData, context) =>
            {
                return Task.Run(() =>
                {
                    context.Response = (string)eventData.Msg;
                });
            });

            var testEventData = new TestEventData() { Msg = "success" };
            var result = eventBus.Send<string>(testEventData);

            Assert.AreEqual(testEventData.Msg, result);
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

            eventBus.AddDynamicRequestHandler(typeof(TestEventData).Name, delateFunc);
            eventBus.RemoveDynamicRequestHandler(typeof(TestEventData).Name, delateFunc);

            var testEventData = new TestEventData() { Msg = "success" };
            var result = eventBus.Send<string>(testEventData);

            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public void Request_ShouldResponse()
        {
            var eventBus = new InProcessServiceBus();

            eventBus.AddRequestHandler<TestEventData>((eventData, context) =>
            {
                return Task.Run(() =>
                {
                    context.Response = eventData.Msg;
                });
            });

            var testEventData = new TestEventData() { Msg = "success" };
            var result = eventBus.Send<string>(testEventData);

            Assert.AreEqual(testEventData.Msg, result);
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

            eventBus.AddRequestHandler<TestEventData>(delateFunc);
            eventBus.RemoveRequestHandler<TestEventData>(delateFunc);

            var testEventData = new TestEventData() { Msg = "success" };
            var result = eventBus.Send<string>(testEventData);

            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public void MutipleRequestHandler_ShouldResponseForPipeline()
        {
            var eventBus = new InProcessServiceBus();

            eventBus.AddRequestHandler<TestEventData>((eventData, context) =>
            {
                return Task.Run(() =>
                {
                    context.Response = eventData.Msg + "1";
                });
            });

            eventBus.AddRequestHandler<TestEventData>((eventData, context) =>
            {
                return Task.Run(() =>
                {
                    context.Response = context.Response + "2";
                });
            });

            var testEventData = new TestEventData() { Msg = "success" };
            var result = eventBus.Send<string>(testEventData);

            Assert.AreEqual(testEventData.Msg + "12", result);
        }

        [TestMethod]
        public void MutipleRequestHandler_ShouldResponseForPipeline_ReturnFirst()
        {
            var eventBus = new InProcessServiceBus();

            eventBus.AddRequestHandler<TestEventData>((eventData, context) =>
            {
                return Task.Run(() =>
                {
                    context.Response = eventData.Msg + "1";
                    context.IsEnd = true;
                });
            });

            eventBus.AddRequestHandler<TestEventData>((eventData, context) =>
            {
                return Task.Run(() =>
                {
                    context.Response = context.Response + "2";
                });
            });

            var testEventData = new TestEventData() { Msg = "success" };
            var result = eventBus.Send<string>(testEventData);

            Assert.AreEqual(testEventData.Msg + "1", result);
        }


        [TestMethod]
        public void GenericRequest_ShouldResponse()
        {
            var eventBus = new InProcessServiceBus();

            eventBus.AddRequestHandler<TestEventDataT<TestEventData>>((eventData, context) =>
            {
                return Task.Run(() =>
                {
                    context.Response = eventData.TransportEntity.Msg + eventData.Msg;
                });
            });

            var testEventData = new TestEventDataT<TestEventData>(new TestEventData() { Msg = "non" }) { Msg = "success" };
            var result = eventBus.Send<string>(testEventData);

            Assert.AreEqual(testEventData.TransportEntity.Msg + testEventData.Msg, result);
        }

        [TestMethod]
        public void MutipleGenericRequestHandler_ByDifferentNameSpace_ShouldResponse()
        {
            var eventBus = new InProcessServiceBus();

            eventBus.AddRequestHandler<TestEventDataT<TestEventData>>((eventData, context) =>
            {
                return Task.Run(() =>
                {
                    context.Response = eventData.Msg + "1";
                });
            });

            eventBus.AddRequestHandler<TestEventDataT<TestDemo.TestEventData>>((eventData, context) =>
            {
                return Task.Run(() =>
                {
                    context.Response = context.Response + "2";
                });
            });

            var testEventData = new TestEventDataT<TestEventData>(new TestEventData()) { Msg = "success" };
            var result = eventBus.Send<string>(testEventData);


            Assert.AreEqual(testEventData.Msg + "12", result);
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
