using ServiceAnt.Request.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.Handler.Request
{
    /// <summary>
    /// It uead to register request handler
    /// </summary>
    public interface IAddRequestHandler
    {
        /// <summary>
        /// Register handler with handler factory
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="factory"></param>
        void AddRequestHandler(Type eventType, IHandlerFactory factory);

        /// <summary>
        /// Register handler with handler factory
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="factory"></param>
        void AddRequestHandler<TEvent>(IHandlerFactory factory)
            where TEvent : ITrigger;

        /// <summary>
        /// Register handler with delegate
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="action"></param>
        void AddRequestHandler<TEvent>(Func<TEvent, IRequestHandlerContext, Task> action)
            where TEvent : ITrigger;

        /// <summary>
        /// Register dynamic handler with delegate
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="action"></param>
        void AddDynamicRequestHandler(string eventName, Func<dynamic, IRequestHandlerContext, Task> action);

        /// <summary>
        /// Remove handler by event type
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="action"></param>
        void RemoveRequestHandler<TEvent>(Func<TEvent, IRequestHandlerContext, Task> action)
            where TEvent : ITrigger;

        /// <summary>
        /// Remove dynamic handler by eventName
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="action"></param>
        void RemoveDynamicRequestHandler(string eventName, Func<dynamic, IRequestHandlerContext, Task> action);
    }
}
