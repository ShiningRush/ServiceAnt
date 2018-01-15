using ServiceAnt.Base;
using System;
using System.Threading.Tasks;

namespace ServiceAnt.Request.Handler
{
    /// <summary>
    /// The container of request handler registing with delegate
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public class ActionRequestHandler<TRequest> : IRequestHandler<TRequest> where TRequest : IRequestTrigger
    {
        private Func<TRequest, IRequestHandlerContext, Task> _action;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="action">register delagate</param>
        public ActionRequestHandler(Func<TRequest, IRequestHandlerContext, Task> action)
        {
            _action = action;
        }

        /// <summary>
        /// Handle request async
        /// </summary>
        /// <param name="param">request</param>
        /// <param name="handlerContext">handler context</param>
        /// <returns></returns>
        public Task HandleAsync(TRequest param, IRequestHandlerContext handlerContext)
        {
            return _action(param, handlerContext);
        }

        /// <summary>
        /// Compare the registered delegate is same with inpurt delegate
        /// </summary>
        /// <param name="compareAction">inpurt delegate</param>
        /// <returns></returns>
        public bool IsSame(Func<TRequest, IRequestHandlerContext, Task> compareAction)
        {
            return compareAction == _action;
        }
    }

    /// <summary>
    /// The container of request handler registing with delegate having dynamic request parameter
    /// </summary>
    public class ActionRequestHandler : IDynamicRequestHandler
    {
        private Func<dynamic, IRequestHandlerContext, Task> _action;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="action"></param>
        public ActionRequestHandler(Func<dynamic, IRequestHandlerContext, Task> action)
        {
            _action = action;
        }

        /// <summary>
        /// Handle request async
        /// </summary>
        /// <param name="param">dynamic request parameter</param>
        /// <param name="handlerContext">handler context</param>
        /// <returns></returns>
        public Task HandleAsync(dynamic param, IRequestHandlerContext handlerContext)
        {
            return _action(param, handlerContext);
        }

        /// <summary>
        /// Compare the registered delegate is same with inpurt delegate
        /// </summary>
        /// <param name="compareAction">inpurt delegate</param>
        /// <returns></returns>
        public bool IsSame(Func<dynamic, IRequestHandlerContext, Task> compareAction)
        {
            return compareAction == _action;
        }
    }
}
