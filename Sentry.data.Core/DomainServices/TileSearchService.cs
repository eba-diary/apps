using Sentry.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public abstract class TileSearchService<T> : ITileSearchService<T> where T : DatasetTileDto
    {
        protected readonly IDatasetContext _datasetContext;
        private readonly IUserService _userService;

        protected TileSearchService(IDatasetContext datasetContext, IUserService userService)
        {
            _datasetContext = datasetContext;
            _userService = userService;
        }

        public TileSearchResultDto<T> SearchTiles(TileSearchDto<T> searchDto)
        {
            TileSearchResultDto<T> resultDto = new TileSearchResultDto<T>()
            {
                PageSize = searchDto.PageSize,
                PageNumber = searchDto.PageNumber
            };

            try
            {
                IEnumerable<T> dtos = SearchTileDtos(searchDto);
                List<T> allResults = dtos.FilterBy(searchDto.FilterCategories).ToList();

                resultDto.TotalResults = allResults.Count;
                resultDto.Tiles = ApplyPaging(allResults, searchDto);
            }
            catch (Exception ex)
            {
                Logger.Error("Error searching datasets", ex);
            }

            return resultDto;
        }

        public IEnumerable<T> SearchTileDtos(TileSearchDto<T> searchDto)
        {
            IEnumerable<T> dtos = searchDto.SearchableTiles?.Any() == true ? searchDto.SearchableTiles : GetTileDtos();

            if (!string.IsNullOrWhiteSpace(searchDto.SearchText))
            {
                dtos = dtos.Where(x => x.Name.ToLower().Contains(searchDto.SearchText.ToLower()));
            }

            return dtos;
        }

        #region Abstract
        protected abstract IQueryable<Dataset> GetDatasets();
        protected abstract T MapToTileDto(Dataset dataset);
        #endregion

        #region Private
        private IEnumerable<T> GetTileDtos()
        {
            List<Dataset> datasets = GetDatasets().FetchAllChildren(_datasetContext);

            string associateId = _userService.GetCurrentUser().AssociateId;
            List<T> tileDtos = new List<T>();

            //map to DatasetTileDto
            foreach (Dataset dataset in datasets)
            {
                T tileDto = MapToTileDto(dataset);
                tileDto.IsFavorite = dataset.Favorities.Any(w => w.UserId == associateId);

                tileDto.LastActivityDateTime = dataset.ChangedDtm;
                if (dataset.DatasetFiles?.Any() == true)
                {
                    DateTime lastFileDate = dataset.DatasetFiles.Max(x => x.CreatedDTM);
                    if (lastFileDate > tileDto.LastActivityDateTime)
                    {
                        tileDto.LastActivityDateTime = lastFileDate;
                    }
                }

                tileDtos.Add(tileDto);
            }

            return tileDtos;
        }

        private List<T> ApplyPaging(IEnumerable<T> dtos, TileSearchDto<T> searchDto)
        {
            if (searchDto.OrderByDescending)
            {
                dtos = dtos.OrderByDescending(searchDto.OrderByField);
            }
            else
            {
                dtos = dtos.OrderBy(searchDto.OrderByField);
            }

            if (searchDto.PageSize > 0)
            {
                dtos = dtos.Skip(searchDto.PageSize * (searchDto.PageNumber - 1)).Take(searchDto.PageSize);
            }

            return dtos.ToList();
        }
        #endregion
    }
}
