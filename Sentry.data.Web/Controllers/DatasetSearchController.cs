using Sentry.data.Core;
using System.Collections.Generic;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Web.Controllers
{
    public class DatasetSearchController : TileSearchController<DatasetTileDto>
    {
        public DatasetSearchController(ITileSearchService<DatasetTileDto> tileSearchService, 
                                       IFilterSearchService filterSearchService, 
                                       IDataFeatures dataFeatures) : base(tileSearchService, filterSearchService, dataFeatures) { }

        protected override FilterSearchConfigModel GetFilterSearchConfigModel(FilterSearchModel searchModel)
        {
            return new FilterSearchConfigModel()
            {
                PageTitle = "Datasets",
                SearchType = SearchType.DATASET_SEARCH,
                IconPath = "~/Images/Icons/DatasetsBlue.svg",
                DefaultSearch = searchModel
            };
        }

        protected override bool HasPermission()
        {
            return SharedContext.CurrentUser.CanViewDataset;
        }

        protected override string GetSearchType()
        {
            return SearchType.DATASET_SEARCH;
        }

        protected override List<DatasetTileDto> MapToTileDtos(List<TileModel> tileModels)
        {
            return tileModels.ToDatasetTileDtos();
        }

        protected override List<TileModel> MapToTileModels(List<DatasetTileDto> tileDtos)
        {
            return tileDtos.ToModels();
        }
    }
}