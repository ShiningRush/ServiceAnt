using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiBan.Common.BaseAbpModule.Events.Abstractions
{
    public interface IAddRequestHandler
    {
        void AddRequestHandler(Type eventType, IHandlerFactory factory);

        void AddRequestHandler<TEvent>(IHandlerFactory factory)
            where TEvent : IntegrationEvent;

        void AddRequestHandler<TEvent>(Func<TEvent, IRequestHandlerContext, Task> action)
            where TEvent : IntegrationEvent;

        void AddDynamicRequestHandler(string eventName, Func<dynamic, IRequestHandlerContext, Task> action);


        void RemoveRequestHandler<TEvent>(Func<TEvent, IRequestHandlerContext, Task> action)
            where TEvent : IntegrationEvent;
        void RemoveDynamicRequestHandler(string eventName, Func<dynamic, IRequestHandlerContext, Task> action);
    }
}
