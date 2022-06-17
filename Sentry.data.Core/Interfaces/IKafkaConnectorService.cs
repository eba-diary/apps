
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IKafkaConnectorService
    {
        ConfluentConnectorDTO GetConnectorDto(string resource);
    }
}
