using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiBan.Common.BaseAbpModule.Events.Abstractions
{
    public interface IAddSubscription
    {
        void AddSubScription(Type eventType, IHandlerFactory factory);

        void AddSubScription<TEvent>(IHandlerFactory factory)
            where TEvent : IntegrationEvent;

        void AddSubScription<TEvent>(Func<TEvent, Task> action)
            where TEvent : IntegrationEvent;

        void AddDynamicSubScription(string eventName, Func<dynamic, Task> action);


        void RemoveSubscription<TEvent>(Func<TEvent, Task> action)
            where TEvent : IntegrationEvent;
        void RemoveDynamicSubscription(string eventName, Func<dynamic, Task> action);
    }
}
