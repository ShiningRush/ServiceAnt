using Microsoft.Extensions.DependencyInjection;
using ServiceAnt.Handler.Request;
using ServiceAnt.Subscription;
using System;
using System.Reflection;
using System.Linq;
using ServiceAnt.Base;
using ServiceAnt.Request.Handler;
using ServiceAnt.Infrastructure.Dependency;
using ServiceAnt.IocInstaller.DotNetCore;
using ServiceAnt;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private static System.Reflection.Assembly[] _handlerAssemblies;

        public static void AddServiceAnt(this IServiceCollection @this, params Assembly[] handlerAssemblies)
        {
            _handlerAssemblies = handlerAssemblies;

            var subcriptionsManager = new InMemorySubscriptionsManager();
            var requestManager = new InMemoryRequestHandlerManager();

            RegisterHandlers(subcriptionsManager, requestManager);
            @this.AddSingleton<ISubscriptionManager>(subcriptionsManager);
            @this.AddSingleton<IRequestHandlerManager>(requestManager);

            @this.AddTransient<IIocResolver>(serviceProvider =>
            {
                return new IocResolver(serviceProvider);
            });
            @this.AddSingleton<IServiceBus, InProcessServiceBus>();


            var allHandlerTypes = _handlerAssemblies.SelectMany(p => p.ExportedTypes).Where(p=>typeof(IHandler).IsAssignableFrom(p));
            foreach (var aHandlerType in allHandlerTypes)
            {
                @this.AddTransient(aHandlerType);
            }
        }

        private static void RegisterHandlers(ISubscriptionManager subcriptionsManager, IRequestHandlerManager requestManager)
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

        private static void RegisterHandlerType(ISubscriptionManager subcriptionsManager, IRequestHandlerManager requestManager, Type aHandlerType)
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
                        requestManager.AddRequestHandler(genericArgs[0], new IocHandlerFactory(aHandlerType, genericArgs[0]));
                    else
                        subcriptionsManager.AddSubscription(genericArgs[0], new IocHandlerFactory(aHandlerType, genericArgs[0]));
                }
            }
        }
    }
}
