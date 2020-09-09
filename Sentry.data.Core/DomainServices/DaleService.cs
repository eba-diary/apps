using System;
using System.Collections.Generic;
using System.Diagnostics;

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

        public List<DaleResultDto> GetSearchResults(DaleSearchDto dto)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            List<DaleResultDto> daleResults = _daleSearchProvider.GetSearchResults(dto);

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;

            DaleEventDto daleEventDto = new DaleEventDto()
            {
                Criteria = dto.Criteria,
                Destiny = dto.Destiny.GetDescription(),
                QuerySeconds = ts.Seconds,
                QueryRows = daleResults.Count
            };

            string queryBlob = Newtonsoft.Json.JsonConvert.SerializeObject(daleEventDto);
            _eventService.PublishSuccessEvent("DaleQuery", _userService.GetCurrentUser().AssociateId,"Dale Query Executed", null, queryBlob);

            return daleResults;
        }

        public bool UpdateIsSensitive(List<DaleSensitiveDto> dtos)
        {

            bool success = false;

            string sensitiveBlob = Newtonsoft.Json.JsonConvert.SerializeObject(dtos);
            success = _daleSearchProvider.SaveSensitive(sensitiveBlob);
            
            return success;
        }
    }
}
