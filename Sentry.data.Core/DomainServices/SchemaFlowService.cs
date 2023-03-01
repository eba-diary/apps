using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.IO;
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

        public async Task<SchemaResultDto> AddSchemaAsync(AddSchemaDto dto)
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
                    dto.DataFlowDto.Name = $"{dataset.ShortName}_{addedSchemaDto.Name.Replace(" ", "")}";
                    dto.DataFlowDto.IsSecured = true;
                    dto.DataFlowDto.SchemaMap = new List<SchemaMapDto>
                    {
                        new SchemaMapDto { DatasetId = dto.DataFlowDto.DatasetId, SchemaId = addedSchemaDto.SchemaId }
                    };
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
                throw new ResourceForbiddenException();
            }
        }

        public async Task<SchemaResultDto> UpdateSchemaAsync(UpdateSchemaDto dto)
        {
            throw new NotImplementedException();
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
                    return Path.Combine(s3DropStep.TriggerBucket, s3DropStep.TriggerKey);
                }
            }

            return null;
        }
        #endregion
    }
}
