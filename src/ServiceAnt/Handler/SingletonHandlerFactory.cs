using System;

namespace ServiceAnt.Handler
{
    internal class SingletonHandlerFactory : IHandlerFactory
    {
        private IHandler _instance;
        private Type _localEventType;

        public SingletonHandlerFactory(IHandler instance, Type localEventType = null)
        {
            _instance = instance;
            _localEventType = localEventType;
        }

        public IHandler GetHandler()
        {
            return _instance;
        }

        public Type GetLocalEventType()
        {
            return _localEventType;
        }

        public void ReleaseHandler(object obj)
        {
        }
    }
}
