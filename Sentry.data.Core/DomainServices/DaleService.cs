using System;
using System.Collections.Generic;
using System.Linq;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.Common.Logging;



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
            List<DaleResultDto> daleResults = _daleSearchProvider.GetSearchResults(dto);
            return daleResults;
        }
    }
}
