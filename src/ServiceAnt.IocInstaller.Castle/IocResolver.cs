using Castle.Windsor;
using ServiceAnt.Infrastructure.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.IocInstaller.Castle
{
    public class IocResolver : IIocResolver
    {
        private readonly IWindsorContainer _container;

        public IocResolver(IWindsorContainer container)
        {
            _container = container;
        }

        public void Release(object obj)
        {
            _container.Release(obj);
        }

        public T Resolve<T>(Type type)
        {
            return (T)_container.Resolve(type);
        }
    }
}
