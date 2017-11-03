using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiBan.Common.BaseAbpModule.Events.Abstractions
{
    public interface IRequestHandler : IHandler
    {

    }

    public interface IRequestHandler<TEvent> : IRequestHandler where TEvent : IntegrationEvent
    {
        Task HandleAsync(TEvent param, IRequestHandlerContext handlerContext);
    }

    public interface IDynamicRequestHandler : IRequestHandler
    {
        Task HandleAsync(dynamic param, IRequestHandlerContext handlerContext);
    }
}
