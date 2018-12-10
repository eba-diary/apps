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
        void Publish(string topic, string key, string value);
    }
}
