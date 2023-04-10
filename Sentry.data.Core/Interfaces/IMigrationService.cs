using Sentry.Core;
using Sentry.data.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IMigrationService
    {
        List<MigrationHistory> GetMigrationHistory(List<int> datasetIdList);
        List<DatasetRelativeDto> GetRelativesWithMigrationHistory(List<DatasetRelativeDto> relativeDtos);
    }
}
