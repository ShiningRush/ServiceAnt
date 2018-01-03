﻿using ServiceAnt.Common.Extension;
using ServiceAnt.Request.Handler;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceAnt.Handler.Request
{
    /// <summary>
    /// It save the handlers in memory
    /// </summary>
    public class InMemoryRequestHandlerManager : IRequestHandlerManager
    {
        private readonly ConcurrentDictionary<string, List<IHandlerFactory>> _handlerFactories;

        /// <summary>
        /// Constructor
        /// </summary>
        public InMemoryRequestHandlerManager()
        {
            _handlerFactories = new ConcurrentDictionary<string, List<IHandlerFactory>>();
        }

        /// <summary>
        /// Register dynamic handler with delegate
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="action"></param>
        public void AddDynamicRequestHandler(string eventName, Func<dynamic, IRequestHandlerContext, Task> action)
        {
            var eventHandler = new ActionRequestHandler(action);

            DoAddRequestHandler(new SingletonHandlerFactory(eventHandler), eventName: eventName);
        }

        /// <summary>
        /// Register handler with handler factory
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="factory"></param>
        public void AddRequestHandler(Type eventType, IHandlerFactory factory)
        {
            DoAddRequestHandler(factory, eventType);
        }

        /// <summary>
        /// Register handler with delegate
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="action"></param>
        public void AddRequestHandler<TEvent>(Func<TEvent, IRequestHandlerContext, Task> action) where TEvent : TransportTray
        {
            var eventHandler = new ActionRequestHandler<TEvent>(action);

            AddRequestHandler<TEvent>(new SingletonHandlerFactory(eventHandler, typeof(TEvent)));
        }

        /// <summary>
        /// Register handler with handler factory
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="factory"></param>
        public void AddRequestHandler<TEvent>(IHandlerFactory factory) where TEvent : TransportTray
        {
            DoAddRequestHandler(factory, typeof(TEvent));
        }

        /// <summary>
        /// Get the all handler factory of a request
        /// </summary>
        /// <param name="request">request object</param>
        /// <returns></returns>
        public List<IHandlerFactory> GetHandlerFactoriesForRequest(TransportTray request)
        {
            return GetOrCreateHandlerFactories(GetRequestName(request.GetType()));
        }
        /// <summary>
        /// Get the all handler factory of a request
        /// </summary>
        /// <param name="requestName">request name</param>
        /// <returns></returns>

        public List<IHandlerFactory> GetHandlerFactoriesForRequest(string requestName)
        {
            return GetOrCreateHandlerFactories(requestName);
        }

        /// <summary>
        /// Get the request name by TransportTray type
        /// </summary>
        /// <param name="aType">the class inherited TransportTray</param>
        /// <returns></returns>
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

        /// <summary>
        /// Register dynamic handler with delegate
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="action"></param>
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

        /// <summary>
        /// Remove handler by event type
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="action"></param>
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
