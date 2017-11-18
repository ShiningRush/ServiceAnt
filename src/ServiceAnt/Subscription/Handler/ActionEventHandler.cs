using System;
using System.Threading.Tasks;

namespace ServiceAnt.Handler.Subscription.Handler
{
    public class ActionEventHandler<TEvent> : IEventHandler<TEvent> where TEvent : TransportTray
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
