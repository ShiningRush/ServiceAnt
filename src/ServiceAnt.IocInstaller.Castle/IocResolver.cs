using Castle.Windsor;
using ServiceAnt.Infrastructure.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.IocInstaller.Castle
{
    /// <summary>
    /// IocResolver
    /// </summary>
    public class IocResolver : IIocResolver
    {
        private readonly IWindsorContainer _container;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="container">the container of castle</param>
        public IocResolver(IWindsorContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// Releases a pre-resolved object. See Resolve methods.
        /// </summary>
        /// <param name="obj">Object to be released</param>
        public void Release(object obj)
        {
            _container.Release(obj);
        }

        /// <summary>
        /// Gets an object from IOC container.
        /// Returning object must be Released (see <see cref="Release"/>) after usage.
        /// </summary> 
        /// <typeparam name="T">Type of the object to cast</typeparam>
        /// <param name="type">Type of the object to resolve</param>
        /// <returns>The object instance</returns>
        public T Resolve<T>(Type type)
        {
            return (T)_container.Resolve(type);
        }
    }
}
