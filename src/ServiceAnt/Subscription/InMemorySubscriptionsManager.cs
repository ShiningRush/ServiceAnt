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
    public class InMemorySubscriptionsManager : ISubscriptionManager
    {
        private readonly ConcurrentDictionary<string, List<IHandlerFactory>> _handlerFactories;

        public InMemorySubscriptionsManager()
        {
            _handlerFactories = new ConcurrentDictionary<string, List<IHandlerFactory>>();
        }

        public void AddDynamicSubScription(string eventName, Func<dynamic, Task> action)
        {
            var eventHandler = new ActionEventHandler(action);

            DoAddSubscription(new SingletonHandlerFactory(eventHandler), eventName: eventName);
        }

        public void AddSubScription(Type eventType, IHandlerFactory factory)
        {
            DoAddSubscription(factory, eventType);
        }

        public void AddSubScription<TEvent>(Func<TEvent, Task> action) where TEvent : TransportTray
        {
            var eventHandler = new ActionEventHandler<TEvent>(action);

            AddSubScription<TEvent>(new SingletonHandlerFactory(eventHandler, typeof(TEvent)));
        }

        public void AddSubScription<TEvent>(IHandlerFactory factory) where TEvent : TransportTray
        {
            DoAddSubscription(factory, typeof(TEvent));
        }

        public List<IHandlerFactory> GetHandlerFactoriesForEvent(string eventName)
        {
            return GetOrCreateHandlerFactories(eventName);
        }

        public List<IHandlerFactory> GetHandlerFactoriesForEvent(TransportTray @event)
        {
            return GetOrCreateHandlerFactories(GetEventName(@event.GetType()));
        }

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

        public void RemoveSubscription<TEvent>(Func<TEvent, Task> action) where TEvent : TransportTray
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
