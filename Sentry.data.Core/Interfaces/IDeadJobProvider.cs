using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IDeadJobProvider
    {
        List<DeadSparkJob> GetDeadSparkJobs(DateTime timeCreated);
    }
}
