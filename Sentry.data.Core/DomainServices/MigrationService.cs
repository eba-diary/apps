using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.Entities;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class MigrationService : IMigrationService
    {
        private readonly IDatasetContext _datasetContext;


        public MigrationService(IDatasetContext datasetContext)
        {
            _datasetContext = datasetContext;
        }

        //GET MIGRATION HISTORY ROWS BY A LIST OF datasetIds
        public List<MigrationHistory> GetMigrationHistory(List<int> datasetIdList)
        {
            List<MigrationHistory> migrationHistories = _datasetContext.MigrationHistory.Where(w => datasetIdList.Contains((int)w.SourceDatasetId) || datasetIdList.Contains((int)w.TargetDatasetId))
                                                                                       .OrderByDescending(o => o.CreateDateTime)
                                                                                       .ToList();
            return migrationHistories;
        }

        //GET DATASET RELATIVES WITH MIGRATION HISTORY BASED ON LIST OF DatasetRelativeDto
        public List<DatasetRelativeDto> GetRelativesWithMigrationHistory(List<DatasetRelativeDto> relativeDtos)
        {
            //DECLARE OUR MASTER LIST
            List<DatasetRelativeDto> allRelatives = new List<DatasetRelativeDto>();
            
            //USE JOIN LOGIC TO DETERMINE WHICH RELATIVES WERE FROM SOURCE MIGRATION REQUEST SIDE
            //USE DISTINCT BECAUSE THERE ARE MANY MigrationHistories to a SINGLE RELATIVE, I CHOSE THIS INSTEAD OF MORE COMPLEX GROUP BY LOGIC
            IEnumerable<DatasetRelativeDto> sourceRelatives = 
            (   from r in relativeDtos
                join mh in _datasetContext.MigrationHistory on r.DatasetId equals mh.SourceDatasetId
                select r
            ).Distinct();

            //USE JOIN LOGIC TO DETERMINE WHICH RELATIVES WERE FROM TARGET MIGRATION REQUEST SIDE
            //USE DISTINCT BECAUSE THERE ARE MANY MigrationHistories to a SINGLE RELATIVE, I CHOSE THIS INSTEAD OF MORE COMPLEX GROUP BY LOGIC
            IEnumerable<DatasetRelativeDto> targetRelatives =
            (   from r in relativeDtos
                join mh in _datasetContext.MigrationHistory on r.DatasetId equals mh.TargetDatasetId
                select r
            ).Distinct();

            //ADD SOURCE AND TARGET RELATIVES
            allRelatives.AddRange(sourceRelatives);
            allRelatives.AddRange(targetRelatives);

            //USE DISTINCT BECAUSE SOURCE AND TARGET MAY HAVE SAME ENTRY
            return allRelatives.Distinct().ToList();
        }

    }
}
