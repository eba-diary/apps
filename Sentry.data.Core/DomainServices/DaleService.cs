using Sentry.data.Core.Exceptions;
using System.Collections.Generic;
using Sentry.data.Core.GlobalEnums;
using System;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class DaleService : IDaleService
    {
        private readonly UserService _userService;
        private readonly IEventService _eventService;
        private readonly IDaleSearchProvider _daleSearchProvider;

        public DaleService(UserService userService, IEventService eventService, IDaleSearchProvider daleSearchProvider)
        {
            _userService = userService;
            _eventService = eventService;
            _daleSearchProvider = daleSearchProvider;
        }

        public DaleResultDto GetSearchResults(DaleSearchDto dtoSearch)
        {
            DaleResultDto dtoResult = _daleSearchProvider.GetSearchResults(dtoSearch);

            string queryBlob = Newtonsoft.Json.JsonConvert.SerializeObject(dtoResult.DaleEvent);
            _eventService.PublishSuccessEvent(GlobalConstants.EventType.DALE_SEARCH, "Dale Query Executed", queryBlob);

            return dtoResult;
        }

        public FilterSearchDto GetSearchFilters(DaleSearchDto dtoSearch)
        {
            FilterSearchDto dtoResult = _daleSearchProvider.GetSearchFilters(dtoSearch);
            
            string queryBlob = Newtonsoft.Json.JsonConvert.SerializeObject(dtoResult.DaleEvent);
            _eventService.PublishSuccessEvent(GlobalConstants.EventType.DALE_SEARCH, "Data Inventory Filter Query Executed", queryBlob);

            return dtoResult;
        }

        public bool UpdateIsSensitive(List<DaleSensitiveDto> dtos)
        {
            return _daleSearchProvider.SaveSensitive(dtos);
        }

        public DaleContainSensitiveResultDto DoesItemContainSensitive(DaleSearchDto dtoSearch)
        {
            DaleContainSensitiveResultDto dtoResult;

            CanDaleSensitiveView();
            IsCriteriaValid(dtoSearch);
            
            dtoResult = _daleSearchProvider.DoesItemContainSensitive(dtoSearch);

            if(!dtoResult.DaleEvent.QuerySuccess)
            {
                throw new DaleQueryException();
            }

            return dtoResult;
        }

        public DaleCategoryResultDto GetCategoriesByAsset(string search)
        {
            DaleCategoryResultDto dtoResult;

            CanDaleSensitiveView();

            if (string.IsNullOrWhiteSpace(search))
            {
                throw new DaleInvalidSearchException();
            }


            dtoResult = _daleSearchProvider.GetCategoriesByAsset(search);

            if (!dtoResult.DaleEvent.QuerySuccess)
            {
                throw new DaleQueryException();
            }

            return dtoResult;
        }

        public bool TryGetCategoryName(string category, out string categoryName)
        {
            if (string.Equals(category, "said", StringComparison.OrdinalIgnoreCase))
            {
                category = FilterCategoryNames.ASSET;
            }
            
            return CustomAttributeHelper.TryGetFilterCategoryName<DataInventory>(category, out categoryName);
        }

        private void CanDaleSensitiveView()
        {
            if (!_userService.GetCurrentUser().CanDaleSensitiveView)
            {
                throw new DaleUnauthorizedAccessException();
            }
        }

        private void IsCriteriaValid(DaleSearchDto dto)
        {
            if (dto.Sensitive == DaleSensitive.SensitiveOnly)
            {
                return;
            }

            //validate for white space only, null, empty string in criteria
            if (String.IsNullOrWhiteSpace(dto.Criteria))
            {
                throw new DaleInvalidSearchException();
            }

            //validate to ensure valid destination
            if
            ((dto.Destiny != DaleDestiny.Object)
                    && (dto.Destiny != DaleDestiny.Column)
                    && (dto.Destiny != DaleDestiny.SAID)
                    && (dto.Destiny != DaleDestiny.Database)
                    && (dto.Destiny != DaleDestiny.Server)
            )
            {
                throw new DaleInvalidSearchException();
            }
        }
    }
}
