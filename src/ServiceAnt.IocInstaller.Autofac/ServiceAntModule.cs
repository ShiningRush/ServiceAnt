using Autofac;
using ServiceAnt.Base;
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
        /// <param name="handlerAssemblies"> the assemblies which containg handlers, those will be registered to container</param>
        public ServiceAntModule(params System.Reflection.Assembly[] handlerAssemblies)
        {
            _handlerAssemblies = handlerAssemblies;
        }

        /// <summary>
        /// Intall dependenies and register handler function
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            var subcriptionsManager = new InMemorySubscriptionsManager();
            var requestManager = new InMemoryRequestHandlerManager();

            RegisterHandlers(subcriptionsManager, requestManager);
            builder.RegisterInstance(subcriptionsManager).As<ISubscriptionManager>().SingleInstance();
            builder.RegisterInstance(requestManager).As<IRequestHandlerManager>().SingleInstance();

            builder.Register<IIocResolver>(ctx =>
            {
                return new IocResolver(ctx.Resolve<IComponentContext>());
            });

            builder.RegisterType<InProcessServiceBus>().AsSelf().As<IServiceBus>().SingleInstance();

            builder.RegisterAssemblyTypes(_handlerAssemblies).AsSelf();

        }

        private void RegisterHandlers(ISubscriptionManager subcriptionsManager, IRequestHandlerManager requestManager )
        {
            foreach (var aHandlerAssembly in _handlerAssemblies)
            {
                var handlerTypes = aHandlerAssembly.GetTypes().Where(p => typeof(IHandler).IsAssignableFrom(p) && !p.IsInterface);

                foreach (var aHandler in handlerTypes)
                {
                    RegisterHandlerType(subcriptionsManager, requestManager, aHandler);
                }
            }
        }

        private void RegisterHandlerType(ISubscriptionManager subcriptionsManager, IRequestHandlerManager requestManager , Type aHandlerType)
        {
            var interfaces = aHandlerType.GetInterfaces();
            foreach (var aInterface in interfaces)
            {
                if (!typeof(IHandler).IsAssignableFrom(aInterface))
                {
                    continue;
                }

                var genericArgs = aInterface.GetGenericArguments();
                if (genericArgs.Length == 1)
                {
                    if (typeof(IRequestHandler).IsAssignableFrom(aInterface))
                        requestManager.AddRequestHandler(genericArgs[0], new IocHandlerFactory( aHandlerType, genericArgs[0]));
                    else
                        subcriptionsManager.AddSubscription(genericArgs[0], new IocHandlerFactory( aHandlerType, genericArgs[0]));
                }
            }
        }
    }
}
