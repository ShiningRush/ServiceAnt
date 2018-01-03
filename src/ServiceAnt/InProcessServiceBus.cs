using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceAnt.Handler;
using ServiceAnt.Handler.Request;
using ServiceAnt.Handler.Subscription.Handler;
using ServiceAnt.Request.Handler;
using ServiceAnt.Subscription;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceAnt
{
    /// <summary>
    /// The implement that work in-process
    /// </summary>
    public class InProcessServiceBus : IServiceBus
    {
        private ISubscriptionManager _subcriptionManager;
        private IRequestHandlerManager _requestHandlerManager;

        /// <summary>
        /// Use to log message of bus
        /// </summary>
        public event Action<string, string, Exception> OnLogBusMessage;

        private static Lazy<InProcessServiceBus> _defaultInstance = new Lazy<InProcessServiceBus>();

        /// <summary>
        /// Default Instance
        /// </summary>
        public static InProcessServiceBus Default => _defaultInstance.Value;

        /// <summary>
        /// Constructor
        /// </summary>
        public InProcessServiceBus()
        {
            _subcriptionManager = new InMemorySubscriptionsManager();
            _requestHandlerManager = new InMemoryRequestHandlerManager();
        }

        /// <summary>
        /// It used to inject mock object
        /// </summary>
        /// <param name="subcriptionManager"></param>
        /// <param name="requestHandlerManager"></param>
        public InProcessServiceBus(ISubscriptionManager subcriptionManager, IRequestHandlerManager requestHandlerManager)
        {
            _subcriptionManager = subcriptionManager;
            _requestHandlerManager = requestHandlerManager;
        }

        #region Pub/Sub

        /// <summary>
        /// Add Subscription for event by delegate with dynamic parameter
        /// </summary>
        /// <param name="eventName">the name of event(the class name of event type)</param>
        /// <param name="action">Handler delegate</param>
        public void AddDynamicSubscription(string eventName, Func<dynamic, Task> action)
        {
            _subcriptionManager.AddDynamicSubscription(eventName, action);
        }

        /// <summary>
        /// Add Subscription for event by factory
        /// </summary>
        /// <param name="eventType">The type of event must inherit TransportTray</param>
        /// <param name="factory">The factory of handler</param>
        public void AddSubscription(Type eventType, IHandlerFactory factory)
        {
            _subcriptionManager.AddSubscription(eventType, factory);
        }

        /// <summary>
        /// Add Subscription for event by delegate
        /// </summary>
        /// <typeparam name="TEvent">The event must inherit TransportTray</typeparam>
        /// <param name="action">Handler delegate</param>
        public void AddSubscription<TEvent>(Func<TEvent, Task> action) where TEvent : TransportTray
        {
            _subcriptionManager.AddSubscription<TEvent>(action);
        }

        /// <summary>
        /// Add Subscription for event by factory
        /// </summary>
        /// <typeparam name="TEvent">The event must inherit TransportTray </typeparam>
        /// <param name="factory"></param>s
        public void AddSubscription<TEvent>(IHandlerFactory factory) where TEvent : TransportTray
        {
            _subcriptionManager.AddSubscription<TEvent>(factory);
        }


        /// <summary>
        /// Remove subscription from servicebus
        /// </summary>
        /// <param name="eventName">the name of event(the class name of event type)</param>
        /// <param name="action">Handler delegate</param>
        public void RemoveDynamicSubscription(string eventName, Func<dynamic, Task> action)
        {
            _subcriptionManager.RemoveDynamicSubscription(eventName, action);
        }

        /// <summary>
        /// Remove subscription from servicebus
        /// </summary>
        /// <typeparam name="TEvent">The event must inherit TransportTray</typeparam>
        /// <param name="action">Handler delegate</param>
        public void RemoveSubscription<TEvent>(Func<TEvent, Task> action) where TEvent : TransportTray
        {
            _subcriptionManager.RemoveSubscription<TEvent>(action);
        }

        /// <summary>
        /// Publish a event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public Task Publish(TransportTray @event)
        {
            var asyncTask = ProcessEvent(_subcriptionManager.GetEventName(@event.GetType()), JsonConvert.SerializeObject(@event));
            return asyncTask;
        }

        /// <summary>
        /// Publish a event sync
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
                    LogMessage( "error", "There has raised a error when publishing event.", ex);
                }
            }
        }

        #endregion

        #region Req/Resp

        /// <summary>
        /// Register handler with handler factory
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="factory"></param>
        public void AddRequestHandler(Type eventType, IHandlerFactory factory)
        {
            _requestHandlerManager.AddRequestHandler(eventType, factory);
        }

        /// <summary>
        /// Register handler with handler factory
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="factory"></param>
        public void AddRequestHandler<TEvent>(IHandlerFactory factory)
            where TEvent : TransportTray
        {
            _requestHandlerManager.AddRequestHandler<TEvent>(factory);
        }

        /// <summary>
        /// Register handler with delegate
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="action"></param>
        public void AddRequestHandler<TEvent>(Func<TEvent, IRequestHandlerContext, Task> action)
            where TEvent : TransportTray
        {
            _requestHandlerManager.AddRequestHandler<TEvent>(action);
        }

        /// <summary>
        /// Register dynamic handler with delegate
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="action"></param>
        public void AddDynamicRequestHandler(string eventName, Func<dynamic, IRequestHandlerContext, Task> action)
        {
            _requestHandlerManager.AddDynamicRequestHandler(eventName, action);
        }

        /// <summary>
        /// Remove handler by event type
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="action"></param>
        public void RemoveRequestHandler<TEvent>(Func<TEvent, IRequestHandlerContext, Task> action)
            where TEvent : TransportTray
        {
            _requestHandlerManager.RemoveRequestHandler(action);
        }

        /// <summary>
        /// Remove dynamic handler by eventName
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="action"></param>
        public void RemoveDynamicRequestHandler(string eventName, Func<dynamic, IRequestHandlerContext, Task> action)
        {
            _requestHandlerManager.RemoveDynamicRequestHandler(eventName, action);
        }

        /// <summary>
        /// Send a request sync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <returns></returns>
        public T Send<T>(TransportTray @event)
        {
            var asyncTask = ProcessRequest<T>(_requestHandlerManager.GetRequestName(@event.GetType()), JsonConvert.SerializeObject(@event));
            asyncTask.ConfigureAwait(false);
            return asyncTask.Result;
        }

        /// <summary>
        /// Send a request async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task<T> SendAsync<T>(TransportTray @event)
        {
            return await ProcessRequest<T>(_requestHandlerManager.GetRequestName(@event.GetType()), JsonConvert.SerializeObject(@event));
        }

        private async Task<T> ProcessRequest<T>(string eventName, string message)
        {
            var handlerFactories = _requestHandlerManager.GetHandlerFactoriesForRequest(eventName);
            var requestContext = new RequestHandlerContext();
            if (handlerFactories.Count == 0)
                return default(T);

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
                    LogMessage("error", "There has raised a error when send request.", ex);
                }
            }

            return (T)requestContext.Response;
        }

#endregion

        private void LogMessage(string type, string value, Exception ex)
        {
            OnLogBusMessage?.Invoke(type, value, ex);
        }
    }
}
