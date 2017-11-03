using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YiBan.Common.BaseAbpModule.Events.Abstractions;

namespace YiBan.Common.BaseAbpModule.Events
{
    public class ActionEventHandler<TEvent> : IEventHandler<TEvent> where TEvent : IntegrationEvent
    {
        private Func<TEvent, Task> _action;

        public ActionEventHandler(Func<TEvent, Task>  action)
        {
            _action = action;
        }

        public async Task HandleAsync(TEvent param)
        {
            await _action(param);
        }

        public bool IsSame(Func<TEvent, Task> compareAction)
        {
            return compareAction == _action;
        }
    }

    public class ActionEventHandler : IDynamicEventHandler
    {
        private Func<dynamic, Task> _action;

        public ActionEventHandler(Func<dynamic, Task> action)
        {
            _action = action;
        }

        public async Task HandleAsync(dynamic param)
        {
            await _action(param);
        }

        public bool IsSame(Func<dynamic, Task> compareAction)
        {
            return compareAction == _action;
        }
    }
}
