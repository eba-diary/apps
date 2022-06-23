using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Web
{
    public class ConnectorTaskModel
    {
        public int Id { get; set; } 
        public string State { get; set; }   
        public string Worker_Id { get; set; }
    }
}
