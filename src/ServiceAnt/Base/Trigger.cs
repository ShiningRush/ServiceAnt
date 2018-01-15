using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.Base
{
    /// <summary>
    /// It is used to trigger event or request
    /// </summary>
    public interface ITrigger
    {
    }
    
    /// <summary>
    /// It is used to trigger event or request
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    public abstract class Trigger<TDto> : ITrigger
    {
        /// <summary>
        /// Dto
        /// </summary>
        public TDto Dto { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="inputDto"></param>
        public Trigger(TDto inputDto)
        {
            Dto = inputDto;
        }
    }
}
