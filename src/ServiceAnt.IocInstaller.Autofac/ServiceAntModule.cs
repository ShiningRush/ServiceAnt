using Autofac;
using ServiceAnt.Handler.Request;
using ServiceAnt.Infrastructure.Dependency;
using ServiceAnt.Subscription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.IocInstaller.Autofac
{
    public class ServiceAntModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<InProcessServiceBus>().AsSelf().As<IServiceBus>().SingleInstance();
            builder.RegisterType<InMemorySubscriptionsManager>().AsSelf().As<ISubscriptionManager>().SingleInstance();
            builder.RegisterType<InMemoryRequestHandlerManager>().AsSelf().As<IRequestHandlerManager>().SingleInstance();

            builder.Register<IocResolver>(ctx =>
            {
                return new IocResolver(ctx.Resolve<IComponentContext>());
            });

        }
    }
}
