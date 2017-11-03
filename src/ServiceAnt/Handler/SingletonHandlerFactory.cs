using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YiBan.Common.BaseAbpModule.Events.Abstractions;

namespace YiBan.Common.BaseAbpModule.Events
{
    public class SingletonHandlerFactory : IHandlerFactory
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
