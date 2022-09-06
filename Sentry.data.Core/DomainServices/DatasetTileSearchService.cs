using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class DatasetTileSearchService : TileSearchService<DatasetTileDto>
    {
        public DatasetTileSearchService(IDatasetContext datasetContext, IUserService userService, IEventService eventService) : base(datasetContext, userService, eventService) { }

        protected override IQueryable<Dataset> GetDatasets()
        {
            return _datasetContext.Datasets.Where(w => w.DatasetType == DataEntityCodes.DATASET && w.ObjectStatus != GlobalEnums.ObjectStatusEnum.Deleted);
        }

        protected override DatasetTileDto MapToTileDto(Dataset dataset)
        {
            return dataset.ToDatasetTileDto();
        }
    }
}
