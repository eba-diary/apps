using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IDeadJobProvider
    {
        List<DeadSparkJobDto> GetDeadSparkJobDtos(int timeCreated);
    }
}
