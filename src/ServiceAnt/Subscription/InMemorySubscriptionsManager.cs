using ServiceAnt.Common.Extension;
using ServiceAnt.Handler;
using ServiceAnt.Handler.Subscription.Handler;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceAnt.Subscription
{
    /// <summary>
    /// InMemorySubscriptionsManager
    /// </summary>
    public class InMemorySubscriptionsManager : ISubscriptionManager
    {
        private readonly ConcurrentDictionary<string, List<IHandlerFactory>> _handlerFactories;

        /// <summary>
        /// Constructor
        /// </summary>
        public InMemorySubscriptionsManager()
        {
            _handlerFactories = new ConcurrentDictionary<string, List<IHandlerFactory>>();
        }

        /// <summary>
        /// Add Subscription for event by delegate with dynamic parameter
        /// </summary>
        /// <param name="eventName">the name of event(the class name of event type)</param>
        /// <param name="action">Handler delegate</param>
        public void AddDynamicSubscription(string eventName, Func<dynamic, Task> action)
        {
            var eventHandler = new ActionEventHandler(action);

            DoAddSubscription(new SingletonHandlerFactory(eventHandler), eventName: eventName);
        }

        /// <summary>
        /// Add Subscription for event by factory
        /// </summary>
        /// <param name="eventType">The type of event must inherit TransportTray</param>
        /// <param name="factory">The factory of handler</param>
        public void AddSubscription(Type eventType, IHandlerFactory factory)
        {
            DoAddSubscription(factory, eventType);
        }

        /// <summary>
        /// Add Subscription for event by delegate
        /// </summary>
        /// <typeparam name="TEvent">The event must inherit TransportTray</typeparam>
        /// <param name="action">Handler delegate</param>
        public void AddSubscription<TEvent>(Func<TEvent, Task> action) where TEvent : ITrigger
        {
            var eventHandler = new ActionEventHandler<TEvent>(action);

            AddSubscription<TEvent>(new SingletonHandlerFactory(eventHandler, typeof(TEvent)));
        }

        /// <summary>
        /// Add Subscription for event by factory
        /// </summary>
        /// <typeparam name="TEvent">The event must inherit TransportTray </typeparam>
        /// <param name="factory"></param>
        public void AddSubscription<TEvent>(IHandlerFactory factory) where TEvent : ITrigger
        {
            DoAddSubscription(factory, typeof(TEvent));
        }

        /// <summary>
        /// Get all factory of handler by evetname
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public List<IHandlerFactory> GetHandlerFactoriesForEvent(string eventName)
        {
            return GetOrCreateHandlerFactories(eventName);
        }

        /// <summary>
        /// Get all factory of handler by event object
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public List<IHandlerFactory> GetHandlerFactoriesForEvent(ITrigger @event)
        {
            return GetOrCreateHandlerFactories(GetEventName(@event.GetType()));
        }

        /// <summary>
        /// Remove subscription from servicebus
        /// </summary>
        /// <param name="eventName">the name of event(the class name of event type)</param>
        /// <param name="action">Handler delegate</param>
        public void RemoveDynamicSubscription(string eventName, Func<dynamic, Task> action)
        {
            GetOrCreateHandlerFactories(eventName)
                .Locking(factories =>
                {
                    factories.RemoveAll(aFactory =>
                    {
                        var singleInstanceFactory = aFactory as SingletonHandlerFactory;
                        if (singleInstanceFactory == null)
                        {
                            return false;
                        }

                        var actionHandler = singleInstanceFactory.GetHandler() as ActionEventHandler;
                        if (actionHandler == null)
                        {
                            return false;
                        }

                        return actionHandler.IsSame(action);
                    });
                });
        }

        /// <summary>
        /// Remove subscription from servicebus
        /// </summary>
        /// <typeparam name="TEvent">The event must inherit TransportTray</typeparam>
        /// <param name="action">Handler delegate</param>
        public void RemoveSubscription<TEvent>(Func<TEvent, Task> action) where TEvent : ITrigger
        {
            GetOrCreateHandlerFactories(typeof(TEvent).Name)
                .Locking(factories =>
                {
                    factories.RemoveAll(aFactory =>
                    {
                        var singleInstanceFactory = aFactory as SingletonHandlerFactory;
                        if (singleInstanceFactory == null)
                        {
                            return false;
                        }

                        var actionHandler = singleInstanceFactory.GetHandler() as ActionEventHandler<TEvent>;
                        if (actionHandler == null)
                        {
                            return false;
                        }

                        return actionHandler.IsSame(action);
                    });
                });
        }

        private List<IHandlerFactory> GetOrCreateHandlerFactories(string eventName)
        {
            return _handlerFactories.GetOrAdd(eventName, new List<IHandlerFactory>());
        }

        private void DoAddSubscription(IHandlerFactory factory, Type eventType = null, string eventName = null)
        {
            if (eventType != null)
            {
                eventName = GetEventName(eventType);
            }

            if (!string.IsNullOrEmpty(eventName))
            {
                GetOrCreateHandlerFactories(eventName)
                    .Locking(factories => factories.Add(factory));
            }
        }

        private void DoRemoveSubscription(IHandlerFactory factory, Type eventType = null, string eventName = null)
        {

        }

        /// <summary>
        /// Get event name by event type
        /// </summary>
        /// <param name="aType"></param>
        /// <returns></returns>
        public string GetEventName(Type aType)
        {
            if (aType.IsGenericType)
            {
                return aType.GetGenericTypeDefinition().Name + "," + String.Join(",", aType.GetGenericArguments().Select(p => p.Name));
            }
            else
            {
                return aType.Name;
            }
        }
    }
}
