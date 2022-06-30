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
        public static List<ConnectorRootModel> MapToModelList(this List<ConnectorRootDto> connectorRootDtos)
        {
            List<ConnectorRootModel> connectorRootModels = new List<ConnectorRootModel>();

            connectorRootDtos.ForEach(crd => connectorRootModels.Add(MapToModel(crd)));

            return connectorRootModels;
        }

        private static ConnectorRootModel MapToModel(this ConnectorRootDto rootDto)
        {
            ConnectorRootModel rootModel = new ConnectorRootModel();

            rootModel.ConnectorName = rootDto.ConnectorName;
            rootModel.ConnectorStatus = rootDto.ConnectorStatus.MapToStatusModel();
            rootModel.ConnectorInfo = rootDto.ConnectorInfo.MapToInfoModel();

            return rootModel;
        }

        private static List<ConnectorTaskModel> MapToTaskList(this List<ConnectorTaskDto> connectorTaskDtos)
        {
            List<ConnectorTaskModel> connectorTaskModels = new List<ConnectorTaskModel>();

            connectorTaskDtos.ForEach(ctd => connectorTaskModels.Add(ctd.MapToTaskModel()));

            return connectorTaskModels;
        }

        private static ConnectorInfoModel MapToInfoModel(this ConnectorInfoDto infoDto)
        {
            ConnectorInfoModel infoModel = new ConnectorInfoModel();

            infoModel.Name = infoDto.Name; 
            infoModel.Type = infoDto.Type;
            infoModel.ConnectorClass = infoDto.ConnectorClass;
            infoModel.S3Region = infoDto.S3Region;
            infoModel.FlushSize = infoDto.FlushSize;
            infoModel.TasksMax = infoDto.TasksMax;
            infoModel.timezone = infoDto.timezone;
            infoModel.transforms = infoDto.transforms;
            infoModel.locale = infoDto.locale;
            infoModel.S3PathStyleAccessEnabled = infoDto.S3PathStyleAccessEnabled;
            infoModel.FormatClass = infoDto.FormatClass;
            infoModel.S3AclCanned = infoDto.S3AclCanned;
            infoModel.TransformsInsertMetadataPartitionField = infoDto.TransformsInsertMetadataPartitionField;
            infoModel.ValueConverter = infoDto.ValueConverter;
            infoModel.S3ProxyPassword = infoDto.S3ProxyPassword;
            infoModel.KeyConverter = infoDto.KeyConverter;
            infoModel.TransformsInsertMetadataTimestampField = infoDto.TransformsInsertMetadataTimestampField;
            infoModel.S3BucketName = infoDto.S3BucketName;
            infoModel.PartitionDurationMs = infoDto.PartitionDurationMs;
            infoModel.S3ProxyUser = infoDto.S3ProxyUser;
            infoModel.S3SseaName = infoDto.S3SseaName;
            infoModel.FileDelim = infoDto.FileDelim;
            infoModel.TransformsInsertMetadataOffsetField = infoDto.TransformsInsertMetadataOffsetField;
            infoModel.topics = infoDto.topics;
            infoModel.PartitionerClass = infoDto.PartitionerClass;
            infoModel.ValueConverterSchemasEnable = infoDto.ValueConverterSchemasEnable;
            infoModel.StorageClass = infoDto.StorageClass;
            infoModel.RotateScheduleIntervalMs = infoDto.RotateScheduleIntervalMs;
            infoModel.PathFormat = infoDto.PathFormat;
            infoModel.TimestampExtractor = infoDto.TimestampExtractor;
            infoModel.S3ProxyUrl = infoDto.S3ProxyUrl;
            infoModel.TransformsInsertMetadataType = infoDto.TransformsInsertMetadataType;

            return infoModel;
        }

        private static ConnectorStatusModel MapToStatusModel(this ConnectorStatusDto statusDto)
        {
            ConnectorStatusModel statusModel = new ConnectorStatusModel();

            statusModel.Name = statusDto.Name;
            statusModel.State = statusDto.State;
            statusModel.WorkerId = statusDto.WorkerId;
            statusModel.Type = statusDto.Type;
            statusModel.ConnectorTasks = statusDto.ConnectorTasks.MapToTaskList();

            return statusModel;
        }

        private static ConnectorTaskModel MapToTaskModel(this ConnectorTaskDto taskDto)
        {
            ConnectorTaskModel taskModel = new ConnectorTaskModel();

            taskModel.Id = taskDto.Id;
            taskModel.State = taskDto.State;
            taskModel.Worker_Id = taskDto.Worker_Id;

            return taskModel;
        }
    }
}