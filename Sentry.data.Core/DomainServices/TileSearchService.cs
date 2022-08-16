using Sentry.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class TileSearchService : ITileSearchService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly IUserService _userService;

        public TileSearchService(IDatasetContext datasetContext, IUserService userService)
        {
            _datasetContext = datasetContext;
            _userService = userService;
        }

        public DatasetSearchResultDto SearchDatasets(DatasetSearchDto datasetSearchDto)
        {
            DatasetSearchResultDto resultDto = new DatasetSearchResultDto()
            {
                PageSize = datasetSearchDto.PageSize,
                PageNumber = datasetSearchDto.PageNumber
            };

            try
            {
                IEnumerable<DatasetTileDto> dtos = SearchDatasetTileDtos(datasetSearchDto);
                List<DatasetTileDto> allResults = dtos.FilterBy(datasetSearchDto.FilterCategories).ToList();

                resultDto.TotalResults = allResults.Count;
                resultDto.Tiles = ApplyPaging(allResults, datasetSearchDto);
            }
            catch (Exception ex)
            {
                Logger.Error("Error searching datasets", ex);
            }

            return resultDto;
        }

        public IEnumerable<DatasetTileDto> SearchDatasetTileDtos(DatasetSearchDto datasetSearchDto)
        {
            IEnumerable<DatasetTileDto> dtos = datasetSearchDto.SearchableTiles ?? GetDatasetTileDtos();

            if (!string.IsNullOrWhiteSpace(datasetSearchDto.SearchText))
            {
                dtos = dtos.Where(x => x.Name.ToLower().Contains(datasetSearchDto.SearchText.ToLower()));
            }

            return dtos;
        }

        #region Private
        private IEnumerable<DatasetTileDto> GetDatasetTileDtos()
        {
            List<Dataset> datasets = _datasetContext.Datasets.Where(w => w.DatasetType == DataEntityCodes.DATASET && w.ObjectStatus != GlobalEnums.ObjectStatusEnum.Deleted).FetchAllChildren(_datasetContext);

            string associateId = _userService.GetCurrentUser().AssociateId;
            List<DatasetTileDto> datasetTileDtos = new List<DatasetTileDto>();

            //map to DatasetTileDto
            foreach (Dataset dataset in datasets)
            {
                DatasetTileDto datasetTileDto = dataset.ToTileDto();
                datasetTileDto.IsFavorite = dataset.Favorities.Any(w => w.UserId == associateId);

                datasetTileDto.LastActivityDateTime = dataset.ChangedDtm;
                if (dataset.DatasetFiles?.Any() == true)
                {
                    DateTime lastFileDate = dataset.DatasetFiles.Max(x => x.CreatedDTM);
                    if (lastFileDate > datasetTileDto.LastActivityDateTime)
                    {
                        datasetTileDto.LastActivityDateTime = lastFileDate;
                    }
                }

                datasetTileDtos.Add(datasetTileDto);
            }

            return datasetTileDtos;
        }

        private List<DatasetTileDto> ApplyPaging(IEnumerable<DatasetTileDto> dtos, DatasetSearchDto datasetSearchDto)
        {
            if (datasetSearchDto.OrderByDescending)
            {
                dtos = dtos.OrderByDescending(datasetSearchDto.OrderByField);
            }
            else
            {
                dtos = dtos.OrderBy(datasetSearchDto.OrderByField);
            }

            if (datasetSearchDto.PageSize > 0)
            {
                dtos = dtos.Skip(datasetSearchDto.PageSize * (datasetSearchDto.PageNumber - 1)).Take(datasetSearchDto.PageSize);
            }

            return dtos.ToList();
        }
        #endregion
    }
}
