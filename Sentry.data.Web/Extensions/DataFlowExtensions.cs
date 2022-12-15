using Nest;
using Newtonsoft.Json;
using Sentry.Configuration;
using Sentry.data.Core;
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
                    _aws_v2 = true;
                    _isAwsConfigSet = "true";
                }
                return _aws_v2;
            }
        }

        private static string AwsRegion
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
                modelList.Add(dto.ToDFModel());
            }
            return modelList;
        }

        public static List<Models.ApiModels.Dataflow.DataFlowStepModel> ToModelList(this List<Core.DataFlowStepDto> dtoList)
        {
            List<Models.ApiModels.Dataflow.DataFlowStepModel> modelList = new List<Models.ApiModels.Dataflow.DataFlowStepModel>();
            foreach (Core.DataFlowStepDto dto in dtoList)
            {
                modelList.Add(dto.ToAPIModel());
            }
            return modelList;
        }

        public static DFModel ToDFModel(this Core.DataFlowDto dto)
        {
            return new DFModel(dto) { };
        }

        public static Models.ApiModels.Dataflow.DataFlowDetailModel MapToModel(this DataFlowDetailDto dto)
        {
            Models.ApiModels.Dataflow.DataFlowDetailModel model = new Models.ApiModels.Dataflow.DataFlowDetailModel();

            model.steps = dto.steps.ToModelList();

            model.Id = dto.Id;
            model.FlowGuid = dto.FlowGuid;
            model.SaidKeyCode = dto.SaidKeyCode;
            model.DatasetId = dto.DatasetId;
            model.SchemaId = dto.SchemaId;
            model.Name = dto.Name;
            model.CreateDTM = dto.CreateDTM.ToString("s");
            model.CreatedBy = dto.CreatedBy;
            model.DFQuestionnaire = dto.DFQuestionnaire;
            model.IngestionType = dto.IngestionType;
            model.IsCompressed = dto.IsCompressed;
            model.IsBackFillRequired = dto.IsBackFillRequired;
            model.IsPreProcessingRequired = dto.IsPreProcessingRequired;
            model.PreProcessingOption = dto.PreProcessingOption;
            model.FlowStorageCode = dto.FlowStorageCode;
            model.AssociatedJobs = dto.AssociatedJobs;
            model.ObjectStatus = dto.ObjectStatus;
            model.DeleteIssuer = dto.DeleteIssuer;
            model.DeleteIssueDTM = dto.DeleteIssueDTM.ToString("s");
            model.NamedEnvironment = dto.NamedEnvironment;
            model.NamedEnvironmentType = dto.NamedEnvironmentType;
            model.TopicName = dto.TopicName;
            model.S3ConnectorName = dto.S3ConnectorName;

            return model;
        }

        public static List<Models.ApiModels.Dataflow.DataFlowDetailModel> MapToDetailModelList(this List<DataFlowDetailDto> detailDto)
        {
            List<Models.ApiModels.Dataflow.DataFlowDetailModel> detailModelList = new List<Models.ApiModels.Dataflow.DataFlowDetailModel>();

            foreach (DataFlowDetailDto dto in detailDto)
            {
                Models.ApiModels.Dataflow.DataFlowDetailModel detail = dto.MapToModel();
                detailModelList.Add(detail);
            }

            return detailModelList;
        }

        public static DataFlowDto ToDto(this DataFlowModel model, string currentUser)
        {
            DataFlowDto dto = new DataFlowDto
            {
                Id = model.DataFlowId,
                Name = model.Name,
                SaidKeyCode = model.SAIDAssetKeyCode,
                CreatedBy = model.CreatedBy,
                CreateDTM = model.CreatedDTM,
                IngestionType = model.IngestionTypeSelection,
                IsCompressed = model.IsCompressed,
                IsBackFillRequired = model.IsBackFillRequired,
                IsPreProcessingRequired = model.IsPreProcessingRequired,
                PreProcessingOption = model.PreProcessingSelection,
                ObjectStatus = model.ObjectStatus,
                FlowStorageCode = model.StorageCode,
                NamedEnvironment = model.NamedEnvironment,
                NamedEnvironmentType = model.NamedEnvironmentType,
                PrimaryContactId = (model.PrimaryContactId) ?? currentUser,
                IsSecured = true,
                TopicName = model.TopicName,
                S3ConnectorName = model.S3ConnectorName
            };

            if (model.SchemaMaps != null)
            {
                dto.SchemaMap = model.SchemaMaps.ToDto();
            }

            if (model.RetrieverJob != null)
            {
                dto.RetrieverJob = model.RetrieverJob.ToDto();
            }

            if (model.IsCompressed)
            {
                CompressionJobDto cDto = model.CompressionJob.First().ToDto();
                dto.CompressionJob = cDto;
                dto.CompressionType = (int)cDto.CompressionType;
            }
            else
            {
                dto.CompressionType = null;
            }

            dto.DFQuestionnaire = JsonConvert.SerializeObject(dto);

            return dto;
        }

        public static Core.CompressionJobDto ToDto(this CompressionModel model)
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
                SearchCriteria = model.SearchCriteria,
                IsDeleted = model.IsDeleted
            };

            return dto;
        }

        public static RetrieverJobDto ToDto(this JobModel model)
        {
            return new RetrieverJobDto()
            {
                DataSourceId = (string.IsNullOrEmpty(model.SelectedDataSource)) ? 0 : int.Parse(model.SelectedDataSource),
                DataSourceType = model.SelectedSourceType,
                IsCompressed = false, //for the data flow compression is handled outside of retriever job logic
                CreateCurrentFile = model.CreateCurrentFile,
                DatasetFileConfig = 0, //jobs for the data flow are linked via data flow id not datasetfileconfig
                FileNameExclusionList = null,
                FileSchema = 0,
                FtpPattern = model.FtpPattern,
                HttpRequestBody = model.HttpRequestBody,
                JobId = 0,
                RelativeUri = model.RelativeUri,
                RequestDataFormat = model.SelectedRequestDataFormat,
                RequestMethod = model.SelectedRequestMethod,
                Schedule = model.Schedule,
                SearchCriteria = model.SearchCriteria,
                TargetFileName = model.TargetFileName,
                ExecutionParameters = model.ExecutionParameters,
                PagingType = model.PagingType,
                PageTokenField= model.PageTokenField,
                PageParameterName= model.PageParameterName,
                RequestVariables = model.RequestVariables?.Select(x => x.ToDto()).ToList()
            };
        }

        public static RequestVariableDto ToDto(this RequestVariableModel model)
        {
            return new RequestVariableDto
            {
                VariableName = model.VariableName,
                VariableValue = model.VariableValue,
                VariableIncrementType = model.VariableIncrementType
            };
        }

        public static RequestVariableModel ToModel(this RequestVariableDto dto)
        {
            return new RequestVariableModel
            {
                VariableName = dto.VariableName,
                VariableValue = dto.VariableValue,
                VariableIncrementType = dto.VariableIncrementType
            };
        }

        public static DataFlowStepModel ToModel(this Core.DataFlowStepDto dto)
        {
            DataFlowStepModel model = new DataFlowStepModel()
            {
                Id = dto.Id,
                ActionId = dto.ActionId,
                ActionName = dto.ActionName,
                ActionDescription = dto.ActionDescription,
                ExecutionOrder = dto.ExeuctionOrder,
                TriggetKey = dto.TriggerKey,
                TargetPrefix = dto.TargetPrefix,
                RootAwsUrl = $"https://{AwsRegion.ToLower()}.amazonaws.com/{dto.TriggerBucket}/"
            };
            return model;
        }

        public static SchemaMapModel ToModel(this SchemaMapDto dto)
        {
            SchemaMapModel model = new SchemaMapModel();

            model.Id = dto.Id;
            model.SearchCriteria = dto.SearchCriteria;
            model.SelectedDataset = dto.DatasetId;
            model.SelectedSchema = dto.SchemaId;

            return model;
        }

        public static List<AssociatedDataFlowModel> ToModel(this List<Tuple<DataFlowDetailDto, List<RetrieverJob>>> jobList)
        {
            List<AssociatedDataFlowModel> resultList = new List<AssociatedDataFlowModel>();
            foreach (Tuple<DataFlowDetailDto, List<RetrieverJob>> item in jobList)
            {
                resultList.Add(item.ToModel());
            }
            return resultList;
        }

        public static AssociatedDataFlowModel ToModel(this Tuple<DataFlowDetailDto, List<RetrieverJob>> job)
        {
            return new AssociatedDataFlowModel(job);
        }

        public static SchemaMapDetailModel ToDetailModel(this Core.SchemaMapDetailDto dto)
        {
            SchemaMapDetailModel model = new SchemaMapDetailModel(dto);
            return model;
        }

        public static List<SchemaMapDetailModel> ToDetailModelList(this List<Core.SchemaMapDetailDto> dtoList)
        {
            List<SchemaMapDetailModel> modelList = new List<SchemaMapDetailModel>();
            foreach (Core.SchemaMapDetailDto dto in dtoList)
            {
                modelList.Add(ToDetailModel(dto));
            }
            return modelList;
        }

        public static Models.ApiModels.Dataflow.DataFlowStepModel ToAPIModel(this Core.DataFlowStepDto dto)
        {
            Models.ApiModels.Dataflow.DataFlowStepModel model = new Models.ApiModels.Dataflow.DataFlowStepModel()
            {
                Id = dto.Id,
                ActionId = dto.ActionId,
                ActionName = dto.ActionName,
                ActionDescription = dto.ActionDescription,
                ExecutionOrder = dto.ExeuctionOrder,
                TriggetKey = dto.TriggerKey,
                TargetPrefix = dto.TargetPrefix,
                RootAwsUrl = $"https://{AwsRegion.ToLower()}.amazonaws.com/{dto.TriggerBucket}/"
            };
            return model;
        }
    }
}