using ServiceAnt.Base;
using ServiceAnt.Handler.Request;
using ServiceAnt.Request;
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
    /// TriggerOpion
    /// </summary>
    public class TriggerOption
    {
        /// <summary>
        /// this value indicate whether ignore exception when triggering
        /// </summary>
        public bool IsIgnoreException { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public TriggerOption(bool isIgnoreException = true)
        {
            IsIgnoreException = isIgnoreException;
        }
    }

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
        Task Publish(IEventTrigger @event);

        /// <summary>
        /// Publish a event
        /// </summary>
        /// <param name="event"></param>
        /// <param name="triggerOption"></param>
        /// <returns></returns>
        Task Publish(IEventTrigger @event, TriggerOption triggerOption);

        /// <summary>
        /// Send a request sync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <returns></returns>
        T Send<T>(IRequestTrigger @event);

        /// <summary>
        /// Send a request sync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <param name="triggerOption"></param>
        /// <returns></returns>
        T Send<T>(IRequestTrigger @event, TriggerOption triggerOption);

        /// <summary>
        /// Send a request async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <returns></returns>
        Task<T> SendAsync<T>(IRequestTrigger @event);

        /// <summary>
        /// Send a request async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <param name="triggerOption"></param>
        /// <returns></returns>
        Task<T> SendAsync<T>(IRequestTrigger @event, TriggerOption triggerOption);
    }
}
