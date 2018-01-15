using ServiceAnt.Base;
using System;
using System.Threading.Tasks;

namespace ServiceAnt.Subscription
{
    /// <summary>
    /// AddSubscription
    /// </summary>
    public interface IAddSubscription
    {
        /// <summary>
        /// Add Subscription for event by factory
        /// </summary>
        /// <param name="eventType">The type of event must inherit TransportTray</param>
        /// <param name="factory">The factory of handler</param>
        void AddSubscription(Type eventType, IHandlerFactory factory);

        /// <summary>
        /// Add Subscription for event by factory
        /// </summary>
        /// <typeparam name="TEvent">The event must inherit TransportTray </typeparam>
        /// <param name="factory"></param>
        void AddSubscription<TEvent>(IHandlerFactory factory)
            where TEvent : IEventTrigger;

        /// <summary>
        /// Add Subscription for event by delegate
        /// </summary>
        /// <typeparam name="TEvent">The event must inherit TransportTray</typeparam>
        /// <param name="action">Handler delegate</param>
        void AddSubscription<TEvent>(Func<TEvent, Task> action)
            where TEvent : IEventTrigger;

        /// <summary>
        /// Add Subscription for event by delegate with dynamic parameter
        /// </summary>
        /// <param name="eventName">the name of event(the class name of event type)</param>
        /// <param name="action">Handler delegate</param>
        void AddDynamicSubscription(string eventName, Func<dynamic, Task> action);

        /// <summary>
        /// Remove subscription from servicebus
        /// </summary>
        /// <typeparam name="TEvent">The event must inherit TransportTray</typeparam>
        /// <param name="action">Handler delegate</param>
        void RemoveSubscription<TEvent>(Func<TEvent, Task> action)
            where TEvent : IEventTrigger;

        /// <summary>
        /// Remove subscription from servicebus
        /// </summary>
        /// <param name="eventName">the name of event(the class name of event type)</param>
        /// <param name="action">Handler delegate</param>
        void RemoveDynamicSubscription(string eventName, Func<dynamic, Task> action);
    }
}
