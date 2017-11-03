using Abp.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YiBan.Common.BaseAbpModule.Events.Abstractions;

namespace YiBan.Common.BaseAbpModule.Events
{
    public class IocHandlerFactory : IHandlerFactory
    {
        private IIocResolver _iocResolver;
        private Type _handlerType;
        private Type _localEventType;

        public IocHandlerFactory(IIocResolver iocResolver, Type handlerType, Type localEventType)
        {
            _iocResolver = iocResolver;
            _handlerType = handlerType;
            _localEventType = localEventType;
        }

        public IHandler GetHandler()
        {
            return (IHandler)_iocResolver.Resolve(_handlerType);
        }

        public void ReleaseHandler(object obj)
        {
            _iocResolver.Release(obj);
        }

        public Type GetLocalEventType()
        {
            return _localEventType;
        }
    }
}
