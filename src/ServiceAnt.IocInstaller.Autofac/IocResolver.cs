using Autofac;
using ServiceAnt.Infrastructure.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.IocInstaller.Autofac
{
    public class IocResolver : IIocResolver
    {
        private readonly IComponentContext _componentContext;

        public IocResolver(IComponentContext componentContext)
        {
            _componentContext = componentContext;
        }

        public void Release(object obj)
        {
            //_componentContext
        }

        public T Resolve<T>(Type type)
        {
            return (T)_componentContext.Resolve(type);
        }
    }
}
