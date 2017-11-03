using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiBan.Common.BaseAbpModule.Events.Abstractions
{
    public interface IHandler
    {
    }

    public interface IEventHandler<TEvent> : IHandler where TEvent : IntegrationEvent
    {
        Task HandleAsync(TEvent param);
    }

    public interface IDynamicEventHandler : IHandler
    {
        Task HandleAsync(dynamic param);
    }
}
