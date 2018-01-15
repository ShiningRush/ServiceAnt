using ServiceAnt.Base;
using ServiceAnt.Request;
using System;
using System.Collections.Generic;

namespace ServiceAnt.Handler.Request
{
    /// <summary>
    /// It used to manage request handle function
    /// </summary>
    public interface IRequestHandlerManager : IAddRequestHandler
    {
        /// <summary>
        /// Get the all handler factory of a request
        /// </summary>
        /// <param name="requestName">request name</param>
        /// <returns></returns>
        List<IHandlerFactory> GetHandlerFactoriesForRequest(string requestName);

        /// <summary>
        /// Get the all handler factory of a request
        /// </summary>
        /// <param name="request">request object</param>
        /// <returns></returns>
        List<IHandlerFactory> GetHandlerFactoriesForRequest(IRequestTrigger @request);

        /// <summary>
        /// Get the request name by TransportTray type
        /// </summary>
        /// <param name="aType">the class inherited TransportTray</param>
        /// <returns></returns>
        string GetRequestName(Type aType);
    }
}
