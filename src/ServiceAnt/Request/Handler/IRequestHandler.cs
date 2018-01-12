using ServiceAnt.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.Request.Handler
{
    /// <summary>
    /// The inerface of handling request
    /// </summary>
    public interface IRequestHandler : IHandler
    {
    }

    /// <summary>
    /// The interface of handling request
    /// </summary>
    /// <typeparam name="TRequest">request parameter</typeparam>
    public interface IRequestHandler<TRequest> : IRequestHandler where TRequest : ITrigger
    {
        /// <summary>
        /// Handle request async
        /// </summary>
        /// <param name="param">request</param>
        /// <param name="handlerContext">handler context</param>
        /// <returns></returns>
        Task HandleAsync(TRequest param, IRequestHandlerContext handlerContext);
    }

    /// <summary>
    /// The interface of handling request with dynamic parameter
    /// </summary>
    public interface IDynamicRequestHandler : IRequestHandler
    {
        /// <summary>
        /// Handle request async
        /// </summary>
        /// <param name="param">dynamic request parameter</param>
        /// <param name="handlerContext">handler context</param>
        /// <returns></returns>
        Task HandleAsync(dynamic param, IRequestHandlerContext handlerContext);
    }
}
