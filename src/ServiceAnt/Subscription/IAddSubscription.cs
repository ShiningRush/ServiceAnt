using ServiceAnt.Handler;
using System;
using System.Threading.Tasks;

namespace ServiceAnt.Subscription
{
    public interface IAddSubscription
    {
        void AddSubScription(Type eventType, IHandlerFactory factory);

        void AddSubScription<TEvent>(IHandlerFactory factory)
            where TEvent : TransportTray;

        void AddSubScription<TEvent>(Func<TEvent, Task> action)
            where TEvent : TransportTray;

        void AddDynamicSubScription(string eventName, Func<dynamic, Task> action);


        void RemoveSubscription<TEvent>(Func<TEvent, Task> action)
            where TEvent : TransportTray;
        void RemoveDynamicSubscription(string eventName, Func<dynamic, Task> action);
    }
}
