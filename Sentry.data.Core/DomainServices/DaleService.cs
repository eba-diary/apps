using Sentry.data.Core.Exceptions;
using System.Collections.Generic;

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
            _eventService.PublishSuccessEvent("DaleQuery", _userService.GetCurrentUser().AssociateId, "Dale Query Executed", null, queryBlob);

            return dtoResult;
        }

        public bool UpdateIsSensitive(List<DaleSensitiveDto> dtos)
        {

            bool success = false;

            string sensitiveBlob = Newtonsoft.Json.JsonConvert.SerializeObject(dtos);
            success = _daleSearchProvider.SaveSensitive(sensitiveBlob);

            return success;
        }

        public DaleContainSensitiveResultDto DoesItemContainSensitive(DaleSearchDto dto)
        {
            DaleContainSensitiveResultDto dtoResult;

            if (CanDaleSensitiveView())
            {
                dtoResult = _daleSearchProvider.DoesItemContainSensitive(dto);
            }
            else
            {
                throw new DaleUnauthorizedAccessException();
            }

            if(!dtoResult.DaleEvent.QuerySuccess)
            {
                throw new DaleQueryException();
            }

            return dtoResult;
        }

        private bool CanDaleSensitiveView()
        {
            if (!_userService.GetCurrentUser().CanDaleSensitiveView)
            {
                return false;
            }
            return true;
        }
    }
}
