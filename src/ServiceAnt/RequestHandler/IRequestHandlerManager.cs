using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YiBan.Common.BaseAbpModule.Events.Abstractions
{
    public interface IRequestHandlerManager : IAddRequestHandler
    {
        List<IHandlerFactory> GetHandlerFactoriesForRequest(string requestName);
        List<IHandlerFactory> GetHandlerFactoriesForRequest(IntegrationEvent @request);

        string GetRequestName(Type aType);
    }
}
