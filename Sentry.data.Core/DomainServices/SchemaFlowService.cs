using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Sentry.data.Core.DependencyInjection;
using Sentry.data.Core.DomainServices;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class SchemaFlowService : BaseDomainService<SchemaFlowService>, ISchemaFlowService
    {
        private readonly IConfigService _configService;
        private readonly ISchemaService _schemaService;
        private readonly IDataFlowService _dataFlowService;
        private readonly IDatasetContext _datasetContext;
        private readonly IUserService _userService;
        private readonly ISecurityService _securityService;
        private readonly IGlobalDatasetProvider _globalDatasetProvider;

        public SchemaFlowService(IConfigService configService,
            ISchemaService schemaService,
            IDataFlowService dataFlowService,
            IDatasetContext datasetContext,
            IUserService userService,
            ISecurityService securityService,
            IGlobalDatasetProvider globalDatasetProvider,
            DomainServiceCommonDependency<SchemaFlowService> commonDependency) : base(commonDependency)
        {
            _configService = configService;
            _schemaService = schemaService;
            _dataFlowService = dataFlowService;
            _datasetContext = datasetContext;
            _userService = userService;
            _securityService = securityService;
            _globalDatasetProvider = globalDatasetProvider;
        }

        public async Task<SchemaResultDto> AddSchemaAsync(SchemaFlowDto dto)
        {
            //get dataset
            Dataset dataset = _datasetContext.GetById(dto.SchemaDto.ParentDatasetId);

            //check permission to schema
            CheckPermission(dataset, "AddSchema");

            try
            {
                //create schema
                FileSchemaDto addedSchemaDto = await _schemaService.AddSchemaAsync(dto.SchemaDto);

                //create file config
                dto.DatasetFileConfigDto.SchemaId = addedSchemaDto.SchemaId;
                dto.DatasetFileConfigDto.FileExtensionId = addedSchemaDto.FileExtensionId;
                _configService.Create(dto.DatasetFileConfigDto);

                //create data flow
                dto.DataFlowDto.Name = $"{dataset.ShortName}_{addedSchemaDto.Name.Replace(" ", "")}";
                dto.DataFlowDto.IsSecured = true;
                dto.DataFlowDto.DatasetId = dataset.DatasetId;
                dto.DataFlowDto.SchemaMap = new List<SchemaMapDto>
                {
                    new SchemaMapDto { DatasetId = dataset.DatasetId, SchemaId = addedSchemaDto.SchemaId }
                };
                DataFlowDto addedDataFlowDto = await _dataFlowService.AddDataFlowAsync(dto.DataFlowDto);

                SchemaResultDto resultDto = CreateSchemaResultDto(addedSchemaDto, addedDataFlowDto, dto.DatasetFileConfigDto.DatasetScopeTypeName);
                await AddUpdateEnvironmentSchemaAsync(resultDto);

                return resultDto;
            }
            catch (Exception)
            {
                _datasetContext.Clear();
                throw;
            }
        }

        public async Task<SchemaResultDto> UpdateSchemaAsync(SchemaFlowDto dto)
        {
            //get schema
            FileSchema schema = _datasetContext.FileSchema.FirstOrDefault(x => x.SchemaId == dto.SchemaDto.SchemaId && x.ObjectStatus == ObjectStatusEnum.Active);
            DatasetFileConfig fileConfig = _datasetContext.DatasetFileConfigs.FirstOrDefault(x => x.Schema.SchemaId == dto.SchemaDto.SchemaId && !x.DeleteInd);
            DataFlow dataFlow = _datasetContext.DataFlow.FirstOrDefault(x => x.SchemaId == schema.SchemaId && x.ObjectStatus == ObjectStatusEnum.Active);

            if (schema == null || fileConfig == null || dataFlow == null)
            {
                LogResourceNotFound(schema, fileConfig, dataFlow, dto.SchemaDto.SchemaId);
                throw new ResourceNotFoundException("UpdateSchema", dto.SchemaDto.SchemaId);
            }

            //check permission to schema
            CheckPermission(fileConfig.ParentDataset, "UpdateSchema", dto.SchemaDto.SchemaId);

            try
            {
                //keep values that are not available to edit from API
                PrepSchemaDto(dto, schema, fileConfig);

                //need to trigger consumption layer even if create current view changes after successfully updating
                bool currentViewChanged = dto.SchemaDto.CreateCurrentView != schema.CreateCurrentView;

                //if schema root path went from null to value or value to null need to update dataflow to catch data flow step update
                dto.DataFlowDto.DataFlowStepUpdateRequired = RequireDataFlowUpdate(dto, schema);

                //update schema
                FileSchemaDto updatedSchemaDto = await _schemaService.UpdateSchemaAsync(dto.SchemaDto, schema);

                //update file config
                dto.DatasetFileConfigDto.FileExtensionId = updatedSchemaDto.FileExtensionId;
                _configService.UpdateDatasetFileConfig(dto.DatasetFileConfigDto, fileConfig);

                //update data flow
                DataFlowDto updatedDataFlowDto = await _dataFlowService.UpdateDataFlowAsync(dto.DataFlowDto, dataFlow);

                //save changes also gets called in UpdateDataFlowAsync, but only when their is a data flow change
                await _datasetContext.SaveChangesAsync();

                //if create current view changed, call schema service GenerateConsumptionLayerEvents after changes are saved
                RaiseConsumptionLayerEvent(currentViewChanged, schema);

                SchemaResultDto resultDto = CreateSchemaResultDto(updatedSchemaDto, updatedDataFlowDto, GetScopeType(dto, fileConfig));
                await AddUpdateEnvironmentSchemaAsync(resultDto);

                return resultDto;

            }
            catch (Exception)
            {
                _datasetContext.Clear();
                throw;
            }
        }

        #region Private
        private void CheckPermission(Dataset dataset, string action, int schemaId = 0)
        {
            IApplicationUser user = _userService.GetCurrentUser();
            UserSecurity security = _securityService.GetUserSecurity(dataset, user);

            if (!security.CanManageSchema)
            {
                if (schemaId > 0)
                {
                    throw new ResourceForbiddenException(user.AssociateId, nameof(UserSecurity.CanManageSchema), action, schemaId);
                }
                else
                {
                    throw new ResourceForbiddenException(user.AssociateId, nameof(UserSecurity.CanManageSchema), action);
                }
            }
        }

        private void PrepSchemaDto(SchemaFlowDto dto, FileSchema schema, DatasetFileConfig fileConfig)
        {
            //keep values that are not available to edit from API
            dto.SchemaDto.ParentDatasetId = fileConfig.ParentDataset.DatasetId;
            dto.SchemaDto.ParquetStorageBucket = schema.ParquetStorageBucket;
            dto.SchemaDto.ParquetStoragePrefix = schema.ParquetStoragePrefix;
            if (dto.DataFlowDto.IngestionType == 0)
            {
                //no ingestion type was specified which means it will not be changed so keep the existing KafkaFlag
                dto.SchemaDto.CLA1286_KafkaFlag = schema.CLA1286_KafkaFlag;
            }
        }

        private bool RequireDataFlowUpdate(SchemaFlowDto dto, FileSchema schema)
        {
            return (string.IsNullOrWhiteSpace(dto.SchemaDto.SchemaRootPath) && !string.IsNullOrWhiteSpace(schema.SchemaRootPath)) ||
                (!string.IsNullOrWhiteSpace(dto.SchemaDto.SchemaRootPath) && string.IsNullOrWhiteSpace(schema.SchemaRootPath));
        }

        private void RaiseConsumptionLayerEvent(bool currentViewChanged, FileSchema schema)
        {
            if (currentViewChanged)
            {
                JObject changedProperty = new JObject { { "createcurrentview", schema.CreateCurrentView } };
                if (_dataFeatures.CLA5211_SendNewSnowflakeEvents.GetValue())
                {
                    _schemaService.TryGenerateSnowflakeConsumptionCreateEvent(schema, changedProperty, false);
                }
                else
                {
                    _schemaService.GenerateConsumptionLayerEvents(schema, changedProperty);
                }
            }
        }

        private string GetScopeType(SchemaFlowDto dto, DatasetFileConfig fileConfig)
        {
            return string.IsNullOrWhiteSpace(dto.DatasetFileConfigDto.DatasetScopeTypeName) ? fileConfig.DatasetScopeType.Name : dto.DatasetFileConfigDto.DatasetScopeTypeName;
        }

        private SchemaResultDto CreateSchemaResultDto(FileSchemaDto fileSchemaDto, DataFlowDto dataFlowDto, string scopeTypeCode)
        {
            return new SchemaResultDto
            {
                SchemaId = fileSchemaDto.SchemaId,
                SchemaDescription = fileSchemaDto.Description,
                Delimiter = fileSchemaDto.Delimiter,
                HasHeader = fileSchemaDto.HasHeader,
                ScopeTypeCode = scopeTypeCode,
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

        private async Task AddUpdateEnvironmentSchemaAsync(SchemaResultDto resultDto)
        {
            if (_dataFeatures.CLA4789_ImprovedSearchCapability.GetValue())
            {
                EnvironmentSchema environmentSchema = resultDto.ToEnvironmentSchema();
                await _globalDatasetProvider.AddUpdateEnvironmentSchemaAsync(resultDto.DatasetId, environmentSchema);
            }
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

        private void LogResourceNotFound(FileSchema schema, DatasetFileConfig config, DataFlow dataFlow, int schemaId)
        {
            if (schema == null)
            {
                _logger.LogWarning($"No active {nameof(FileSchema)} exists for {nameof(FileSchema.SchemaId)} {schemaId}");
            }
            else
            {
                if (config == null)
                {
                    _logger.LogWarning($"No active {nameof(DatasetFileConfig)} exists for {nameof(FileSchema.SchemaId)} {schemaId}");
                }

                if (dataFlow == null)
                {
                    _logger.LogWarning($"No active {nameof(DataFlow)} exists for {nameof(FileSchema.SchemaId)} {schemaId}");
                }
            }
        }
        #endregion
    }
}
