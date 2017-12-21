using ServiceAnt.Common.Extension;
using ServiceAnt.Request.Handler;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceAnt.Handler.Request
{
    public class InMemoryRequestHandlerManager : IRequestHandlerManager
    {
        private readonly ConcurrentDictionary<string, List<IHandlerFactory>> _handlerFactories;

        public InMemoryRequestHandlerManager()
        {
            _handlerFactories = new ConcurrentDictionary<string, List<IHandlerFactory>>();
        }

        public void AddDynamicRequestHandler(string eventName, Func<dynamic, IRequestHandlerContext, Task> action)
        {
            var eventHandler = new ActionRequestHandler(action);

            DoAddRequestHandler(new SingletonHandlerFactory(eventHandler), eventName: eventName);
        }

        public void AddRequestHandler(Type eventType, IHandlerFactory factory)
        {
            DoAddRequestHandler(factory, eventType);
        }

        public void AddRequestHandler<TEvent>(Func<TEvent, IRequestHandlerContext, Task> action) where TEvent : TransportTray
        {
            var eventHandler = new ActionRequestHandler<TEvent>(action);

            AddRequestHandler<TEvent>(new SingletonHandlerFactory(eventHandler, typeof(TEvent)));
        }

        public void AddRequestHandler<TEvent>(IHandlerFactory factory) where TEvent : TransportTray
        {
            DoAddRequestHandler(factory, typeof(TEvent));
        }

        public List<IHandlerFactory> GetHandlerFactoriesForRequest(TransportTray request)
        {
            return GetOrCreateHandlerFactories(GetRequestName(request.GetType()));
        }

        public List<IHandlerFactory> GetHandlerFactoriesForRequest(string requestName)
        {
            return GetOrCreateHandlerFactories(requestName);
        }

        public string GetRequestName(Type aType)
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

        public void RemoveDynamicRequestHandler(string eventName, Func<dynamic, IRequestHandlerContext, Task> action)
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

                        var actionHandler = singleInstanceFactory.GetHandler() as ActionRequestHandler;
                        if (actionHandler == null)
                        {
                            return false;
                        }

                        return actionHandler.IsSame(action);
                    });
                });
        }

        public void RemoveRequestHandler<TEvent>(Func<TEvent, IRequestHandlerContext, Task> action) where TEvent : TransportTray
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

                        var actionHandler = singleInstanceFactory.GetHandler() as ActionRequestHandler<TEvent>;
                        if (actionHandler == null)
                        {
                            return false;
                        }

                        return actionHandler.IsSame(action);
                    });
                });
        }

        private void DoAddRequestHandler(IHandlerFactory factory, Type eventType = null, string eventName = null)
        {
            if (eventType != null)
            {
                eventName = GetRequestName(eventType);
            }

            if (!string.IsNullOrEmpty(eventName))
            {
                GetOrCreateHandlerFactories(eventName)
                    .Locking(factories => factories.Add(factory));
            }
        }

        private List<IHandlerFactory> GetOrCreateHandlerFactories(string eventName)
        {
            return _handlerFactories.GetOrAdd(eventName, new List<IHandlerFactory>());
        }
    }
}
