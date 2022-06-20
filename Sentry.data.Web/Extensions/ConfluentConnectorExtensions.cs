using Newtonsoft.Json;
using Sentry.Configuration;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web
{

    public static class ConfluentConnectorExtensions
    {
        public static List<ConfluentConnectorRootModel> MaptoRootModelList(this List<ConfluentConnectorRootDTO> connectorRootDTOList, List<ConfluentConnectorRootModel> confluentConnectorRootModelList)
        {
            foreach (ConfluentConnectorRootDTO confluentConnectorRootDTO in connectorRootDTOList)
            {
                ConfluentConnectorRootModel confluentConnectorRootModel = new ConfluentConnectorRootModel();
                confluentConnectorRootModelList.Add(MaptoRootModel(confluentConnectorRootDTO, confluentConnectorRootModel));
            }

            return confluentConnectorRootModelList;
        }

        public static ConfluentConnectorRootModel MaptoRootModel(this ConfluentConnectorRootDTO connectorRootDTO, ConfluentConnectorRootModel confluentConnectorRootModel)
        {
            confluentConnectorRootModel.name = connectorRootDTO.name;
            confluentConnectorRootModel.type = connectorRootDTO.type;

            confluentConnectorRootModel.connector = MaptoConnectorModel(connectorRootDTO.connector);

            List<ConfluentConnectorTaskModel> confluentConnectorTaskModels = new List<ConfluentConnectorTaskModel>();
            confluentConnectorRootModel.tasks = MapToTaskModelList(connectorRootDTO.tasks, confluentConnectorTaskModels);

            return confluentConnectorRootModel;
        }

        public static ConfluentConnectorModel MaptoConnectorModel(ConfluentConnectorDTO connectorDTO)
        {
            ConfluentConnectorModel connectorModel = new ConfluentConnectorModel();

            connectorModel.state = connectorDTO.state;
            connectorModel.worker_id = connectorDTO.worker_id;

            return connectorModel;
        }

        public static List<ConfluentConnectorTaskModel> MapToTaskModelList(List<ConfluentConnectorTaskDTO> connectorDTOTasks, List<ConfluentConnectorTaskModel> connectorTaskModels)
        {
            foreach (ConfluentConnectorTaskDTO tast in connectorDTOTasks)
            {
                ConfluentConnectorTaskModel confluentConnectorTask = new ConfluentConnectorTaskModel();
                connectorTaskModels.Add(MapToTaskModel(tast, confluentConnectorTask));
            }

            return connectorTaskModels;
        }

        public static ConfluentConnectorTaskModel MapToTaskModel(ConfluentConnectorTaskDTO connectorTaskDTO, ConfluentConnectorTaskModel connectorTaskModel)
        {
            connectorTaskModel.id = connectorTaskDTO.id;
            connectorTaskModel.state = connectorTaskDTO.state;
            connectorTaskModel.worker_id = connectorTaskDTO.worker_id;

            return connectorTaskModel;
        }
    }
}