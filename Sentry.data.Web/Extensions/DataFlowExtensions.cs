﻿using Newtonsoft.Json;
using Sentry.Configuration;
using Sentry.data.Core;
using Sentry.data.Infrastructure;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web
{

    public static class DataFlowExtensions
    {
        private static string _bucket;
        private static string _awsRegion;
        private static string _isAwsConfigSet;
        private static bool _aws_v2;

        public static bool AWSv2Configuration
        {
            get
            {
                if (_isAwsConfigSet == null)
                {
                    using (IContainer container = Bootstrapper.Container.GetNestedContainer())
                    {
                        IDataFeatures featureFlags = container.GetInstance<IDataFeatures>();
                        _aws_v2 = featureFlags.Use_AWS_v2_Configuration_CLA_1488.GetValue();
                    }
                    _isAwsConfigSet = "true";
                }
                return _aws_v2;
            }
        }
        private static string RootBucket
        {
            get
            {
#pragma warning disable S3240 // The simplest possible condition syntax should be used
                if (_bucket == null)
#pragma warning restore S3240 // The simplest possible condition syntax should be used
                {
                    _bucket = AWSv2Configuration
                        ? Config.GetHostSetting("AWS2_0RootBucket")
                        : Config.GetHostSetting("AWSRootBucket");
                }
                return _bucket;
            }
        }
        private static string AWSRegion
        {
            get
            {
                if (_awsRegion == null)
                {
                    _awsRegion = AWSv2Configuration
                            ? Config.GetHostSetting("AWS2_0Region")
                            : Config.GetHostSetting("AWSRegion");
                }
                return _awsRegion;
            }
        }


        public static List<DFModel> ToModelList(this List<Core.DataFlowDto> dtoList)
        {
            List<DFModel> modelList = new List<DFModel>();
            foreach (Core.DataFlowDto dto in dtoList)
            {
                modelList.Add(dto.ToModel());
            }
            return modelList;
        }
        public static DFModel ToModel(this Core.DataFlowDto dto)
        {
            return new DFModel(dto) { };
        }

        public static Core.DataFlowDto ToDto(this DataFlowModel model)
        {
            Core.DataFlowDto dto = new Core.DataFlowDto
            {
                Id = model.DataFlowId,
                Name = "Blah",
                CreatedBy = model.CreatedBy,
                CreateDTM = model.CreatedDTM,
                IngestionType = model.IngestionType,
                IsCompressed = model.IsCompressed,
                IsPreProcessingRequired = model.IsPreProcessingRequired,
                PreProcessingOptions = model.PreprocessingOptions.Select(s => (DataFlowPreProcessingTypes)Enum.ToObject(typeof(DataFlowPreProcessingTypes), s)).ToList()
            };            

            if (model.SchemaMaps != null)
            {
                dto.SchemaMap = model.SchemaMaps.ToDto();
            }

            if (model.RetrieverJob != null)
            {
                dto.RetrieverJob = model.RetrieverJob.First().ToDto();
            }

            if (model.CompressionJob != null)
            {
                dto.CompressionJob = model.CompressionJob.First().ToDto();
            }

            dto.DFQuestionnaire = JsonConvert.SerializeObject(dto);

            return dto;
        }

        private static Core.CompressionJobDto ToDto(this CompressionModel model)
        {
            return new Core.CompressionJobDto()
            {
                CompressionType = (CompressionTypes)Enum.Parse(typeof(CompressionTypes), model.CompressionType),
                FileNameExclusionList = model.FileNameExclusionList
            };
        }

        public static List<Core.SchemaMapDto> ToDto(this List<SchemaMapModel> modelList)
        {
            List<Core.SchemaMapDto> dtoList = new List<Core.SchemaMapDto>();

            foreach(SchemaMapModel model in modelList)
            {
                dtoList.Add(model.ToDto());
            }

            return dtoList;
        }

        public static Core.SchemaMapDto ToDto(this SchemaMapModel model)
        {
            Core.SchemaMapDto dto = new Core.SchemaMapDto
            {
                Id = model.Id,
                SchemaId = model.SelectedSchema,
                DatasetId = model.SelectedDataset,
                SearchCriteria = model.SearchCriteria
            };

            return dto;
        }

        public static Core.RetrieverJobDto ToDto(this JobModel model)
        {
            Core.RetrieverJobDto dto = new Core.RetrieverJobDto()
            {
                DataSourceId = model.SelectedDataSource,
                DataSourceType = model.SelectedSourceType,
                IsCompressed = false, //for the data flow compression is handled outside of retriever job logic
                CreateCurrentFile = model.CreateCurrentFile,
                DatasetFileConfig = 0, //jobs for the data flow are linked via data flow id not datasetfileconfig
                FileNameExclusionList = null,
                FileSchema = 0,
                FtpPatrn = model.FtpPattern,
                HttpRequestBody = model.HttpRequestBody,
                JobId = 0,
                RelativeUri = model.RelativeUri,
                RequestDataFormat = model.SelectedRequestDataFormat,
                RequestMethod = model.SelectedRequestMethod,
                Schedule = model.Schedule,
                SearchCriteria = model.SearchCriteria,
                TargetFileName = model.TargetFileName
        };

            return dto;
        }

        public static DataFlowModel ToModel(this Core.DataFlowDetailDto dto)
        {
            DataFlowModel model = new DataFlowModel()
            {
                CreatedBy = dto.CreatedBy,
                DataFlowId = dto.Id,
                CreatedDTM = dto.CreateDTM,
                IsCompressed = dto.IsCompressed
            };

            if (dto.RetrieverJob != null)
            {
                JobModel jobModel = new JobModel()
                {
                    CreateCurrentFile = dto.RetrieverJob.CreateCurrentFile,
                    FtpPattern = dto.RetrieverJob.FtpPatrn,
                    HttpRequestBody = dto.RetrieverJob.HttpRequestBody,
                    IsRegexSearch = true,
                    OverwriteDataFile = false,
                    RelativeUri = dto.RetrieverJob.RelativeUri,
                    Schedule = dto.RetrieverJob.Schedule
                };
            }
            return model;
        }

        public static DataFlowStepModel ToModel(this Core.DataFlowStepDto dto)
        {
            DataFlowStepModel model = new DataFlowStepModel()
            {
                Id = dto.Id,
                ActionId = dto.ActionId,
                ActionName = dto.ActionName,
                ExecutionOrder = dto.ExeuctionOrder,
                TriggetKey = dto.TriggerKey,
                TargetPrefix = dto.TargetPrefix,
                RootAWSUrl = $"https://{AWSRegion.ToLower()}.amazonaws.com/{RootBucket.ToLower()}/"
            };
            return model;
        }

        public static List<AssociatedDataFlowModel> ToModel(this List<Tuple<DataFlowDetailDto, List<RetrieverJob>>> jobList)
        {
            List<AssociatedDataFlowModel> resultList = new List<AssociatedDataFlowModel>();
            foreach (Tuple<DataFlowDetailDto, List<RetrieverJob>> item in jobList)
            {
                resultList.Add(new AssociatedDataFlowModel(item));
            }
            return resultList;
        }
    }
}