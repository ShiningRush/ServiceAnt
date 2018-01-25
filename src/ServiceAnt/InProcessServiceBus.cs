using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceAnt.Base;
using ServiceAnt.Handler.Request;
using ServiceAnt.Handler.Subscription.Handler;
using ServiceAnt.Infrastructure.Dependency;
using ServiceAnt.Request;
using ServiceAnt.Request.Handler;
using ServiceAnt.Subscription;
using ServiceAnt.Subscription.Handler;
using System;
using System.Threading.Tasks;

namespace ServiceAnt
{
    /// <summary>
    /// The implement that work in-process
    /// </summary>
    public class InProcessServiceBus : IServiceBus
    {
        private readonly ISubscriptionManager _subcriptionManager;
        private readonly IRequestHandlerManager _requestHandlerManager;
        private readonly IIocResolver _iocResolver;

        /// <summary>
        /// Use to log message of bus
        /// </summary>
        public event LogBusMessage OnLogBusMessage;

        private static InProcessServiceBus _defaultInstance;
        private static object _lock = new object();

        /// <summary>
        /// Default Instance
        /// </summary>
        public static InProcessServiceBus Default
        {
            get
            {
                if (_defaultInstance == null)
                {
                    lock (_lock)
                    {
                        if (_defaultInstance == null)
                            _defaultInstance = new InProcessServiceBus();
                    }
                }

                return _defaultInstance;
            }
        }

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
        /// <param name="iocResolver"></param>
        public InProcessServiceBus(ISubscriptionManager subcriptionManager, IRequestHandlerManager requestHandlerManager, IIocResolver iocResolver)
        {
            _subcriptionManager = subcriptionManager;
            _requestHandlerManager = requestHandlerManager;
            _iocResolver = iocResolver;
            _defaultInstance = this;
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
        public void AddSubscription<TEvent>(Func<TEvent, Task> action) where TEvent : IEventTrigger
        {
            _subcriptionManager.AddSubscription<TEvent>(action);
        }

        /// <summary>
        /// Add Subscription for event by factory
        /// </summary>
        /// <typeparam name="TEvent">The event must inherit TransportTray </typeparam>
        /// <param name="factory"></param>s
        public void AddSubscription<TEvent>(IHandlerFactory factory) where TEvent : IEventTrigger
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
        public void RemoveSubscription<TEvent>(Func<TEvent, Task> action) where TEvent : IEventTrigger
        {
            _subcriptionManager.RemoveSubscription<TEvent>(action);
        }

        /// <summary>
        /// Publish a event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public Task Publish(IEventTrigger @event)
        {
            return Publish(@event, new TriggerOption());
        }

        /// <summary>
        /// Publish a event
        /// </summary>
        /// <param name="event"></param>
        /// <param name="triggerOption"></param>
        /// <returns></returns>
        public Task Publish(IEventTrigger @event, TriggerOption triggerOption)
        {
            return ProcessEvent(_subcriptionManager.GetEventName(@event.GetType()), JsonConvert.SerializeObject(@event), triggerOption);
        }

        /// <summary>
        /// Publish a event sync
        /// </summary>
        /// <param name="event"></param>
        public void PublishSync(IEventTrigger @event)
        {
            var asyncTask = Publish(@event, new TriggerOption());
            asyncTask.Wait();
        }

        private async Task ProcessEvent(string eventName, string message, TriggerOption triggerOption)
        {
            var handlerFactories = _subcriptionManager.GetHandlerFactoriesForEvent(eventName);
            foreach (var aHandlerFactory in handlerFactories)
            {
                try
                {
                    SetTheIocResolverIfIocHandlerFactory(aHandlerFactory);
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
                    LogMessage( LogLevel.ERROR, "There has caught a error when publishing event.", ex);
                    if (!triggerOption.IsIgnoreException)
                        throw ex;
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
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="factory"></param>
        public void AddRequestHandler<TRequest>(IHandlerFactory factory)
            where TRequest : IRequestTrigger
        {
            _requestHandlerManager.AddRequestHandler<TRequest>(factory);
        }

        /// <summary>
        /// Register handler with delegate
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="action"></param>
        public void AddRequestHandler<TRequest>(Func<TRequest, IRequestHandlerContext, Task> action)
            where TRequest : IRequestTrigger
        {
            _requestHandlerManager.AddRequestHandler<TRequest>(action);
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
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="action"></param>
        public void RemoveRequestHandler<TRequest>(Func<TRequest, IRequestHandlerContext, Task> action)
            where TRequest : IRequestTrigger
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
        public T Send<T>(IRequestTrigger @event)
        {
            return Send<T>(@event, new TriggerOption());
        }

        /// <summary>
        /// Send a request sync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <param name="triggerOption"></param>
        /// <returns></returns>
        public T Send<T>(IRequestTrigger @event, TriggerOption triggerOption)
        {
            var asyncTask = SendAsync<T>(@event, triggerOption);
            asyncTask.ConfigureAwait(false);
            return asyncTask.Result;
        }

        /// <summary>
        /// Send a request async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <returns></returns>
        public Task<T> SendAsync<T>(IRequestTrigger @event)
        {
            return SendAsync<T>(@event, new TriggerOption(false));
        }

        /// <summary>
        /// Send a request async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <param name="triggerOption"></param>
        /// <returns></returns>
        public Task<T> SendAsync<T>(IRequestTrigger @event, TriggerOption triggerOption)
        {
            return ProcessRequest<T>(_requestHandlerManager.GetRequestName(@event.GetType()), JsonConvert.SerializeObject(@event), triggerOption);
        }

        private async Task<T> ProcessRequest<T>(string eventName, string message, TriggerOption triggerOption)
        {
            var handlerFactories = _requestHandlerManager.GetHandlerFactoriesForRequest(eventName);
            var requestContext = new RequestHandlerContext();
            if (handlerFactories.Count == 0)
                return default(T);

            foreach (var aHandlerFactory in handlerFactories)
            {
                if (requestContext.IsEnd)
                    break;
                SetTheIocResolverIfIocHandlerFactory(aHandlerFactory);
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
                    LogMessage(LogLevel.ERROR, "There has raised a error when send request.", ex);
                    if (!triggerOption.IsIgnoreException)
                        throw ex;
                }
            }

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(requestContext.Response));
        }

#endregion

        private void LogMessage(LogLevel type, string value, Exception ex)
        {
            OnLogBusMessage?.Invoke(type, value, ex);
        }

        private void SetTheIocResolverIfIocHandlerFactory(IHandlerFactory aHandlerFactory)
        {
            var iocHandlerFactory = aHandlerFactory as IocHandlerFactory;
            if (iocHandlerFactory != null)
                iocHandlerFactory.SetIocResolver(_iocResolver);
        }
    }
}
