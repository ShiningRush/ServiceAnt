using ServiceAnt.Handler.Request.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.Handler.Request
{
    public interface IAddRequestHandler
    {
        void AddRequestHandler(Type eventType, IHandlerFactory factory);

        void AddRequestHandler<TEvent>(IHandlerFactory factory)
            where TEvent : TransportTray;

        void AddRequestHandler<TEvent>(Func<TEvent, IRequestHandlerContext, Task> action)
            where TEvent : TransportTray;

        void AddDynamicRequestHandler(string eventName, Func<dynamic, IRequestHandlerContext, Task> action);


        void RemoveRequestHandler<TEvent>(Func<TEvent, IRequestHandlerContext, Task> action)
            where TEvent : TransportTray;
        void RemoveDynamicRequestHandler(string eventName, Func<dynamic, IRequestHandlerContext, Task> action);
    }
}
