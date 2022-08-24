using Newtonsoft.Json;
using Sentry.Associates;
using Sentry.Common.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public abstract class TileSearchService<T> : ITileSearchService<T> where T : DatasetTileDto
    {
        protected readonly IDatasetContext _datasetContext;
        private readonly IUserService _userService;
        private readonly IEventService _eventService;

        protected TileSearchService(IDatasetContext datasetContext, IUserService userService, IEventService eventService)
        {
            _datasetContext = datasetContext;
            _userService = userService;
            _eventService = eventService;
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
                List<T> dtos = searchDto.SearchableTiles?.Any() == true ? searchDto.SearchableTiles : GetSearchableTiles();

                if (!string.IsNullOrWhiteSpace(searchDto.SearchText))
                {
                    dtos = dtos.Where(x => x.Name.ToLower().Contains(searchDto.SearchText.ToLower())).ToList();
                }

                List<T> allResults = dtos.FilterBy(searchDto.FilterCategories);

                if (searchDto.UpdateFilters)
                {
                    resultDto.FilterCategories = dtos.CreateFilters(searchDto.FilterCategories);
                }

                resultDto.TotalResults = allResults.Count;
                resultDto.Tiles = ApplyPaging(allResults, searchDto);
            }
            catch (Exception ex)
            {
                Logger.Error("Error searching datasets", ex);
            }

            return resultDto;
        }

        public List<T> GetSearchableTiles()
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();
            List<Dataset> datasets = GetDatasets().FetchAllChildren(_datasetContext);
            sw.Stop();
            Logger.Info($"GetSearchableTiles - GetDatasets {sw.ElapsedMilliseconds}ms");

            sw.Restart();
            string associateId = _userService.GetCurrentUser().AssociateId;
            sw.Stop();
            Logger.Info($"GetSearchableTiles - GetCurrentUser {sw.ElapsedMilliseconds}ms");

            sw.Restart();
            List<Task<T>> tasks = datasets.Select(x => MapAsync(x, associateId)).ToList();
            List<T> tileDtos = tasks.Select(x => x.Result).ToList();
            sw.Stop();
            Logger.Info($"GetSearchableTiles - Map {sw.ElapsedMilliseconds}ms");

            return tileDtos;
        }

        public async Task PublishSearchEventAsync(TileSearchEventDto eventDto)
        {
            string serializedSearch = JsonConvert.SerializeObject(eventDto);
            await _eventService.PublishSuccessEvent(GlobalConstants.EventType.SEARCH, "Searched Datasets", serializedSearch);
        }

        #region Abstract
        protected abstract IQueryable<Dataset> GetDatasets();
        protected abstract T MapToTileDto(Dataset dataset);
        #endregion

        #region Private
        private async Task<T> MapAsync(Dataset dataset, string associateId)
        {
            return await Task.Run(() =>
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

                return tileDto;
            }).ConfigureAwait(false);
        }

        private List<T> ApplyPaging(IEnumerable<T> dtos, TileSearchDto<T> searchDto)
        {
            if (searchDto.OrderByDescending)
            {
                dtos = dtos.OrderByDescending(searchDto.OrderByField).ThenBy(x => x.Name);
            }
            else
            {
                dtos = dtos.OrderBy(searchDto.OrderByField).ThenBy(x => x.Name);
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
