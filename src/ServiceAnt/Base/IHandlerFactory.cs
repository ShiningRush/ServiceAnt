using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.Base
{
    /// <summary>
    /// The interface is used to get handler
    /// </summary>
    public interface IHandlerFactory
    {
        /// <summary>
        /// Get Handler
        /// </summary>
        /// <returns></returns>
        IHandler GetHandler();

        /// <summary>
        /// Get event type of registering event type
        /// </summary>
        /// <returns></returns>
        Type GetLocalEventType();

        /// <summary>
        /// Releast generated handler if need
        /// </summary>
        /// <param name="obj"></param>
        void ReleaseHandler(object obj);
    }
}
