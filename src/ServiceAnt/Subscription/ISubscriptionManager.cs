using ServiceAnt.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.Subscription
{
    public interface ISubscriptionManager : IAddSubscription
    {
        List<IHandlerFactory> GetHandlerFactoriesForEvent(string eventName);
        List<IHandlerFactory> GetHandlerFactoriesForEvent(TransportTray @event);

        string GetEventName(Type aType);
    }
}
