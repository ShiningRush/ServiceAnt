using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiBan.Common.BaseAbpModule.Events.Abstractions
{
    public interface ISubscriptionManager : IAddSubscription
    {
        List<IHandlerFactory> GetHandlerFactoriesForEvent(string eventName);
        List<IHandlerFactory> GetHandlerFactoriesForEvent(IntegrationEvent @event);

        string GetEventName(Type aType);
    }
}
