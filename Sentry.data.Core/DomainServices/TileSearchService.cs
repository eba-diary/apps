using Sentry.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public abstract class TileSearchService<T> : ITileSearchService<T> where T : DatasetTileDto
    {
        private readonly IUserService _userService;

        protected TileSearchService(IUserService userService)
        {
            _userService = userService;
        }

        public TileSearchResultDto<T> SearchDatasets(TileSearchDto<T> datasetSearchDto)
        {
            TileSearchResultDto<T> resultDto = new TileSearchResultDto<T>()
            {
                PageSize = datasetSearchDto.PageSize,
                PageNumber = datasetSearchDto.PageNumber
            };

            try
            {
                IEnumerable<T> dtos = SearchDatasetTileDtos(datasetSearchDto);
                List<T> allResults = dtos.FilterBy(datasetSearchDto.FilterCategories).ToList();

                resultDto.TotalResults = allResults.Count;
                resultDto.Tiles = ApplyPaging(allResults, datasetSearchDto);
            }
            catch (Exception ex)
            {
                Logger.Error("Error searching datasets", ex);
            }

            return resultDto;
        }

        public IEnumerable<T> SearchDatasetTileDtos(TileSearchDto<T> datasetSearchDto)
        {
            IEnumerable<T> dtos = datasetSearchDto.SearchableTiles ?? GetDatasetTileDtos();

            if (!string.IsNullOrWhiteSpace(datasetSearchDto.SearchText))
            {
                dtos = dtos.Where(x => x.Name.ToLower().Contains(datasetSearchDto.SearchText.ToLower()));
            }

            return dtos;
        }

        #region Abstract
        protected abstract List<Dataset> GetDatasets();
        protected abstract T MapToTileDto(Dataset dataset);
        #endregion

        #region Private
        private IEnumerable<T> GetDatasetTileDtos()
        {
            List<Dataset> datasets = GetDatasets();

            string associateId = _userService.GetCurrentUser().AssociateId;
            List<T> datasetTileDtos = new List<T>();

            //map to DatasetTileDto
            foreach (Dataset dataset in datasets)
            {
                T datasetTileDto = MapToTileDto(dataset);
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

        private List<T> ApplyPaging(IEnumerable<T> dtos, TileSearchDto<T> datasetSearchDto)
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
