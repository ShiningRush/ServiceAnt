using ServiceAnt.Handler;
using ServiceAnt.Handler.Request;
using ServiceAnt.Subscription;
using System;
using System.Threading.Tasks;

namespace ServiceAnt
{
    /// <summary>
    /// BusMessageLevel
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Debug
        /// </summary>
        DEBUG,
        /// <summary>
        /// INFO
        /// </summary>
        INFO,
        /// <summary>
        /// Error
        /// </summary>
        ERROR
    }

    /// <summary>
    /// This delegate used to handle bus message
    /// </summary>
    /// <param name="msgLevel">message level</param>
    /// <param name="message">message</param>
    /// <param name="ex">if happended exception, it will return it</param>
    public delegate void LogBusMessage(LogLevel msgLevel, string message, Exception ex);

    /// <summary>
    /// It used to publish event or send a request
    /// </summary>
    public interface IServiceBus : IAddSubscription, IAddRequestHandler
    {
        /// <summary>
        /// Use to log message of bus
        /// </summary>
        event LogBusMessage OnLogBusMessage;

        /// <summary>
        /// Publish a event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        Task Publish(ITrigger @event);

        /// <summary>
        /// Send a request sync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <returns></returns>
        T Send<T>(ITrigger @event);

        /// <summary>
        /// Send a request async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <returns></returns>
        Task<T> SendAsync<T>(ITrigger @event);
    }
}
