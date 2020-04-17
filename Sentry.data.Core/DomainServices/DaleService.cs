using System;
using System.Collections.Generic;
using System.Linq;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using Sentry.Common.Logging;


namespace Sentry.data.Core
{
    class DaleService : IDaleService
    {
        private readonly UserService _userService;
        private readonly IEventService _eventService;


        public DaleService(UserService userService, IEventService eventService)
        {
            _userService = userService;
            _eventService = eventService;
        }

        public void GetSearchResults(string search)
        {

        }




    }
}
