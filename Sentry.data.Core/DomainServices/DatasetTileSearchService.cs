using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class DatasetTileSearchService : TileSearchService<DatasetTileDto>
    {
        public DatasetTileSearchService(IDatasetContext datasetContext, IUserService userService, IEventService eventService) : base(datasetContext, userService, eventService) { }

        protected override IQueryable<Dataset> GetDatasets()
        {
            IQueryable<Dataset> datasets = _datasetContext.Datasets.Where(w => w.DatasetType == DataEntityCodes.DATASET);

            if (_userService.GetCurrentUser().IsAdmin)
            {
                datasets = datasets.Where(x => x.ObjectStatus != GlobalEnums.ObjectStatusEnum.Deleted);
            }
            else
            {
                datasets = datasets.Where(x => x.ObjectStatus == GlobalEnums.ObjectStatusEnum.Active);
            }

            return datasets;
        }

        protected override DatasetTileDto MapToTileDto(Dataset dataset)
        {
            return dataset.ToDatasetTileDto();
        }
    }
}
