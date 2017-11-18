using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.Handler.Request
{
    public interface IRequestHandlerManager : IAddRequestHandler
    {
        List<IHandlerFactory> GetHandlerFactoriesForRequest(string requestName);
        List<IHandlerFactory> GetHandlerFactoriesForRequest(TransportTray @request);

        string GetRequestName(Type aType);
    }
}
