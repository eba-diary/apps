using Sentry.data.Core;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Web.Helpers;
using Sentry.data.Web.Models.AdminPage;
using System;
using System.Collections.Generic;

namespace Sentry.data.Web.Extensions
{
    public static class ProcessActivityExtensions
    {
        /// DatasetProcessActivityDto MapToModelList
        public static List<DatasetProcessActivityModel> MapToModelList(this List<DatasetProcessActivityDto> rootDtos)
        {
            List<DatasetProcessActivityModel> rootModels = new List<DatasetProcessActivityModel>();

            rootDtos.ForEach(rm => rootModels.Add(MapToModel(rm)));

            return rootModels;
        }

        /// SchemaProcessActivityDto MapToModelList
        public static List<SchemaProcessActivityModel> MapToModelList(this List<SchemaProcessActivityDto> rootDtos)
        {
            List<SchemaProcessActivityModel> rootModels = new List<SchemaProcessActivityModel>();

            rootDtos.ForEach(rm => rootModels.Add(MapToModel(rm)));

            return rootModels;
        }

        /// DatasetFileProcessActivityDto MapToModelList
        public static List<DatasetFileProcessActivityModel> MapToModelList(this List<DatasetFileProcessActivityDto> rootDtos)
        {
            List<DatasetFileProcessActivityModel> rootModels = new List<DatasetFileProcessActivityModel>();

            rootDtos.ForEach(rm => rootModels.Add(MapToModel(rm)));

            return rootModels;
        }

        /// DatasetProcessActivityDto MapToModel
        private static DatasetProcessActivityModel MapToModel(this DatasetProcessActivityDto rootDto)
        {
            DatasetProcessActivityModel rootModel = new DatasetProcessActivityModel();

            rootModel.DatasetName = rootDto.DatasetName;
            rootModel.DatasetId = rootDto.DatasetId;
            rootModel.LastEventTime = rootDto.LastEventTime;
            rootModel.FileCount = rootDto.FileCount;

            return rootModel;
        }

        /// SchemaProcessActivityDto MapToModel
        private static SchemaProcessActivityModel MapToModel(this SchemaProcessActivityDto rootDto)
        {
            SchemaProcessActivityModel rootModel = new SchemaProcessActivityModel();

            rootModel.SchemaName = rootDto.SchemaName;
            rootModel.SchemaId = rootDto.SchemaId;
            rootModel.DatasetId = rootDto.DatasetId;
            rootModel.LastEventTime = rootDto.LastEventTime;
            rootModel.FileCount = rootDto.FileCount;

            return rootModel;
        }

        /// DatasetFileProcessActivityDto MapToModel
        private static DatasetFileProcessActivityModel MapToModel(this DatasetFileProcessActivityDto rootDto)
        {
            DatasetFileProcessActivityModel rootModel = new DatasetFileProcessActivityModel();

            DataActionType lastFlowStep = Utility.FindEnumFromId<DataActionType>(rootDto.LastFlowStep);

            rootModel.FileName = rootDto.FileName;
            rootModel.LastFlowStep = lastFlowStep.GetDescription();
            rootModel.FlowExecutionGuid = rootDto.FlowExecutionGuid;
            rootModel.LastEventTime = rootDto.LastEventTime;

            return rootModel;
        }
    }
}