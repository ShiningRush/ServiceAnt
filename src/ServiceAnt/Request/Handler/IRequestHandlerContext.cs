using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAnt.Request.Handler
{
    public interface IRequestHandlerContext
    {
        /// <summary>
        /// Response
        /// </summary>
        object Response { get; set; }

        /// <summary>
        /// Environment
        /// </summary>
        Dictionary<string, object> Environment { get; set; }

        /// <summary>
        /// Indicate if it is the end of pipeline
        /// </summary>
        bool IsEnd { get; set; }

        /// <summary>
        /// Get the value from enviroment
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T Get<T>(string key);

        /// <summary>
        /// Set the T to environment
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IRequestHandlerContext Set<T>(string key, T value);

    }

    public class RequestHandlerContext : IRequestHandlerContext
    {
        /// <summary>
        /// Response
        /// </summary>
        public object Response { get; set; }


        /// <summary>
        /// Indicate if it is the end of pipeline
        /// </summary>
        public bool IsEnd { get; set; }

        /// <summary>
        /// Environment
        /// </summary>
        public Dictionary<string, object> Environment { get; set; }

        /// <summary>
        /// Get the value from enviroment
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            object o;
            return this.Environment.TryGetValue(key, out o) ? (T)o : default(T);
        }


        /// <summary>
        /// Set the T to environment
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IRequestHandlerContext Set<T>(string key,T value)
        {
            object o;
            if (!this.Environment.TryGetValue(key, out o))
                this.Environment.Add(key, o);
            else
                this.Environment[key] = value;

            return this;
        }
    }
}
