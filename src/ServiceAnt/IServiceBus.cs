using ServiceAnt.Handler;
using ServiceAnt.Handler.Request;
using ServiceAnt.Subscription;
using System.Threading.Tasks;

namespace ServiceAnt
{
    public interface IServiceBus : IAddSubscription, IAddRequestHandler
    {
        void Publish(TransportTray @event);

        T Send<T>(TransportTray @event);

        Task<T> SendAsync<T>(TransportTray @event);
    }
}
