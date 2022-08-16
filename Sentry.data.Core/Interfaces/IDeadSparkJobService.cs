using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IDeadSparkJobService
    {
        List<DeadSparkJobDto> GetDeadSparkJobDtos(DateTime timeCreated);
    }
}
