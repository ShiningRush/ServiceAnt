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
    public class ServiceAntInstaller : IWindsorInstaller
    {
        private IWindsorContainer _container;
        private IServiceBus _serviceBus;

        public ServiceAntInstaller()
        {
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            _container = container;

            container.Register(
                Component.For<IServiceBus>().ImplementedBy<InProcessServiceBus>().LifestyleSingleton(),
                Component.For<ISubscriptionManager>().ImplementedBy<InMemorySubscriptionsManager>().LifestyleSingleton(),
                Component.For<IRequestHandlerManager>().ImplementedBy<InMemoryRequestHandlerManager>().LifestyleSingleton());


            _serviceBus = container.Resolve<IServiceBus>();
            container.Kernel.ComponentRegistered += Kernel_ComponentRegistered;
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
                        _serviceBus.AddSubScription(genericArgs[0], new ServiceAnt.Handler.IocHandlerFactory(resolver, handler.ComponentModel.Implementation, genericArgs[0]));
                }
            }
        }
    }
}
