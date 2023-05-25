using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public class MigrationService : IMigrationService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly IDatasetService _datasetService;


        public MigrationService(IDatasetContext datasetContext, IDatasetService datasetService)
        {
            _datasetContext = datasetContext;
            _datasetService = datasetService;
        }

        //GET MIGRATION HISTORY ROWS BY A LIST OF datasetIds
        public List<MigrationHistory> GetMigrationHistory(int datasetId, string namedEnvironment)
        {
            List<int> datasetIdList = GetRelatedDatasetIds(datasetId, namedEnvironment);

            List<MigrationHistory> migrationHistories = _datasetContext.MigrationHistory.Where(w => datasetIdList.Contains((int)w.SourceDatasetId) || datasetIdList.Contains((int)w.TargetDatasetId))
                                                                                       .OrderByDescending(o => o.CreateDateTime)
                                                                                       .ToList();
            return migrationHistories;
        }


        private List<int> GetRelatedDatasetIds(int datasetId, string namedEnvironment)
        {
            //GRAB DTO FOR THIS DatasetId
            DatasetDetailDto dto = _datasetService.GetDatasetDetailDto(datasetId);

            //TURN DatasetRelatives INTO datasetIdList AND FILTER ON NAMEDENVIRONMENT SELECTED
            //NOTE: IF namedEnvironment==ALL THEN IT WILL AUTOMATICALLY GRAB EVERYTHING
            //THE OR IN LINQ IS A TRICK TO BRING IN ALL RELATIVES IF namedEnvironment==ALL
            List<int> datasetIdList = new List<int>(dto.DatasetRelatives.Where(w => w.NamedEnvironment == namedEnvironment || namedEnvironment == GlobalConstants.MigrationHistoryNamedEnvFilter.ALL_NAMED_ENV)
                                                                        .Select(s => s.DatasetId));

            return datasetIdList;
        }


        //GET DATASET RELATIVES AND ORIGIN DATASET INFO WITH MIGRATION HISTORY BASED ON datasetId
        public DatasetRelativeOriginDto GetRelativesWithMigrationHistory(int datasetId)
        {
            //GRAB DTO FOR THIS DatasetId
            DatasetDetailDto datasetDetailDto = _datasetService.GetDatasetDetailDto(datasetId);

            //USE JOIN LOGIC TO DETERMINE WHICH RELATIVES WERE FROM SOURCE MIGRATION REQUEST SIDE
            //USE DISTINCT BECAUSE THERE ARE MANY MigrationHistories to a SINGLE RELATIVE, I CHOSE THIS INSTEAD OF MORE COMPLEX GROUP BY LOGIC
            IEnumerable<DatasetRelativeDto> sourceRelatives = 
            (   from r in datasetDetailDto.DatasetRelatives
                join mh in _datasetContext.MigrationHistory on r.DatasetId equals mh.SourceDatasetId
                select r
            ).Distinct();

            //USE JOIN LOGIC TO DETERMINE WHICH RELATIVES WERE FROM TARGET MIGRATION REQUEST SIDE
            //USE DISTINCT BECAUSE THERE ARE MANY MigrationHistories to a SINGLE RELATIVE, I CHOSE THIS INSTEAD OF MORE COMPLEX GROUP BY LOGIC
            IEnumerable<DatasetRelativeDto> targetRelatives =
            (   from r in datasetDetailDto.DatasetRelatives
                join mh in _datasetContext.MigrationHistory on r.DatasetId equals mh.TargetDatasetId
                select r
            ).Distinct();

            DatasetRelativeOriginDto datasetOriginDto = new DatasetRelativeOriginDto()
            {
                OriginDatasetId = datasetDetailDto.DatasetId,
                OriginDatasetName = datasetDetailDto.DatasetName,
                DatasetRelativesDto = new List<DatasetRelativeDto>()
            };

            //DECLARE OUR MASTER LIST
            List<DatasetRelativeDto> allRelatives = new List<DatasetRelativeDto>();

            //ADD SOURCE AND TARGET RELATIVES
            allRelatives.AddRange(sourceRelatives);
            allRelatives.AddRange(targetRelatives);

            //USE DISTINCT BECAUSE SOURCE AND TARGET MAY HAVE SAME ENTRY
            datasetOriginDto.DatasetRelativesDto.AddRange(allRelatives.Distinct().ToList());
            
            return datasetOriginDto;
        }
    }
}
