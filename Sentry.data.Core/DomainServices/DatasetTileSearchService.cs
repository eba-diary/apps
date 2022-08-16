using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class DatasetTileSearchService : TileSearchService<DatasetTileDto>
    {
        private readonly IDatasetContext _datasetContext;

        public DatasetTileSearchService(IDatasetContext datasetContext, IUserService userService) : base(userService)
        {
            _datasetContext = datasetContext;
        }

        protected override List<Dataset> GetDatasets()
        {
            return _datasetContext.Datasets.Where(w => w.DatasetType == DataEntityCodes.DATASET && w.ObjectStatus != GlobalEnums.ObjectStatusEnum.Deleted).FetchAllChildren(_datasetContext);
        }

        protected override DatasetTileDto MapToTileDto(Dataset dataset)
        {
            return dataset.ToTileDto();
        }
    }
}
