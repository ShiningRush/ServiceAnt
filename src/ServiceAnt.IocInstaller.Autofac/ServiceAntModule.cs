using Autofac;
using ServiceAnt.Handler.Request;
using ServiceAnt.Infrastructure.Dependency;
using ServiceAnt.Request.Handler;
using ServiceAnt.Subscription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.IocInstaller.Autofac
{
    /// <summary>
    /// It used to install ServiceAnt to Ioc of Autofac
    /// </summary>
    public class ServiceAntModule : Module
    {
        private static System.Reflection.Assembly[] _handlerAssemblies;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handlerAssemblies"> the assemblies which containg handler, thos will be register to container</param>
        public ServiceAntModule(params System.Reflection.Assembly[] handlerAssemblies)
        {
            _handlerAssemblies = handlerAssemblies;
        }

        /// <summary>
        /// Excute this method ater you builded container
        /// </summary>
        /// <param name="container"></param>
        public static void RegisterHandlers(IComponentContext container)
        {
            foreach (var aHandlerAssembly in _handlerAssemblies)
            {
                var handlerTypes = aHandlerAssembly.GetTypes().Where(p => typeof(ServiceAnt.Handler.IHandler).IsAssignableFrom(p) && !p.IsInterface);

                foreach (var aHandler in handlerTypes)
                {
                    RegisterHandlerType(container, aHandler);
                }
            }
        }

        /// <summary>
        /// Intall dependenies and register handler function
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            var serviceBus = InProcessServiceBus.Default;
            builder.RegisterInstance(serviceBus).As<IServiceBus>();
            builder.RegisterType<InMemorySubscriptionsManager>().AsSelf().As<ISubscriptionManager>().SingleInstance();
            builder.RegisterType<InMemoryRequestHandlerManager>().AsSelf().As<IRequestHandlerManager>().SingleInstance();

            builder.Register<IocResolver>(ctx =>
            {
                return new IocResolver(ctx.Resolve<IComponentContext>());
            });

            builder.RegisterAssemblyTypes(_handlerAssemblies).AsSelf();
        }
        private static void RegisterHandlerType(IComponentContext container, Type aHandlerType)
        {
            var interfaces = aHandlerType.GetInterfaces();
            foreach (var aInterface in interfaces)
            {
                if (!typeof(ServiceAnt.Handler.IHandler).IsAssignableFrom(aInterface))
                {
                    continue;
                }

                var genericArgs = aInterface.GetGenericArguments();
                if (genericArgs.Length == 1)
                {
                    if (typeof(IRequestHandler).IsAssignableFrom(aInterface))
                        container.Resolve<IServiceBus>().AddRequestHandler(genericArgs[0], new ServiceAnt.Handler.IocHandlerFactory(container.Resolve<IocResolver>(), aHandlerType, genericArgs[0]));
                    else
                        container.Resolve<IServiceBus>().AddSubscription(genericArgs[0], new ServiceAnt.Handler.IocHandlerFactory(container.Resolve<IocResolver>(), aHandlerType, genericArgs[0]));
                }
            }
        }
    }
}
