using Sentry.data.Core.Exceptions;
using System;
using System.Collections.Generic;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class DataInventoryService : IDataInventoryService
    {
        private readonly UserService _userService;
        private readonly IEventService _eventService;
        private readonly IDataInventorySearchProvider _dataInventorySearchProvider;

        public DataInventoryService(UserService userService, IEventService eventService, IDataInventorySearchProvider dataInventorySearchProvider)
        {
            _userService = userService;
            _eventService = eventService;
            _dataInventorySearchProvider = dataInventorySearchProvider;
        }

        public DataInventorySearchResultDto GetSearchResults(FilterSearchDto dtoSearch)
        {
            DataInventorySearchResultDto dtoResult = _dataInventorySearchProvider.GetSearchResults(dtoSearch);

            string queryBlob = Newtonsoft.Json.JsonConvert.SerializeObject(dtoResult.DataInventoryEvent);
            _eventService.PublishSuccessEvent(GlobalConstants.EventType.DATA_INVENTORY_SEARCH, "Data Inventory Query Executed", queryBlob);

            return dtoResult;
        }

        public FilterSearchDto GetSearchFilters(FilterSearchDto dtoSearch)
        {
            FilterSearchDto dtoResult = _dataInventorySearchProvider.GetSearchFilters(dtoSearch);
            return dtoResult;
        }

        public bool UpdateIsSensitive(List<DataInventoryUpdateDto> dtos)
        {
            return _dataInventorySearchProvider.SaveSensitive(dtos);
        }

        public bool DoesItemContainSensitive(DataInventorySensitiveSearchDto dtoSearch)
        {
            DataInventorySensitiveSearchResultDto dtoResult;

            CanViewSensitive();

            //validate for white space only, null, empty string in criteria
            if (string.IsNullOrWhiteSpace(dtoSearch.SearchText) || string.IsNullOrWhiteSpace(dtoSearch.SearchTarget))
            {
                throw new DataInventoryInvalidSearchException();
            }

            dtoResult = _dataInventorySearchProvider.DoesItemContainSensitive(dtoSearch);

            if(!dtoResult.DataInventoryEvent.QuerySuccess)
            {
                throw new DataInventoryQueryException();
            }

            return dtoResult.HasSensitive;
        }

        public List<DataInventoryCategoryDto> GetCategoriesByAsset(string search)
        {
            DataInventoryAssetCategoriesDto dtoResult;

            CanViewSensitive();

            if (string.IsNullOrWhiteSpace(search))
            {
                throw new DataInventoryInvalidSearchException();
            }

            dtoResult = _dataInventorySearchProvider.GetCategoriesByAsset(search);

            if (!dtoResult.DataInventoryEvent.QuerySuccess)
            {
                throw new DataInventoryQueryException();
            }

            return dtoResult.DataInventoryCategories;
        }

        public bool TryGetCategoryName(string category, out string categoryName)
        {
            if (string.Equals(category, "said", StringComparison.OrdinalIgnoreCase))
            {
                category = FilterCategoryNames.ASSET;
            }
            
            return CustomAttributeHelper.TryGetFilterCategoryName<DataInventory>(category, out categoryName);
        }

        private void CanViewSensitive()
        {
            if (!_userService.GetCurrentUser().CanViewSensitiveDataInventory)
            {
                throw new DataInventoryUnauthorizedAccessException();
            }
        }
    }
}
