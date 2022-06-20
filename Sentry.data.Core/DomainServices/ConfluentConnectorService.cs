using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.Configuration;
using Sentry.data.Core.Entities;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class ConfluentConnectorService : IKafkaConnectorService
    {
        public ConfluentConnectorRootDTO GetConnectorDto(HttpResponseMessage resources)
        {
            throw new NotImplementedException();
        }
    }
}
