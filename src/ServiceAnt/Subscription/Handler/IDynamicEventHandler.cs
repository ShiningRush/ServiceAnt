using ServiceAnt.Base;
using System.Threading.Tasks;

namespace ServiceAnt.Handler.Subscription.Handler
{
    /// <summary>
    /// Handle event with dynamic parameter
    /// </summary>
    public interface IDynamicEventHandler : IHandler
    {
        /// <summary>
        /// Handle event
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        Task HandleAsync(dynamic param);
    }
}
