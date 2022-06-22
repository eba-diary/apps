
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public interface IKafkaConnectorService
    {
        Task<ConfluentConnectorRootDTO> GetConnectorDto(string resource);
        Task<List<ConfluetConnectorRoot>> GetConnectorList();
    }
}
