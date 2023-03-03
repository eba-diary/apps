using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.Jira;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class SchemaFlowService : ISchemaFlowService
    {
        private readonly IConfigService _configService;
        private readonly ISchemaService _schemaService;
        private readonly IDataFlowService _dataFlowService;
        private readonly IDatasetContext _datasetContext;
        private readonly IUserService _userService;
        private readonly ISecurityService _securityService;

        public SchemaFlowService(IConfigService configService, ISchemaService schemaService, IDataFlowService dataFlowService, IDatasetContext datasetContext, IUserService userService, ISecurityService securityService)
        {
            _configService = configService;
            _schemaService = schemaService;
            _dataFlowService = dataFlowService;
            _datasetContext = datasetContext;
            _userService = userService;
            _securityService = securityService;
        }

        public async Task<SchemaResultDto> AddSchemaAsync(SchemaFlowDto dto)
        {
            //get dataset
            Dataset dataset = _datasetContext.GetById(dto.SchemaDto.ParentDatasetId);

            //check permission to dataset
            IApplicationUser user = _userService.GetCurrentUser();
            UserSecurity security = _securityService.GetUserSecurity(dataset, user);

            if (security.CanManageSchema)
            {
                try
                {
                    //create schema
                    FileSchemaDto addedSchemaDto = await _schemaService.AddSchemaAsync(dto.SchemaDto);

                    //create file config
                    dto.DatasetFileConfigDto.SchemaId = addedSchemaDto.SchemaId;
                    dto.DatasetFileConfigDto.FileExtensionId = addedSchemaDto.FileExtensionId;
                    _configService.Create(dto.DatasetFileConfigDto);

                    //create data flow
                    PrepDataFlowDto(dto, dataset, addedSchemaDto);
                    DataFlowDto addedDataFlowDto = await _dataFlowService.AddDataFlowAsync(dto.DataFlowDto);

                    SchemaResultDto resultDto = CreateSchemaResultDto(addedSchemaDto, dto.DatasetFileConfigDto, addedDataFlowDto);
                    return resultDto;
                }
                catch (Exception)
                {
                    _datasetContext.Clear();
                    throw;
                }
            }
            else
            {
                throw new ResourceForbiddenException(user.AssociateId, nameof(security.CanManageSchema), "AddSchema");
            }
        }

        public async Task<SchemaResultDto> UpdateSchemaAsync(SchemaFlowDto dto)
        {
            //get schema
            FileSchema schema = _datasetContext.FileSchema.FirstOrDefault(x => x.SchemaId == dto.SchemaDto.SchemaId && x.ObjectStatus == ObjectStatusEnum.Active);
            DatasetFileConfig fileConfig = _datasetContext.DatasetFileConfigs.FirstOrDefault(x => x.Schema.SchemaId == dto.SchemaDto.SchemaId && !x.DeleteInd);
            DataFlow dataFlow = _datasetContext.DataFlow.FirstOrDefault(x => x.SchemaId == schema.SchemaId && x.ObjectStatus == ObjectStatusEnum.Active);

            if (schema != null && fileConfig != null && dataFlow != null)
            {
                //check permission to dataset
                IApplicationUser user = _userService.GetCurrentUser();
                UserSecurity security = _securityService.GetUserSecurity(fileConfig.ParentDataset, user);

                if (security.CanManageSchema)
                {
                    try
                    {
                        //keep values that are not available to edit from API
                        dto.SchemaDto.ParquetStorageBucket = schema.ParquetStorageBucket;
                        dto.SchemaDto.ParquetStoragePrefix = schema.ParquetStoragePrefix;
                        if (dto.DataFlowDto.IngestionType == 0)
                        {
                            dto.SchemaDto.CLA1286_KafkaFlag = schema.CLA1286_KafkaFlag;
                        }

                        bool currentViewChanged = dto.SchemaDto.CreateCurrentView != schema.CreateCurrentView;

                        //if schema root path went from null to value or value to null need to update dataflow
                        dto.DataFlowDto.DataFlowStepUpdateRequired = (string.IsNullOrWhiteSpace(dto.SchemaDto.SchemaRootPath) && !string.IsNullOrWhiteSpace(schema.SchemaRootPath)) ||
                            (!string.IsNullOrWhiteSpace(dto.SchemaDto.SchemaRootPath) && string.IsNullOrWhiteSpace(schema.SchemaRootPath));

                        FileSchemaDto updatedSchemaDto = await _schemaService.UpdateSchemaAsync(dto.SchemaDto, schema);

                        //update file config
                        dto.DatasetFileConfigDto.ConfigId = fileConfig.ConfigId;
                        _configService.UpdateDatasetFileConfig(dto.DatasetFileConfigDto);

                        PrepDataFlowDto(dto, fileConfig.ParentDataset, updatedSchemaDto);
                        DataFlowDto updatedDataFlowDto = await _dataFlowService.UpdateDataFlowAsync(dto.DataFlowDto, dataFlow);

                        await _datasetContext.SaveChangesAsync();

                        //if create current view changed, call schema service GenerateConsumptionLayerEvents after changes are saved
                        if (currentViewChanged)
                        {
                            JObject changedProperty = new JObject { { "createcurrentview", schema.CreateCurrentView } };
                            _schemaService.GenerateConsumptionLayerEvents(schema, changedProperty);
                        }

                        SchemaResultDto resultDto = CreateSchemaResultDto(updatedSchemaDto, dto.DatasetFileConfigDto, updatedDataFlowDto);
                        return resultDto;

                    }
                    catch (Exception)
                    {
                        _datasetContext.Clear();
                        throw;
                    }
                }
                else
                {
                    throw new ResourceForbiddenException(user.AssociateId, nameof(security.CanManageSchema), "UpdateSchema", dto.SchemaDto.SchemaId);
                }
            }
            else
            {
                LogResourceNotFound(schema, fileConfig, dataFlow, dto.SchemaDto.SchemaId);
                throw new ResourceNotFoundException("UpdateSchema", dto.SchemaDto.SchemaId);
            }
        }

        #region Private
        private SchemaResultDto CreateSchemaResultDto(FileSchemaDto fileSchemaDto, DatasetFileConfigDto fileConfigDto, DataFlowDto dataFlowDto)
        {
            return new SchemaResultDto
            {
                SchemaId = fileSchemaDto.SchemaId,
                SchemaDescription = fileSchemaDto.Description,
                Delimiter = fileSchemaDto.Delimiter,
                HasHeader = fileSchemaDto.HasHeader,
                ScopeTypeCode = fileConfigDto.DatasetScopeTypeName,
                FileTypeCode = fileSchemaDto.FileExtensionName,
                SchemaRootPath = fileSchemaDto.SchemaRootPath,
                CreateCurrentView = fileSchemaDto.CreateCurrentView,
                IngestionType = (IngestionType)dataFlowDto.IngestionType,
                IsCompressed = dataFlowDto.IsCompressed,
                CompressionTypeCode = dataFlowDto.CompressionType.HasValue ? Enum.GetName(typeof(CompressionTypes), dataFlowDto.CompressionType) : null,
                IsPreprocessingRequired = dataFlowDto.IsPreProcessingRequired,
                PreprocessingTypeCode = dataFlowDto.PreProcessingOption.HasValue ? Enum.GetName(typeof(DataFlowPreProcessingTypes), dataFlowDto.PreProcessingOption) : null,
                DatasetId = dataFlowDto.DatasetId,
                SchemaName = fileSchemaDto.Name,
                SaidAssetCode = dataFlowDto.SaidKeyCode,
                NamedEnvironment = dataFlowDto.NamedEnvironment,
                NamedEnvironmentType = dataFlowDto.NamedEnvironmentType,
                KafkaTopicName = dataFlowDto.TopicName,
                PrimaryContactId = dataFlowDto.PrimaryContactId,
                StorageCode = dataFlowDto.FlowStorageCode,
                DropLocation = GetDropLocation((IngestionType)dataFlowDto.IngestionType, dataFlowDto.Id),
                ControlMTriggerName = fileSchemaDto.ControlMTriggerName,
                ObjectStatus = fileSchemaDto.ObjectStatus,
                CreateDateTime = fileSchemaDto.CreateDateTime,
                UpdateDateTime = dataFlowDto.CreateDTM > fileSchemaDto.UpdateDateTime ? dataFlowDto.CreateDTM : fileSchemaDto.UpdateDateTime
            };
        }

        private string GetDropLocation(IngestionType ingestionType, int dataFlowId)
        {
            if (ingestionType == IngestionType.DFS_Drop)
            {
                RetrieverJob job = _datasetContext.RetrieverJob.FirstOrDefault(x => x.ObjectStatus == ObjectStatusEnum.Active && x.DataFlow.Id == dataFlowId);
                if (job != null)
                {
                    Uri uri = job.DataSource.CalcRelativeUri(job);
                    return uri.ToString();
                }
            }
            else
            {
                DataFlowStep s3DropStep = _datasetContext.DataFlowStep.FirstOrDefault(x => x.DataFlow.Id == dataFlowId && x.DataAction_Type_Id == DataActionType.ProducerS3Drop);
                if (s3DropStep != null)
                {
                    return s3DropStep.TriggerBucket + "/" + s3DropStep.TriggerKey;
                }
            }

            return null;
        }

        private void PrepDataFlowDto(SchemaFlowDto dto, Dataset dataset, FileSchemaDto addedSchemaDto)
        {
            dto.DataFlowDto.Name = $"{dataset.ShortName}_{addedSchemaDto.Name.Replace(" ", "")}";
            dto.DataFlowDto.IsSecured = true;
            dto.DataFlowDto.SchemaMap = new List<SchemaMapDto>
            {
                new SchemaMapDto { DatasetId = dto.DataFlowDto.DatasetId, SchemaId = addedSchemaDto.SchemaId }
            };
        }

        private void LogResourceNotFound(FileSchema schema, DatasetFileConfig config, DataFlow dataFlow, int schemaId)
        {
            if (schema == null)
            {
                Logger.Warn($"No active Schema with Id {schemaId} to update");
            }
            else
            {
                if (config == null)
                {
                    Logger.Warn($"No active DatasetFileConfig exists for SchemaId {schemaId}");
                }

                if (dataFlow == null)
                {
                    Logger.Warn($"No active DataFlow exists for SchemaId {schemaId}");
                }
            }
        }
        #endregion
    }
}
