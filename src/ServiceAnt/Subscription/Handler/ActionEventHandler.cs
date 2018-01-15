using ServiceAnt.Subscription;
using ServiceAnt.Subscription.Handler;
using System;
using System.Threading.Tasks;

namespace ServiceAnt.Handler.Subscription.Handler
{
    /// <summary>
    /// Handle event by action
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public class ActionEventHandler<TEvent> : IEventHandler<TEvent> where TEvent : IEventTrigger
    {
        private Func<TEvent, Task> _action;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="action"></param>
        public ActionEventHandler(Func<TEvent, Task>  action)
        {
            _action = action;
        }

        /// <summary>
        /// handle event
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task HandleAsync(TEvent param)
        {
            await _action(param);
        }

        /// <summary>
        /// check if input action is same with self
        /// </summary>
        /// <param name="compareAction"></param>
        /// <returns></returns>
        public bool IsSame(Func<TEvent, Task> compareAction)
        {
            return compareAction == _action;
        }
    }

    /// <summary>
    /// Handle event by dynamic action
    /// </summary>
    public class ActionEventHandler : IDynamicEventHandler
    {
        private Func<dynamic, Task> _action;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="action"></param>
        public ActionEventHandler(Func<dynamic, Task> action)
        {
            _action = action;
        }

        /// <summary>
        /// handle event
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task HandleAsync(dynamic param)
        {
            await _action(param);
        }

        /// <summary>
        /// check if input action is same with self
        /// </summary>
        /// <param name="compareAction"></param>
        /// <returns></returns>
        public bool IsSame(Func<dynamic, Task> compareAction)
        {
            return compareAction == _action;
        }
    }
}
