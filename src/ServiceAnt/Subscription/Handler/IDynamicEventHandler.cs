using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.Handler.Subscription.Handler
{
    public interface IDynamicEventHandler : IHandler
    {
        Task HandleAsync(dynamic param);
    }
}
