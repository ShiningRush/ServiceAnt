using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.Handler
{
    public interface IHandlerFactory
    {
        IHandler GetHandler();

        Type GetLocalEventType();

        void ReleaseHandler(object obj);
    }
}
