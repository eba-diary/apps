using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public class ConnectorCreateResponseDto
    {
        public bool SuccessStatusCode { get; set; }
        public string SuccessStatusCodeDescription { get; set; }
        public string StatusCode { get; set; }
        public string ReasonPhrase { get; set; }
    }
}
