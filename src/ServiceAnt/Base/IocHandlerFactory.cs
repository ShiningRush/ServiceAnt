using ServiceAnt.Handler;
using ServiceAnt.Infrastructure.Dependency;
using System;

namespace ServiceAnt.Base
{
    /// <summary>
    /// This factory is used to get hanlder with IOC
    /// </summary>
    public class IocHandlerFactory : IHandlerFactory
    {
        private IIocResolver _iocResolver;
        private Type _handlerType;
        private Type _localEventType;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="handlerType"></param>
        /// <param name="localEventType"></param>
        public IocHandlerFactory(Type handlerType, Type localEventType)
        {
            _handlerType = handlerType;
            _localEventType = localEventType;
        }

        /// <summary>
        /// set the IocResolver to resolve service
        /// </summary>
        /// <param name="iocResolver"></param>
        public void SetIocResolver(IIocResolver iocResolver)
        {
            _iocResolver = iocResolver;
        }

        /// <summary>
        /// get handler
        /// </summary>
        /// <returns></returns>
        public IHandler GetHandler()
        {
            return _iocResolver.Resolve<IHandler>(_handlerType);
        }

        /// <summary>
        /// release handler if need
        /// </summary>
        /// <param name="obj"></param>
        public void ReleaseHandler(object obj)
        {
            _iocResolver.Release(obj);
        }

        /// <summary>
        /// get the trigger class type in excuting process
        /// </summary>
        /// <returns></returns>
        public Type GetLocalEventType()
        {
            return _localEventType;
        }
    }
}
