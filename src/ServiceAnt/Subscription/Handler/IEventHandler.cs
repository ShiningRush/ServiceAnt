using ServiceAnt.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.Subscription.Handler
{
    /// <summary>
    /// It is used to handle event
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventHandler<TEvent> : IHandler where TEvent : ITrigger
    {
        /// <summary>
        /// handle event
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        Task HandleAsync(TEvent param);
    }
}
