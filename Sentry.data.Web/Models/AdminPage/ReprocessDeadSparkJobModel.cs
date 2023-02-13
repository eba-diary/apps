using System;

namespace Sentry.data.Web
{
    public class ReprocessDeadSparkJobModel
    {
        public DateTime DefaultDateTime = DateTime.Now.AddHours(-2);
    }
}