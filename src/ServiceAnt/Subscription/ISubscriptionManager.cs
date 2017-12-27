using ServiceAnt.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.Subscription
{
    /// <summary>
    /// SubscriptionManager
    /// </summary>
    public interface ISubscriptionManager : IAddSubscription
    {
        /// <summary>
        /// Get all factory of handler by evetname
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        List<IHandlerFactory> GetHandlerFactoriesForEvent(string eventName);

        /// <summary>
        /// Get all factory of handler by event object
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        List<IHandlerFactory> GetHandlerFactoriesForEvent(TransportTray @event);

        /// <summary>
        /// Get event name by event type
        /// </summary>
        /// <param name="aType"></param>
        /// <returns></returns>
        string GetEventName(Type aType);
    }
}
