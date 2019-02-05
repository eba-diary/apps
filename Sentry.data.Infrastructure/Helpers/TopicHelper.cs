using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    public static class TopicHelper
    {
        public static string GetDSCEventTopic()
        {
            return Configuration.Config.GetSetting("SAIDKey").ToUpper() + "-" + Configuration.Config.GetHostSetting("EnvironmentName").ToUpper() + "-" + Configuration.Config.GetHostSetting("DSCEventTopic").ToUpper();
        }
    }
}
