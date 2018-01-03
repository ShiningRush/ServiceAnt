using ServiceAnt.Handler;
using ServiceAnt.Handler.Request;
using ServiceAnt.Subscription;
using System;
using System.Threading.Tasks;

namespace ServiceAnt
{
    /// <summary>
    /// It used to publish event or send a request
    /// </summary>
    public interface IServiceBus : IAddSubscription, IAddRequestHandler
    {
        /// <summary>
        /// Use to log message of bus
        /// </summary>
        event Action<string, string, Exception> OnLogBusMessage;

        /// <summary>
        /// Publish a event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        Task Publish(TransportTray @event);

        /// <summary>
        /// Send a request sync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <returns></returns>
        T Send<T>(TransportTray @event);

        /// <summary>
        /// Send a request async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <returns></returns>
        Task<T> SendAsync<T>(TransportTray @event);
    }
}
