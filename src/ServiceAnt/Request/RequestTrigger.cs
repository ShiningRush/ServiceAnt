using ServiceAnt.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceAnt.Request
{
    /// <summary>
    /// It used to trigger a request
    /// </summary>
    public interface IRequestTrigger :ITrigger
    {
    }

    /// <summary>
    /// It used to trigger a request
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    public abstract class RequestTrigger<TDto> : Trigger<TDto>, IRequestTrigger
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="inputDto"></param>
        public RequestTrigger(TDto inputDto) : base(inputDto)
        { }
    }
}
