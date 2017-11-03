using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YiBan.Common.BaseAbpModule.Events.Abstractions;

namespace YiBan.Common.BaseAbpModule.Events
{
    public class ActionRequestHandler<TEvent> : IRequestHandler<TEvent> where TEvent : IntegrationEvent
    {
        private Func<TEvent, IRequestHandlerContext, Task> _action;

        public ActionRequestHandler(Func<TEvent, IRequestHandlerContext, Task> action)
        {
        }

        public Task HandleAsync(TEvent param, IRequestHandlerContext handlerContext)
        {
            return _action(param, handlerContext);
        }

        public bool IsSame(Func<TEvent, IRequestHandlerContext, Task> compareAction)
        {
            return compareAction == _action;
        }
    }

    public class ActionRequestHandler : IDynamicRequestHandler
    {
        private Func<dynamic, IRequestHandlerContext, Task> _action;

        public ActionRequestHandler(Func<dynamic, IRequestHandlerContext, Task> action)
        {
            _action = action;
        }

        public Task HandleAsync(dynamic param, IRequestHandlerContext handlerContext)
        {
            return _action(param, handlerContext);
        }

        public bool IsSame(Func<dynamic, IRequestHandlerContext, Task> compareAction)
        {
            return compareAction == _action;
        }
    }
}
