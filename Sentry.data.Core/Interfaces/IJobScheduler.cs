using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Interfaces
{
    public interface IJobScheduler
    {
        string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay);
    }
}
