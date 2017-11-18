using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceAnt.Handler;
using ServiceAnt.Handler.Request;
using ServiceAnt.Handler.Request.Handler;
using ServiceAnt.Handler.Subscription.Handler;
using ServiceAnt.Infrastructure.Log;
using ServiceAnt.Subscription;
using System;
using System.Threading.Tasks;

namespace ServiceAnt
{
    public class InProcessServiceBus : IServiceBus
    {
        private ISubscriptionManager _subcriptionManager;
        private IRequestHandlerManager _requestHandlerManager;

        public ILogger Logger { get; set; }

        public InProcessServiceBus()
        {
            _subcriptionManager = new InMemorySubscriptionsManager();
            _requestHandlerManager = new InMemoryRequestHandlerManager();
            //Logger = NullLogger.Instance;
        }

        public InProcessServiceBus(ISubscriptionManager subcriptionManager)
        {
            _subcriptionManager = subcriptionManager;
        }

        #region Pub/Sub

        public void AddDynamicSubScription(string eventName, Func<dynamic, Task> action)
        {
            _subcriptionManager.AddDynamicSubScription(eventName, action);
        }


        public void AddSubScription(Type eventType, IHandlerFactory factory)
        {
            _subcriptionManager.AddSubScription(eventType, factory);
        }

        public void AddSubScription<TEvent>(Func<TEvent, Task> action) where TEvent : TransportTray
        {
            _subcriptionManager.AddSubScription<TEvent>(action);
        }

        public void AddSubScription<TEvent>(IHandlerFactory factory) where TEvent : TransportTray
        {
            _subcriptionManager.AddSubScription<TEvent>(factory);
        }

        public void RemoveDynamicSubscription(string eventName, Func<dynamic, Task> action)
        {
            _subcriptionManager.RemoveDynamicSubscription(eventName, action);
        }

        public void RemoveSubscription<TEvent>(Func<TEvent, Task> action) where TEvent : TransportTray
        {
            _subcriptionManager.RemoveSubscription<TEvent>(action);
        }

        public void Publish(TransportTray @event)
        {
            var asyncTask = ProcessEvent(_subcriptionManager.GetEventName(@event.GetType()), JsonConvert.SerializeObject(@event));
        }

        /// <summary>
        /// It's for Unit test
        /// </summary>
        /// <param name="event"></param>
        public void PublishSync(TransportTray @event)
        {
            var asyncTask = ProcessEvent(_subcriptionManager.GetEventName(@event.GetType()), JsonConvert.SerializeObject(@event));
            asyncTask.Wait();
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            var handlerFactories = _subcriptionManager.GetHandlerFactoriesForEvent(eventName);
            foreach (var aHandlerFactory in handlerFactories)
            {
                try
                {
                    var aHandler = aHandlerFactory.GetHandler();
                    if (aHandler is IDynamicEventHandler)
                    {
                        dynamic eventData = JObject.Parse(message);
                        await ((IDynamicEventHandler)aHandler).HandleAsync(eventData);
                    }
                    else
                    {
                        var subscriptionParamType = aHandlerFactory.GetLocalEventType();
                        if (subscriptionParamType != null)
                        {
                            var integrationEvent = JsonConvert.DeserializeObject(message, subscriptionParamType);
                            var concreteType = typeof(IEventHandler<>).MakeGenericType(subscriptionParamType);
                            await (Task)concreteType.GetMethod("HandleAsync").Invoke(aHandler, new object[] { integrationEvent });
                        }
                    }

                    aHandlerFactory.ReleaseHandler(aHandler);
                }
                catch (Exception ex)
                {
                    Logger.Debug("触发事件总线时发生错误。", ex);
                }
            }
        }

        #endregion

        #region Req/Resp

        public void AddRequestHandler(Type eventType, IHandlerFactory factory)
        {
            _requestHandlerManager.AddRequestHandler(eventType, factory);
        }

        public void AddRequestHandler<TEvent>(IHandlerFactory factory)
            where TEvent : TransportTray
        {
            _requestHandlerManager.AddRequestHandler<TEvent>(factory);
        }

        public void AddRequestHandler<TEvent>(Func<TEvent, IRequestHandlerContext, Task> action)
            where TEvent : TransportTray
        {
            _requestHandlerManager.AddRequestHandler<TEvent>(action);
        }

        public void AddDynamicRequestHandler(string eventName, Func<dynamic, IRequestHandlerContext, Task> action)
        {
            _requestHandlerManager.AddDynamicRequestHandler(eventName, action);
        }


        public void RemoveRequestHandler<TEvent>(Func<TEvent, IRequestHandlerContext, Task> action)
            where TEvent : TransportTray
        {
            _requestHandlerManager.RemoveRequestHandler(action);
        }
        public void RemoveDynamicRequestHandler(string eventName, Func<dynamic, IRequestHandlerContext, Task> action)
        {
            _requestHandlerManager.RemoveDynamicRequestHandler(eventName, action);
        }

        public T Send<T>(TransportTray @event)
        {
            var asyncTask = ProcessRequest<T>(_requestHandlerManager.GetRequestName(@event.GetType()), JsonConvert.SerializeObject(@event));
            asyncTask.ConfigureAwait(false);
            return asyncTask.Result;
        }

        public async Task<T> SendAsync<T>(TransportTray @event)
        {
            return await ProcessRequest<T>(_requestHandlerManager.GetRequestName(@event.GetType()), JsonConvert.SerializeObject(@event));
        }

        private async Task<T> ProcessRequest<T>(string eventName, string message)
        {
            var handlerFactories = _requestHandlerManager.GetHandlerFactoriesForRequest(eventName);
            var requestContext = new RequestHandlerContext();
            foreach (var aHandlerFactory in handlerFactories)
            {
                if (requestContext.IsEnd)
                    break;
                try
                {
                    var aHandler = aHandlerFactory.GetHandler();
                    if (aHandler is IDynamicRequestHandler)
                    {
                        dynamic eventData = JObject.Parse(message);
                        await ((IDynamicRequestHandler)aHandler).HandleAsync(eventData, requestContext);
                    }
                    else
                    {
                        var requestParamType = aHandlerFactory.GetLocalEventType();
                        if (requestParamType != null)
                        {
                            var integrationEvent = JsonConvert.DeserializeObject(message, requestParamType);
                            var concreteType = typeof(IRequestHandler<>).MakeGenericType(requestParamType);
                            await (Task)concreteType.GetMethod("HandleAsync").Invoke(aHandler, new object[] { integrationEvent, requestContext });
                        }
                    }

                    aHandlerFactory.ReleaseHandler(aHandler);
                }
                catch (Exception ex)
                {
                    Logger.Debug("发送请求事件时发生错误。", ex);
                }
            }

            return (T)requestContext.Response;
        }
        #endregion
    }
}
