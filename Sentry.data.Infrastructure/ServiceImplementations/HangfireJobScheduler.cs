using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.data.Core.Interfaces;
using Hangfire;
using System.Linq.Expressions;

namespace Sentry.data.Infrastructure.ServiceImplementations
{
    public class HangfireJobScheduler : IJobScheduler
    {
        public string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
        {
            return BackgroundJob.Schedule(methodCall, delay);
        }
    }
}
