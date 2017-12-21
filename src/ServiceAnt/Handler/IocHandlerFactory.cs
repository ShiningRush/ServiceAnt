using ServiceAnt.Handler;
using ServiceAnt.Infrastructure.Dependency;
using System;

namespace ServiceAnt.Handler
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
            return _iocResolver.Resolve<IHandler>(_handlerType);
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
