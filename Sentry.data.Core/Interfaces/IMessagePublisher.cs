using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Sentry.data.Core
{
    public interface IMessagePublisher
    {
        /// <summary>
        /// Publish a message to topic supplied
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Publish(string topic, string key, string value);

        Task PublishDSCEventAsync(string key, string value, string topic = null);

        /// <summary>
        /// Publish message to data.sentry.com event topic
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void PublishDSCEvent(string key, string value, string topic = null);
    }
}
