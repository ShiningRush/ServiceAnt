using ServiceAnt.Base;
using ServiceAnt.Request;
using ServiceAnt.Request.Handler;
using System;
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
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="factory"></param>
        void AddRequestHandler<TRequest>(IHandlerFactory factory)
            where TRequest : IRequestTrigger;

        /// <summary>
        /// Register handler with delegate
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="action"></param>
        void AddRequestHandler<TRequest>(Func<TRequest, IRequestHandlerContext, Task> action)
            where TRequest : IRequestTrigger;

        /// <summary>
        /// Register dynamic handler with delegate
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="action"></param>
        void AddDynamicRequestHandler(string eventName, Func<dynamic, IRequestHandlerContext, Task> action);

        /// <summary>
        /// Remove handler by event type
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="action"></param>
        void RemoveRequestHandler<TRequest>(Func<TRequest, IRequestHandlerContext, Task> action)
            where TRequest : IRequestTrigger;

        /// <summary>
        /// Remove dynamic handler by eventName
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="action"></param>
        void RemoveDynamicRequestHandler(string eventName, Func<dynamic, IRequestHandlerContext, Task> action);
    }
}
