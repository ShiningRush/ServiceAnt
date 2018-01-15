using ServiceAnt.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceAnt.Subscription
{
    /// <summary>
    /// It used to trigger a event
    /// </summary>
    public interface IEventTrigger : ITrigger
    {
    }

    /// <summary>
    /// It used to trigger a event
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    public abstract class EventTrigger<TDto> : Trigger<TDto>, IEventTrigger
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="inputDto"></param>
        public EventTrigger(TDto inputDto) : base(inputDto)
        {

        }
    }
}
