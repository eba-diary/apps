using Newtonsoft.Json;
using Sentry.Configuration;
using Sentry.data.Core;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web
{
    public static class ConnectorExtensions
    {
        public static List<ConnectorModel> MapToModelList(this List<ConnectorDto> connectorDtos)
        {
            List<ConnectorModel> connectorModels = new List<ConnectorModel>();

            connectorDtos.ForEach(crd => connectorModels.Add(MapToModel(crd)));

            return connectorModels;
        }

        private static ConnectorModel MapToModel(this ConnectorDto rootDto)
        {
            ConnectorModel rootModel = new ConnectorModel();

            rootModel.ConnectorName = rootDto.ConnectorName;
            rootModel.ConnectorState = rootDto.ConnectorState;

            return rootModel;
        }
    }
}