using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.Core;
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
            List<T> tileDtos = new List<T>();

            try
            {
                Stopwatch sw = new Stopwatch();

                sw.Start();
                IQueryable<Dataset> datasetQueryable = GetDatasets();
                List<Dataset> datasets = datasetQueryable.FetchAllChildren(_datasetContext);
                sw.Stop();
                Logger.Info($"GetSearchableTiles - GetDatasets {sw.ElapsedMilliseconds}ms");

                sw.Restart();
                Dictionary<int, DateTime> datasetFileDates = _datasetContext.DatasetFileStatusActive.GroupBy(x => x.Dataset).
                    Select(x => new KeyValuePair<int, DateTime>(x.Key.DatasetId, x.Max(m => m.CreatedDTM))).
                    ToDictionary(x => x.Key, x => x.Value);
                sw.Stop();
                Logger.Info($"GetSearchableTiles - MaxDatasetFileDate {sw.ElapsedMilliseconds}ms");

                sw.Restart();
                var datasetIdSaidCodes = _datasetContext.DataFlow.Where(x => x.ObjectStatus != GlobalEnums.ObjectStatusEnum.Deleted).
                    GroupBy(x => new { x.DatasetId, x.SaidKeyCode }).
                    Select(x => x.Key).
                    ToList();

                Dictionary<int, List<string>> producerAssets = datasetIdSaidCodes.GroupBy(x => x.DatasetId).
                    ToDictionary(x => x.Key, 
                                 v => v.Select(d => d.SaidKeyCode).ToList());
                sw.Stop();
                Logger.Info($"GetSearchableTiles - ProducerAssets {sw.ElapsedMilliseconds}ms");

                string associateId = _userService.GetCurrentUser().AssociateId;

                List<Task<T>> tasks = datasets.Select(x => MapAsync(x, associateId, datasetFileDates, producerAssets)).ToList();
                tileDtos = tasks.Select(x => x.Result).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error("Error getting searchable tiles", ex);
            }

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
        private async Task<T> MapAsync(Dataset dataset, string associateId, Dictionary<int, DateTime> datasetFileDates, Dictionary<int, List<string>> producerAssets)
        {
            return await Task.Run(() => {
                T tileDto = MapToTileDto(dataset);
                tileDto.IsFavorite = dataset.Favorities.Any(w => w.UserId == associateId);

                tileDto.LastActivityDateTime = dataset.ChangedDtm;
                if (datasetFileDates.TryGetValue(dataset.DatasetId, out DateTime maxDate) && maxDate > tileDto.LastActivityDateTime)
                {
                    tileDto.LastActivityDateTime = maxDate;
                }

                tileDto.ProducerAssets = new List<string>();
                if (producerAssets.TryGetValue(dataset.DatasetId, out List<string> assets))
                {
                    tileDto.ProducerAssets = assets;
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
