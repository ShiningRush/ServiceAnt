using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.Handler.Subscription.Handler
{
    public interface IEventHandler<TEvent> : IHandler where TEvent : TransportTray
    {
        Task HandleAsync(TEvent param);
    }
}
