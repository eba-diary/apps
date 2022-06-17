using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IKafkaConnectorProvider
    {
        Task<HttpResponseMessage> PostRequestAsync(string resource, HttpContent postContent);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <exception cref="TaskCanceledException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        /// <returns></returns>
        Task<HttpResponseMessage> GetRequestAsync(string resource);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <exception cref="TaskCanceledException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        /// <returns></returns>
        Task<HttpResponseMessage> DeleteRequestAsync(string resource);
    }
}
