using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using ServiceAnt.Handler.Request;
using ServiceAnt.Request.Handler;
using ServiceAnt.Subscription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.IocInstaller.Castle
{
    /// <summary>
    /// It used to install ServiceAnt to Ioc of Castle
    /// </summary>
    public class ServiceAntInstaller : IWindsorInstaller
    {
        private readonly Assembly[] _handlerAssemblies;
        private IWindsorContainer _container;
        private IServiceBus _serviceBus;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handlerAssemblies"> the assemblies which containg handler, those will be register to container</param>
        public ServiceAntInstaller(params Assembly[] handlerAssemblies)
        {
            _handlerAssemblies = handlerAssemblies;
        }

        /// <summary>
        /// Intall dependenies and register handler function
        /// </summary>
        /// <param name="container"></param>
        /// <param name="store"></param>
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            _container = container;

            container.Register(
                Component.For<IServiceBus>().Instance(InProcessServiceBus.Default),
                Component.For<ISubscriptionManager>().ImplementedBy<InMemorySubscriptionsManager>().LifestyleSingleton(),
                Component.For<IRequestHandlerManager>().ImplementedBy<InMemoryRequestHandlerManager>().LifestyleSingleton());

            _serviceBus = container.Resolve<IServiceBus>();
            container.Kernel.ComponentRegistered += Kernel_ComponentRegistered;

            foreach (var aHandlerAssembly in _handlerAssemblies)
            {
                container.Register(Classes.FromAssembly(aHandlerAssembly)
                    .BasedOn<ServiceAnt.Handler.IHandler>()
                    .WithService.Self()
                    .LifestyleTransient());
            }


        }

        private void Kernel_ComponentRegistered(string key, IHandler handler)
        {
            if (!typeof(ServiceAnt.Handler.IHandler).GetTypeInfo().IsAssignableFrom(handler.ComponentModel.Implementation))
            {
                return;
            }

            var interfaces = handler.ComponentModel.Implementation.GetTypeInfo().GetInterfaces();
            foreach (var aInterface in interfaces)
            {
                if (!typeof(ServiceAnt.Handler.IHandler).GetTypeInfo().IsAssignableFrom(aInterface))
                {
                    continue;
                }

                var resolver = new IocResolver(_container);

                var genericArgs = aInterface.GetGenericArguments();
                if (genericArgs.Length == 1)
                {
                    if (typeof(IRequestHandler).GetTypeInfo().IsAssignableFrom(aInterface))
                        _serviceBus.AddRequestHandler(genericArgs[0], new ServiceAnt.Handler.IocHandlerFactory(resolver, handler.ComponentModel.Implementation, genericArgs[0]));
                    else
                        _serviceBus.AddSubscription(genericArgs[0], new ServiceAnt.Handler.IocHandlerFactory(resolver, handler.ComponentModel.Implementation, genericArgs[0]));
                }
            }
        }
    }
}
