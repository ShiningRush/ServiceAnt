using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiBan.Common.BaseAbpModule.Events.Abstractions
{
    public interface IEventBus : IAddSubscription, IAddRequestHandler
    {
        void Publish(IntegrationEvent @event);

        T Send<T>(IntegrationEvent @event);

        Task<T> SendAsync<T>(IntegrationEvent @event);
    }
}
