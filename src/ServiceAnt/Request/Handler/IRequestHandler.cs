using ServiceAnt.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.Request.Handler
{
    public interface IRequestHandler : IHandler
    {
    }

    public interface IRequestHandler<TEvent> : IRequestHandler where TEvent : TransportTray
    {
        Task HandleAsync(TEvent param, IRequestHandlerContext handlerContext);
    }

    public interface IDynamicRequestHandler : IRequestHandler
    {
        Task HandleAsync(dynamic param, IRequestHandlerContext handlerContext);
    }
}
