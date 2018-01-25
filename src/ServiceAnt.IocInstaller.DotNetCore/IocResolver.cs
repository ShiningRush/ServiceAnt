using ServiceAnt.Infrastructure.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.IocInstaller.DotNetCore
{
    /// <summary>
    /// IocResolver
    /// </summary>
    public class IocResolver : IIocResolver
    {
        private readonly IServiceProvider _container;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="container">the container of castle</param>
        public IocResolver(IServiceProvider container)
        {
            _container = container;
        }

        /// <summary>
        /// Releases a pre-resolved object. See Resolve methods.
        /// </summary>
        /// <param name="obj">Object to be released</param>
        public void Release(object obj)
        {
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
            return (T)_container.GetService(type);
        }
    }
}
