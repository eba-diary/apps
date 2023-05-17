using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Configuration;
using Sentry.Core;
using Sentry.data.Core.DependencyInjection;
using Sentry.data.Core.DomainServices;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Entities.Schema.Elastic;
using Sentry.data.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public class SchemaService : BaseDomainService<SchemaService>, ISchemaService
    {
        public readonly IDataFlowService _dataFlowService;
        public readonly IJobService _jobService;
        private readonly IDatasetContext _datasetContext;
        private readonly IUserService _userService;
        private readonly ISecurityService _securityService;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ISnowProvider _snowProvider;
        private readonly IEventService _eventService;
        private readonly IElasticDocumentClient _elasticDocumentClient;
        private readonly IDscEventTopicHelper _dscEventTopicHelper;
        private readonly IGlobalDatasetProvider _globalDatasetProvider;

        private string _bucket;
        private readonly IList<string> _eventGeneratingUpdateFields = new List<string>() { "createcurrentview", "parquetstoragebucket", "parquetstorageprefix" };

        public SchemaService(IDatasetContext dsContext, IUserService userService,
            IDataFlowService dataFlowService, IJobService jobService, ISecurityService securityService,
            IMessagePublisher messagePublisher, ISnowProvider snowProvider, 
            IEventService eventService, IElasticDocumentClient elasticDocumentClient, 
            IDscEventTopicHelper dscEventTopicHelper, IGlobalDatasetProvider globalDatasetProvider,
            DomainServiceCommonDependency<SchemaService> commonDependency) : base(commonDependency)
        {
            _datasetContext = dsContext;
            _userService = userService;
            _dataFlowService = dataFlowService;
            _jobService = jobService;
            _securityService = securityService;
            _messagePublisher = messagePublisher;
            _snowProvider = snowProvider;
            _eventService = eventService;
            _elasticDocumentClient = elasticDocumentClient;
            _dscEventTopicHelper = dscEventTopicHelper;
            _globalDatasetProvider = globalDatasetProvider;
        }

        private string RootBucket
        {
            get
            {
                if (_bucket == null)
                {
                    _bucket = Config.GetHostSetting("AWS2_0RootBucket");
                }
                return _bucket;
            }
        }

        public Task<FileSchemaDto> AddSchemaAsync(FileSchemaDto dto)
        {
            FileSchema schema = MapToFileSchema(dto);
            FileSchemaDto resultDto = MapToDto(schema);
            return Task.FromResult(resultDto);
        }

        public int Create(FileSchemaDto dto)
        {
            FileSchema newSchema = MapToFileSchema(dto);
            return newSchema.SchemaId;
        }

        public int CreateAndSaveSchema(FileSchemaDto schemaDto)
        {
            FileSchema newSchema;
            try
            {
                newSchema = MapToFileSchema(schemaDto);

                _datasetContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "schemaservice-createandsaveschema");
                throw;
            }

            return newSchema.SchemaId;
        }

        public async Task CreateExternalDependenciesAsync(int schemaId)
        {
            FileSchemaDto schemaDto = GetFileSchemaDto(schemaId);
            schemaDto.ParentDatasetId = _datasetContext.DatasetFileConfigs.Where(x => x.Schema.SchemaId == schemaId && x.ObjectStatus == GlobalEnums.ObjectStatusEnum.Active).Select(x => x.ParentDataset.DatasetId).FirstOrDefault();
            await CreateExternalDependenciesAsync(schemaDto).ConfigureAwait(false);
        }

        public async Task CreateExternalDependenciesAsync(FileSchemaDto schemaDto)
        {
            if (_dataFeatures.CLA4789_ImprovedSearchCapability.GetValue())
            {
                EnvironmentSchema environmentSchema = schemaDto.ToEnvironmentSchema();
                await _globalDatasetProvider.AddUpdateEnvironmentSchemaAsync(schemaDto.ParentDatasetId, environmentSchema).ConfigureAwait(false);
            }
        }

        //SINCE SCHEMA SAVED FROM MULTIPLE PLACES, HAVE ONE METHOD TO TAKE CARE OF PUBLISHING SAVE
        public void PublishSchemaEvent(int datasetId, int schemaId)
        {
            _eventService.PublishSuccessEventBySchemaId(GlobalConstants.EventType.CREATE_DATASET_SCHEMA, GlobalConstants.EventType.CREATE_DATASET_SCHEMA, datasetId, schemaId);
        }
                
        [Obsolete("Use " + nameof(CreateSchemaRevision) + "method excepting " + nameof(SchemaRevisionFieldStructureDto),false)]
        public int CreateAndSaveSchemaRevision(int schemaId, List<BaseFieldDto> schemaRows, string revisionname, string jsonSchema = null)
        {
            Dataset ds = _datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == schemaId).Select(s => s.ParentDataset).FirstOrDefault();

            if (ds == null)
            {
                throw new DatasetNotFoundException();
            }

            try
            {
                UserSecurity us = _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());
                if (!us.CanManageSchema)
                {
                    try
                    {
                        IApplicationUser user = _userService.GetCurrentUser();
                        _logger.LogInformation($"{nameof(SchemaService).ToLower()}-{nameof(CreateAndSaveSchemaRevision).ToLower()} unauthorized_access: Id:{user.AssociateId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"{nameof(SchemaService).ToLower()}-{nameof(CreateAndSaveSchemaRevision).ToLower()} unauthorized_access");
                    }
                    throw new SchemaUnauthorizedAccessException();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(SchemaService).ToLower()}-{nameof(CreateAndSaveSchemaRevision).ToLower()} failed to retrieve UserSecurity object");
                throw new SchemaUnauthorizedAccessException();
            }

            FileSchema schema = _datasetContext.GetById<FileSchema>(schemaId);
            SchemaRevision revision;
            SchemaRevision latestRevision = null;

            try
            {
                if (schema != null)
                {
                    latestRevision = _datasetContext.SchemaRevision.Where(w => w.ParentSchema.SchemaId == schema.SchemaId).OrderByDescending(o => o.Revision_NBR).Take(1).FirstOrDefault();
                    
                    revision = new SchemaRevision()
                    {
                        SchemaRevision_Name = revisionname,
                        CreatedBy = _userService.GetCurrentUser().AssociateId,
                        JsonSchemaObject = jsonSchema
                    };

                    schema.AddRevision(revision);

                    _datasetContext.Add(revision);

                    //filter out fields marked for deletion
                    foreach (var row in schemaRows.Where(w => !w.DeleteInd))
                    {
                        revision.Fields.Add(AddRevisionField(row, revision, previousRevision: latestRevision));
                    }

                    //Add posible checksum validation here

                    SetHierarchyProperties(revision.Fields);

                    _datasetContext.SaveChanges();

                    DeleteElasticIndexForSchema(schemaId);
                    IndexElasticFieldsForSchema(schemaId, ds.DatasetId, revision.Fields);
                    GenerateConsumptionLayerCreateEvent(revision, JObject.Parse("{\"revision\":\"added\"}"));
                                        
                    return revision.SchemaRevision_Id;
                }
            }
            catch (AggregateException agEx)
            {
                var flatArgExs = agEx.Flatten().InnerExceptions;
                foreach(var ex in flatArgExs)
                {
                    _logger.LogError(ex, "Failed generating consumption layer event");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add revision");
            }

            return 0;
        }

        public int CreateSchemaRevision(SchemaRevisionFieldStructureDto schemaRevisionFieldsStructureDto)
        {
            Dataset ds = _datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == schemaRevisionFieldsStructureDto.Revision.SchemaId).Select(s => s.ParentDataset).FirstOrDefault();

            if (ds == null)
            {
                throw new DatasetNotFoundException();
            }

            try
            {
                UserSecurity us = _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());
                if (!us.CanManageSchema)
                {
                    try
                    {
                        IApplicationUser user = _userService.GetCurrentUser();
                        _logger.LogInformation($"{nameof(SchemaService).ToLower()}-{nameof(CreateAndSaveSchemaRevision).ToLower()} unauthorized_access: Id:{user.AssociateId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"{nameof(SchemaService).ToLower()}-{nameof(CreateAndSaveSchemaRevision).ToLower()} unauthorized_access");
                    }
                    throw new SchemaUnauthorizedAccessException();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(SchemaService).ToLower()}-{nameof(CreateAndSaveSchemaRevision).ToLower()} failed to retrieve UserSecurity object");
                throw new SchemaUnauthorizedAccessException();
            }

            FileSchema schema = _datasetContext.GetById<FileSchema>(schemaRevisionFieldsStructureDto.Revision.SchemaId);
            SchemaRevision revision;
            SchemaRevision latestRevision = null;

            try
            {
                if (schema != null)
                {
                    latestRevision = _datasetContext.SchemaRevision.Where(w => w.ParentSchema.SchemaId == schema.SchemaId).OrderByDescending(o => o.Revision_NBR).Take(1).FirstOrDefault();

                    revision = new SchemaRevision()
                    {
                        SchemaRevision_Name = schemaRevisionFieldsStructureDto.Revision.SchemaRevisionName,
                        CreatedBy = _userService.GetCurrentUser().AssociateId,
                        JsonSchemaObject = null
                    };

                    schema.AddRevision(revision);

                    _datasetContext.Add(revision);

                    //filter out fields marked for deletion
                    foreach (var row in schemaRevisionFieldsStructureDto.FieldStructure.Where(w => !w.DeleteInd))
                    {
                        revision.Fields.Add(AddRevisionField(row, revision, previousRevision: latestRevision));
                    }

                    //Add posible checksum validation here

                    SetHierarchyProperties(revision.Fields);

                    return revision.SchemaRevision_Id;
                }
            }
            catch (AggregateException agEx)
            {
                var flatArgExs = agEx.Flatten().InnerExceptions;
                foreach (var ex in flatArgExs)
                {
                    _logger.LogError(ex, "Failed generating consumption layer event");
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add revision");
                throw;
            }

            return 0;
        }

        public void CreateSchemaRevisionExternalDependencies(int schemaId, int schemaRevisionId)
        {
            FileSchema fileSchema = _datasetContext.GetById<FileSchema>(schemaId);
            SchemaRevision schemaRevision = fileSchema.Revisions.FirstOrDefault(w => w.SchemaRevision_Id == schemaRevisionId);
            int parentDatasetId = _datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == schemaId).Select(s => s.ParentDataset.DatasetId).FirstOrDefault();

            CreateSchemaRevisionExternalDependencies_Internal(schemaRevision, parentDatasetId);
        }

        private void CreateSchemaRevisionExternalDependencies_Internal(SchemaRevision revision, int datasetId)
        {
            DeleteElasticIndexForSchema(revision.ParentSchema.SchemaId);
            IndexElasticFieldsForSchema(revision.ParentSchema.SchemaId, datasetId, revision.Fields);
            GenerateConsumptionLayerCreateEvent(revision, JObject.Parse("{\"revision\":\"added\"}"));
        }

        public bool UpdateAndSaveSchema(FileSchemaDto schemaDto)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            _logger.LogInformation($"startmethod <{m.ReflectedType.Name}>");

            DatasetFileConfig fileConfig = GetDatasetFileConfig(schemaDto.ParentDatasetId, schemaDto.SchemaId, x => x.CanManageSchema);

            JObject whatPropertiesChanged;
            /* Any exceptions saving schema changes, do not execute remaining line of code */
            try
            {
                whatPropertiesChanged = UpdateSchema(schemaDto, fileConfig.Schema);
                _logger.LogInformation($"<{m.ReflectedType.Name.ToLower()}> Changes detected for {fileConfig.ParentDataset.DatasetName}\\{fileConfig.Schema.Name} | {whatPropertiesChanged}");
                _datasetContext.SaveChanges();

                if (_dataFeatures.CLA4789_ImprovedSearchCapability.GetValue())
                {
                    EnvironmentSchema environmentSchema = schemaDto.ToEnvironmentSchema();

                    DataFlow dataFlow = _datasetContext.DataFlow.FirstOrDefault(x => x.SchemaId == schemaDto.SchemaId && x.ObjectStatus == GlobalEnums.ObjectStatusEnum.Active);
                    if (dataFlow != null)
                    {
                        environmentSchema.SchemaSaidAssetCode = dataFlow.SaidKeyCode;
                    }

                    _globalDatasetProvider.AddUpdateEnvironmentSchemaAsync(fileConfig.ParentDataset.DatasetId, environmentSchema).Wait();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"<{m.ReflectedType.Name.ToLower()}> Failed schema save");
                return false;
            }

            /* The remaining actions should all be executed even if one fails
                * If there are any exceptions, log the exceptions and continue on */
            var exceptions = new List<Exception>();

            /*
            * Generate consumption layer events to dsc event topic
            *  This ensures schema is updated appropriately with
            *  adjustements made within 
            */
            try
            {
                GenerateConsumptionLayerEvents(fileConfig.Schema, whatPropertiesChanged);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            /* Allow changes to be saved if notification or events are not generated, therefore,
            *   log the error and return true 
            */
            if (exceptions.Count > 0)
            {
                _logger.LogError(new AggregateException(exceptions), $"<{m.ReflectedType.Name.ToLower()}> Failed sending downstream notifications or events");
            }

            _logger.LogInformation($"endmethod <{m.ReflectedType.Name}>");
            return true;
        }

        public Task<FileSchemaDto> UpdateSchemaAsync(FileSchemaDto dto, FileSchema schema)
        {
            //Uses name in ControlMTrigger comparison, name is immutable
            dto.Name = schema.Name;

            //Only change delimiter when file type is changing
            if (string.IsNullOrWhiteSpace(dto.FileExtensionName) && string.IsNullOrEmpty(dto.Delimiter))
            {
                dto.Delimiter = schema.Delimiter;
            }

            UpdateSchema(dto, schema);
            FileSchemaDto resultDto = MapToDto(schema);
            return Task.FromResult(resultDto);
        }

        private string GetDSCEventTopic(int datasetId)
        {
            string topicName;
            Dataset ds = _datasetContext.GetById<Dataset>(datasetId);
            topicName = _dscEventTopicHelper.GetDSCTopic(ds);
            if (string.IsNullOrEmpty(topicName))
            {
                throw new ArgumentException("Topic Name is null");
            }
            return topicName;
        }
        public int GetFileExtensionIdByName(string extensionName)
        {
            FileExtension extension = _datasetContext.FileExtensions.FirstOrDefault(x => x.Name == extensionName);
            if (extension != null)
            {
                return extension.Id;
            }

            return 0;
        }

        public void CreateOrUpdateConsumptionLayersForSchema(int[] schemaIdList)
        {
            foreach(int schemaId in schemaIdList)
            {
                FileSchema schema = _datasetContext.GetById<FileSchema>(schemaId);
                Dataset ds = _datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == schema.SchemaId).Select(s => s.ParentDataset).FirstOrDefault();
                FileSchemaDto dto = MapToDto(schema);

                CreateOrUpdateConsumptionLayersForSchema(schema, dto, ds);
            }
        }

        public void CreateOrUpdateConsumptionLayersForSchema(FileSchema schema, FileSchemaDto dto, Dataset ds)
        {

            foreach (SchemaConsumptionSnowflake snowflakeConsumption in GenerateConsumptionLayers(dto, schema, ds).Cast<SchemaConsumptionSnowflake>().ToList())
            {
                schema.AddOrUpdateSnowflakeConsumptionLayer(snowflakeConsumption);
            }

            _datasetContext.SaveChanges();
        }

        public (int schemaId, bool schemaExistsInTargetDataset) SchemaExistsInTargetDataset(int targetDatasetId, string schemaName)
        {
            if (string.IsNullOrEmpty(schemaName))
            {
                throw new ArgumentNullException(nameof(schemaName));
            }
            if (targetDatasetId == 0)
            {
                throw new ArgumentNullException(nameof(targetDatasetId));
            }

            int schemaId = _datasetContext.DatasetFileConfigs.Where(w => w.ParentDataset.DatasetId == targetDatasetId && w.Schema.Name == schemaName && w.Schema.ObjectStatus == GlobalEnums.ObjectStatusEnum.Active).Select(s => s.Schema.SchemaId).FirstOrDefault();

            return (schemaId, (schemaId != 0));
        }

        public void GenerateConsumptionLayerEvents(FileSchema schema, JObject propertyDeltaList)
        {
            /*Generate *-CREATE-TABLE-REQUESTED event when:
            *  - CreateCurrentView changes
            */
            if (_eventGeneratingUpdateFields.Any(x => propertyDeltaList.ContainsKey(x)))
            {
                SchemaRevision latestRevision = schema.Revisions.OrderByDescending(o => o.SchemaRevision_Id).FirstOrDefault();
                GenerateConsumptionLayerCreateEvent(latestRevision, propertyDeltaList);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaRevision"></param>
        /// <param name="propertyDeltaList"></param>
        /// <exception cref="AggregateException">Thows exception when event could not be published</exception>
        private void GenerateConsumptionLayerCreateEvent(SchemaRevision schemaRevision, JObject propertyDeltaList)
        {
            string methodName = $"{nameof(SchemaService).ToLower()}_{nameof(GenerateConsumptionLayerCreateEvent).ToLower()}";

            //Do nothing if there is no revision associated with schema
            if (schemaRevision == null)
            {
               _logger.LogDebug($"<{methodName}> - consumption layer event not generated - no schema revision");
                return;
            }
                        
            int dsId = _datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == schemaRevision.ParentSchema.SchemaId).Select(s => s.ParentDataset.DatasetId).FirstOrDefault();

            bool generateEvent = false;
            /* schema column updates trigger, this will trigger for initial schema column add along with any updates there after */
            if (propertyDeltaList.ContainsKey("revision") && propertyDeltaList.GetValue("revision").ToString().ToLower() == "added")
            {
                generateEvent = true;
            }
            /* schema configuration trigger for createCurrentView regardless true\false */
            else if (_eventGeneratingUpdateFields.Any(x => propertyDeltaList.ContainsKey(x)))
            {
                generateEvent = true;
            }

            //We want to attempt to send both events even if first fails, then report back any failures.
            if (generateEvent)
            {
                var exceptionList = new List<Exception>();
                HiveTableCreateModel hiveCreate = new HiveTableCreateModel()
                {
                    SchemaID = schemaRevision.ParentSchema.SchemaId,
                    RevisionID = schemaRevision.SchemaRevision_Id,
                    DatasetID = dsId,
                    HiveStatus = null,
                    InitiatorID = _userService.GetCurrentUser().AssociateId,
                    ChangeIND = propertyDeltaList.ToString(Formatting.None)
                };

                try
                {
                   _logger.LogDebug($"<{methodName}> sending {hiveCreate.EventType.ToLower()} event...");
                    string topicName = null;
                    if (string.IsNullOrWhiteSpace(_dataFeatures.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()))
                    {
                        topicName = GetDSCEventTopic(dsId);
                        _messagePublisher.Publish(topicName, schemaRevision.ParentSchema.SchemaId.ToString(), JsonConvert.SerializeObject(hiveCreate));
                    }
                    else
                    {
                        _messagePublisher.PublishDSCEvent(schemaRevision.ParentSchema.SchemaId.ToString(), JsonConvert.SerializeObject(hiveCreate), topicName);
                    }

                   _logger.LogDebug($"<{methodName}> sent {hiveCreate.EventType.ToLower()} event");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"<{methodName}> failed sending event: {JsonConvert.SerializeObject(hiveCreate)}");
                    exceptionList.Add(ex);
                }


                SnowTableCreateModel snowModel = new SnowTableCreateModel()
                {
                    DatasetID = dsId,
                    SchemaID = schemaRevision.ParentSchema.SchemaId,
                    RevisionID = schemaRevision.SchemaRevision_Id,
                    InitiatorID = _userService.GetCurrentUser().AssociateId,
                    ChangeIND = propertyDeltaList.ToString(Formatting.None)
                };

                try
                {
                    _logger.LogInformation($"<{methodName}> sending {snowModel.EventType.ToLower()} event...");
                    string topicName = null;
                    if (string.IsNullOrWhiteSpace(_dataFeatures.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()))
                    {
                        topicName = GetDSCEventTopic(dsId);
                        _messagePublisher.Publish(topicName, snowModel.SchemaID.ToString(), JsonConvert.SerializeObject(snowModel));
                    }
                    else
                    {
                        _messagePublisher.PublishDSCEvent(snowModel.SchemaID.ToString(), JsonConvert.SerializeObject(snowModel), topicName);
                    }
                    _logger.LogInformation($"<{methodName}> sent {snowModel.EventType.ToLower()} event");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"<{methodName}> failed sending event: {snowModel}");
                    exceptionList.Add(ex);
                }

                if (exceptionList.Any())
                {
                    throw new AggregateException("Failed sending consumption layer event", exceptionList);
                }
            }     
        }

        /// <summary>
        /// Updates existing schema object from DTO.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="schema"></param>
        /// <returns> Returns list of properties that have changed.</returns>
        internal JObject UpdateSchema(FileSchemaDto dto, FileSchema schema)
        {
            List<bool> changes = new List<bool>();
            JObject whatPropertiesChanged = new JObject();

            changes.Add(TryUpdate(() => schema.Delimiter, () => dto.Delimiter, (x) => schema.Delimiter = x));

            changes.Add(TryUpdate(() => schema.CreateCurrentView, () => dto.CreateCurrentView, 
                (x) => {
                            schema.CreateCurrentView = x;
                            whatPropertiesChanged.Add("createcurrentview", x.ToString().ToLower());
                       }));

            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                changes.Add(TryUpdate(() => schema.Description, () => dto.Description, (x) => schema.Description = x));
            }

            if (dto.FileExtensionId > 0)
            {
                changes.Add(TryUpdate(() => schema.Extension.Id, () => dto.FileExtensionId, (x) => schema.Extension = _datasetContext.GetById<FileExtension>(dto.FileExtensionId)));
            }
            else if (!string.IsNullOrWhiteSpace(dto.FileExtensionName))
            {
                changes.Add(TryUpdate(() => schema.Extension.Name, () => dto.FileExtensionName, (x) => schema.Extension = _datasetContext.FileExtensions.First(f => f.Name.ToLower() == dto.FileExtensionName.ToLower())));
            }

            changes.Add(TryUpdate(() => schema.HasHeader, () => dto.HasHeader, (x) => schema.HasHeader = x));

            changes.Add(TryUpdate(() => schema.CLA1396_NewEtlColumns, () => dto.CLA1396_NewEtlColumns, (x) => schema.CLA1396_NewEtlColumns = x));

            changes.Add(TryUpdate(() => schema.CLA1580_StructureHive, () => dto.CLA1580_StructureHive, (x) => schema.CLA1580_StructureHive = x));

            changes.Add(TryUpdate(() => schema.CLA2472_EMRSend, () => dto.CLA2472_EMRSend, (x) => schema.CLA2472_EMRSend = x));

            changes.Add(TryUpdate(() => schema.CLA1286_KafkaFlag, () => dto.CLA1286_KafkaFlag, (x) => schema.CLA1286_KafkaFlag = x));

            changes.Add(TryUpdate(() => schema.CLA3014_LoadDataToSnowflake, () => dto.CLA3014_LoadDataToSnowflake, (x) => schema.CLA3014_LoadDataToSnowflake = x));

            changes.Add(TryUpdate(() => schema.SchemaRootPath, () => dto.SchemaRootPath, (x) => schema.SchemaRootPath = x));

            changes.Add(TryUpdate(() => schema.ControlMTriggerName, () => GetControlMTrigger(dto), (x) => schema.ControlMTriggerName = x));

            SchemaParquetUpdate(schema, dto, changes, whatPropertiesChanged);

            if (changes.Any(x => x))
            {
                schema.LastUpdatedDTM = DateTime.Now;
                schema.UpdatedBy = _userService.GetCurrentUser().AssociateId;
            }

            return whatPropertiesChanged;
        }

        private void SchemaParquetUpdate(FileSchema schema, FileSchemaDto dto, List<bool> changes, JObject whatPropertiesChanged)
        {
            changes.Add(TryUpdate(() => schema.ParquetStorageBucket, () => dto.ParquetStorageBucket,
                (x) =>
                {
                    schema.ParquetStorageBucket = x;
                    whatPropertiesChanged.Add("parquetstoragebucket", string.IsNullOrEmpty(x) ? null : x.ToLower());
                }));
            changes.Add(TryUpdate(() => schema.ParquetStoragePrefix, () => dto.ParquetStoragePrefix,
                (x) =>
                {
                    schema.ParquetStoragePrefix = x;
                    whatPropertiesChanged.Add("parquetstorageprefix", string.IsNullOrEmpty(x) ? null : x.ToLower());
                }));

            if (dto.ConsumptionDetails != null)
            {
                foreach (var consumptionDetailDto in dto.ConsumptionDetails.OfType<SchemaConsumptionSnowflakeDto>())
                {
                    SchemaConsumptionSnowflake consumptionDetail;
                    if (consumptionDetailDto.SchemaConsumptionId == 0)
                    {
                        //this is logic to account for the fact that we're still supporting the v2 API that doesn't provide a SchemaConsumptionId
                        consumptionDetail = schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().First(cd => cd.SnowflakeType == consumptionDetailDto.SnowflakeType);
                    }
                    else
                    {
                        //this is how the logic should work for the v20220609 API
                        consumptionDetail = schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().First(cd => cd.SchemaConsumptionId == consumptionDetailDto.SchemaConsumptionId);
                    }
                    changes.Add(TryUpdate(() => consumptionDetail.SnowflakeStage, () => consumptionDetailDto.SnowflakeStage, (x) => consumptionDetail.SnowflakeStage = x));
                }
            }
        }

        private string GetControlMTrigger(FileSchemaDto dto)
        {
            string controlMTriggerName = string.Empty;
            Dataset ds = _datasetContext.GetById<Dataset>(dto.ParentDatasetId);
            if(ds != null)
            {
                Regex reg = new Regex("[^a-zA-Z0-9]");
                string namedEnvironmentCleaned = reg.Replace((ds.NamedEnvironment != null)? ds.NamedEnvironment.ToUpper() : String.Empty,String.Empty);
                string shortNameCleaned = reg.Replace((ds.ShortName != null)? ds.ShortName.ToUpper() : String.Empty, String.Empty);
                string schemaNameCleaned = reg.Replace((dto.Name != null)? dto.Name.ToUpper() : String.Empty, String.Empty);

                controlMTriggerName = $"DATA_{namedEnvironmentCleaned}_{shortNameCleaned}_{schemaNameCleaned}_COMPLETED";
            }
            
            return controlMTriggerName;
        }

        private bool TryUpdate<T>(Func<T> existingValue, Func<T> updateValue, Action<T> setter)
        {
            T value = updateValue();
            if (!Equals(existingValue(), value))
            {
                setter(value);
                return true;
            }

            return false;
        }

        public UserSecurity GetUserSecurityForSchema(int schemaId)
        {
            Dataset ds = _datasetContext.DatasetFileConfigs.FirstOrDefault(w => w.Schema.SchemaId == schemaId).ParentDataset;
            IApplicationUser user = _userService.GetCurrentUser();
            UserSecurity us = _securityService.GetUserSecurity(ds, user);
            return us;
        }

        public FileSchemaDto GetFileSchemaDto(int id)
        {
            FileSchema scm = _datasetContext.FileSchema.Where(w => w.SchemaId == id).FirstOrDefault();
            return MapToDto(scm);
        }

        public SchemaRevisionDto GetSchemaRevisionDto(int id)
        {
            SchemaRevision revision = _datasetContext.GetById<SchemaRevision>(id);
            SchemaRevisionDto dto = revision.ToDto();
            return dto;
        }

        public List<SchemaRevisionDto> GetSchemaRevisionDtoBySchema(int id)
        {
            Dataset ds = _datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == id).Select(s => s.ParentDataset).FirstOrDefault();
            if (ds == null)
            {
                throw new SchemaNotFoundException();
            }

            try
            {
                UserSecurity us;
                us = _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());
                if (!(us.CanPreviewDataset || us.CanViewFullDataset || us.CanUploadToDataset || us.CanEditDataset || us.CanManageSchema))
                {
                    try
                    {
                        IApplicationUser user = _userService.GetCurrentUser();
                        _logger.LogInformation($"schemacontroller-fetSchemarevisiondtobyschema unauthorized_access: Id:{user.AssociateId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "schemacontroller-fetSchemarevisiondtobyschema unauthorized_access");
                    }
                    throw new SchemaUnauthorizedAccessException();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"schemacontroller-fetSchemarevisiondtobyschema failed to retrieve UserSecurity object");
                throw new SchemaUnauthorizedAccessException();
            }

            List<SchemaRevisionDto> dtoList = new List<SchemaRevisionDto>();
            foreach (SchemaRevision revision in _datasetContext.SchemaRevision.Where(w => w.ParentSchema.SchemaId == id).ToList())
            {
                dtoList.Add(revision.ToDto());
            }
            return dtoList;
        }

        public List<BaseFieldDto> GetBaseFieldDtoBySchemaRevision(int revisionId)
        {
            //Perform fetch of children is needed to prevent n+1 when sending to ToDto()
            List<BaseField> fieldList = _datasetContext.BaseFields.Where(w => w.ParentSchemaRevision.SchemaRevision_Id == revisionId).Fetch(f => f.ChildFields).OrderBy(o => o.OrdinalPosition).ToList();

            //ToDto() assumes only root level columns are in the initial list, therefore, we filter on where ParentField == null.
            // This does not produce any n+1 scenario since the child fields have been loaded into memory already, therefore, .net does not need to go back to database
            return fieldList.Where(w => w.ParentField == null).ToList().ToDto();
        }

        public SchemaRevisionDto GetLatestSchemaRevisionDtoBySchema(int schemaId)
        {
            Dataset ds = _datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == schemaId).Select(s => s.ParentDataset).FirstOrDefault();

            if (ds == null)
            {
                return null;
            }
            
            try
            {
                UserSecurity us = _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());
                if (!(us.CanPreviewDataset || us.CanViewFullDataset || us.CanUploadToDataset || us.CanEditDataset || us.CanManageSchema))
                {
                    try
                    {
                        IApplicationUser user = _userService.GetCurrentUser();
                        _logger.LogInformation($"schemacontroller-getlatestschemarevisiondtobyschema unauthorized_access: Id:{user.AssociateId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "schemacontroller-getlatestschemarevisiondtobyschema unauthorized_access");
                    }
                    throw new SchemaUnauthorizedAccessException();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"schemacontroller-getlatestschemarevisiondtobyschema failed to retrieve UserSecurity object");
                throw new SchemaUnauthorizedAccessException();
            }

            SchemaRevision revision = _datasetContext.SchemaRevision.Where(w => w.ParentSchema.SchemaId == schemaId).OrderByDescending(o => o.Revision_NBR).Take(1).FirstOrDefault();

            return revision?.ToDto();
        }

        public SchemaRevisionJsonStructureDto GetLatestSchemaRevisionJsonStructureBySchemaId(int datasetId, int schemaId)
        {
            //check schema exists
            DatasetFileConfig fileConfig = GetDatasetFileConfig(datasetId, schemaId, x => x.CanPreviewDataset || x.CanViewFullDataset || x.CanUploadToDataset || x.CanEditDataset || x.CanManageSchema);

            //get latest revision
            SchemaRevision revision = fileConfig.GetLatestSchemaRevision();

            //return result as dto
            return new SchemaRevisionJsonStructureDto()
            {
                Revision = revision?.ToDto(),
                JsonStructure = revision?.ToJsonStructure()
            };
        }

        public SchemaRevisionFieldStructureDto GetLatestSchemaRevisionFieldStructureBySchemaId(int datasetId, int schemaId)
        {
            //check schema exists
            DatasetFileConfig fileConfig = GetDatasetFileConfig(datasetId, schemaId, x => x.CanPreviewDataset || x.CanViewFullDataset || x.CanUploadToDataset || x.CanEditDataset || x.CanManageSchema);

            //get latest revision
            SchemaRevision revision = fileConfig.GetLatestSchemaRevision();

            return new SchemaRevisionFieldStructureDto()
            {
                Revision = revision?.ToDto(),
                FieldStructure = revision?.ToFieldStructure()
            };
        }

        public List<DatasetFile> GetDatasetFilesBySchema(int schemaId)
        {
            List<DatasetFile> fileList = _datasetContext.DatasetFileStatusActive.Where(w => w.Schema.SchemaId == schemaId).ToList();
            return fileList;
        }
        
        public IDictionary<int, string> GetSchemaList()
        {
            IDictionary<int, string> schemaList = _datasetContext.Schema
                .Where(w => w.ObjectStatus == GlobalEnums.ObjectStatusEnum.Active)
                .Select(s => new { s.SchemaId, s.Name })
                .ToDictionary(d => d.SchemaId, d => d.Name);

            return schemaList;
        }

        public DatasetFile GetLatestDatasetFileBySchema(int schemaId)
        {
            DatasetFile file = _datasetContext.DatasetFileStatusActive.OrderBy(x => x.CreatedDTM).FirstOrDefault(w => w.Schema.SchemaId == schemaId);
            return file;
        }

        public FileSchema GetFileSchemaByStorageCode(string storageCode)
        {
            FileSchema schema = _datasetContext.FileSchema.Where(w => w.StorageCode == storageCode).FirstOrDefault();
            return schema;
        }

        public List<Dictionary<string, object>> GetTopNRowsByConfig(int id, int rows)
        {
            DatasetFileConfig config = _datasetContext.DatasetFileConfigs.Where(w => w.ConfigId == id).FirstOrDefault();
            if (config == null)
            {
                throw new SchemaNotFoundException("Schema not found");
            }

            //if there are no revisions (schema metadata not supplied), do not run the query
            if (!config.Schema.Revisions.Any())
            {
                throw new SchemaNotFoundException("Column metadata not added");
            }
            
            return GetTopNRowsBySchema(config.Schema.SchemaId, rows);
        }
        public List<Dictionary<string, object>> GetTopNRowsBySchema(int id, int rows)
        {
            Dataset ds = _datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == id).Select(s => s.ParentDataset).FirstOrDefault();
            if (ds == null)
            {
                throw new SchemaNotFoundException();
            }

            try
            {
                UserSecurity us;
                us = _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());
                if (!(us.CanPreviewDataset))
                {
                    try
                    {
                        IApplicationUser user = _userService.GetCurrentUser();
                        _logger.LogInformation($"schemacontroller-{System.Reflection.MethodBase.GetCurrentMethod().Name.ToLower()} unauthorized_access: Id:{user.AssociateId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"schemacontroller-{System.Reflection.MethodBase.GetCurrentMethod().Name.ToLower()} unauthorized_access");
                    }
                    throw new SchemaUnauthorizedAccessException();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"schemacontroller-{System.Reflection.MethodBase.GetCurrentMethod().Name.ToLower()} failed to retrieve UserSecurity object");
                throw new SchemaUnauthorizedAccessException();
            }

            FileSchemaDto schemaDto = GetFileSchemaDto(id);

            List<Dictionary<string, object>> dicRows = new List<Dictionary<string, object>>();
            var snowConsumption = schemaDto.ConsumptionDetails.OfType<SchemaConsumptionSnowflakeDto>().FirstOrDefault(consumptionType => consumptionType.SnowflakeType == SnowflakeConsumptionType.DatasetSchemaParquet);
            if (snowConsumption != null)
            {

                //OVERRIDE Database in lower environments where snowflake data doesn't exist to always hit qual
                string snowDatabase = Config.GetHostSetting("SnowDatabaseOverride");
                if (snowDatabase != string.Empty)
                {
                    snowConsumption.SnowflakeDatabase = snowDatabase;
                }

                string vwVersion = "vw_" + snowConsumption.SnowflakeTable;
                bool tableExists = _snowProvider.CheckIfExists(snowConsumption.SnowflakeDatabase, snowConsumption.SnowflakeSchema, vwVersion);     //Does table exist
                                                                                                                                       //If table does not exist
                if (!tableExists)
                {
                    throw new HiveTableViewNotFoundException("Table or view not found");
                }

                //Query table for rows
                System.Data.DataTable result = _snowProvider.GetTopNRows(snowConsumption.SnowflakeDatabase, snowConsumption.SnowflakeSchema, vwVersion, rows);

                Dictionary<string, object> dicRow = null;
                foreach (System.Data.DataRow dr in result.Rows)
                {
                    dicRow = new Dictionary<string, object>();
                    foreach (System.Data.DataColumn col in result.Columns)
                    {
                        string colName = col.ColumnName;
                        dicRow.Add(colName, dr[col]);
                    }
                    dicRows.Add(dicRow);
                }
            }
            return dicRows;
        }

        public void RegisterRawFile(FileSchema schema, string objectKey, string versionId, DataFlowStepEvent stepEvent)
        {
            if (objectKey == null)
            {
               _logger.LogDebug($"schemaservice-registerrawfile no-objectkey-input");
                throw new ArgumentException("schemaservice-registerrawfile no-objectkey-input");
            }

            if (schema == null)
            {
               _logger.LogDebug($"schemaservice-registerrawfile no-schema-input");
                throw new ArgumentException("schemaservice-registerrawfile no-schema-input");
            }

            if (stepEvent == null)
            {
               _logger.LogDebug($"schemaservice-registerrawfile no-stepevent-input");
                throw new ArgumentException("schemaservice-registerrawfile no-stepevent-input");
            }

            try
            {
                DatasetFile file = new DatasetFile();

                MapToDatasetFile(stepEvent, objectKey, versionId, file);
                _datasetContext.Add(file);

                //if this is a reprocess scenario, set previous dataset files ParentDatasetFileID to this datasetfile
                //  this will ensure only the latest file version shows within UI
                if (stepEvent.RunInstanceGuid != null || stepEvent.RunInstanceGuid != string.Empty)
                {
                    List<DatasetFile> previousFileList = new List<DatasetFile>();
                    previousFileList = _datasetContext.DatasetFileStatusActive.Where(w => w.Schema.SchemaId == stepEvent.SchemaId && w.FileName == file.FileName && w.ParentDatasetFileId == null && w.DatasetFileId != file.DatasetFileId).ToList();

                    if (previousFileList.Any())
                    {
                       _logger.LogDebug($"schemaservice-registerrawfile setting-parentdatasetfileid detected {previousFileList.Count} file(s) to be updated");
                    }

                    foreach (DatasetFile item in previousFileList)
                    {
                        item.ParentDatasetFileId = file.DatasetFileId;
                    }
                }
                                
                _datasetContext.SaveChanges();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"schemaservice-registerrawfile-failed");
                throw;
            }
        }

        /// <summary>
        /// Recursive function to traverse a schema structure and build a list of names.
        /// </summary>
        /// <param name="field">The field to generate the name for.</param>
        /// <returns>The Path of Structs for the input field</returns>
        public string BuildDotNamePath(BaseField field)
        {
            string hierarchy = "";
            if (field.ParentField != null)
            {
                hierarchy = field.ParentField.DotNamePath;
            }
            if (!String.IsNullOrEmpty(hierarchy))
            {
                hierarchy = hierarchy + ".";
            }
            return hierarchy + field.Name;
        }

        /// <summary>
        /// Recursive function to traverse a layer of schema to call BuildParentChildHierarchy on. 
        /// </summary>
        /// <param name="fields">List of fields to iterate over</param>
        public void SetHierarchyProperties(IList<BaseField> fields)
        {
            int index = 1;
            foreach (BaseField field in fields)
            {
                field.DotNamePath = BuildDotNamePath(field);
                field.StructurePosition = BuildStructurePosition(field, index);
                if (field.ChildFields.Count > 0)
                {
                    SetHierarchyProperties(field.ChildFields);
                }
                index++;
            }
        }

        private string BuildStructurePosition(BaseField field, int index)
        {
            if (field.ParentField == null)
            {
                return index.ToString();
            }
            else
            {
                return $"{field.ParentField.StructurePosition}.{index}";
            }
        }

        private void DeleteElasticIndexForSchema(int schemaId)
        {
            _elasticDocumentClient.DeleteByQuery<ElasticSchemaField>(q => q
                .Query(qm => qm
                    .Bool(b => b
                        .Must(
                            mm => mm.Term(s => s.SchemaId, schemaId)
                        )
                    )
                )
            );
        }

        private void IndexElasticFieldsForSchema(int schemaId, int datasetId, IList<BaseField> fields)
        {
            List<ElasticSchemaField> elasticFields = BaseFieldsFlatten(fields, schemaId, datasetId).ToList();
            _elasticDocumentClient.IndexManyAsync(elasticFields).Wait();
        }

        private HashSet<ElasticSchemaField> BaseFieldsFlatten(IList<BaseField> fields, int schemaId, int datasetId)
        {
            HashSet<ElasticSchemaField> elasticSchemaFields = new HashSet<ElasticSchemaField>();
            foreach(BaseField field in fields)
            {
                if(field.ChildFields.Count > 0)
                {
                    elasticSchemaFields.UnionWith(BaseFieldsFlatten(field.ChildFields, schemaId, datasetId));
                }
                elasticSchemaFields.Add(new ElasticSchemaField(field, schemaId, datasetId));
            }
            return elasticSchemaFields;
        }

        private void CheckAccessToDataset(Dataset ds, Func<UserSecurity, bool> userCan)
        {
            try
            {
                IApplicationUser user = _userService.GetCurrentUser();
                UserSecurity userSecurity = _securityService.GetUserSecurity(ds, user);

                if (userCan(userSecurity))
                {
                    return;
                }

                _logger.LogInformation($"schmeacontroller-checkdatasetpermission unauthorized_access for {user.AssociateId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"schemacontroller-checkdatasetpermission failed to check access");
            }

            throw new SchemaUnauthorizedAccessException();
        }

        private DatasetFileConfig GetDatasetFileConfig(int datasetId, int schemaId, Func<UserSecurity, bool> userCan)
        {
            Dataset ds = _datasetContext.Datasets.FirstOrDefault(x => x.DatasetId == datasetId && x.ObjectStatus == GlobalEnums.ObjectStatusEnum.Active);

            if (ds == null)
            {
                throw new DatasetNotFoundException();
            }

            CheckAccessToDataset(ds, userCan);

            DatasetFileConfig fileConfig = ds.DatasetFileConfigs.FirstOrDefault(w => w.Schema.SchemaId == schemaId);

            if (fileConfig == null)
            {
                throw new SchemaNotFoundException();
            }

            return fileConfig;
        }

        private void MapToDatasetFile(DataFlowStepEvent stepEvent, string fileKey, string fileVersionId, DatasetFile file)
        {
            file.DatasetFileId = 0;
            file.FileName = Path.GetFileName(fileKey);
            file.Dataset = _datasetContext.GetById<Dataset>(stepEvent.DatasetID);
            file.UploadUserName = "";
            file.DatasetFileConfig = null;
            file.FileLocation = stepEvent.StepTargetPrefix + Path.GetFileName(fileKey).Trim();
            file.CreatedDTM = DateTime.ParseExact(stepEvent.FlowExecutionGuid, GlobalConstants.DataFlowGuidConfiguration.GUID_FORMAT, null);
            file.ModifiedDTM = DateTime.Now;
            file.ParentDatasetFileId = null;
            file.VersionId = fileVersionId;
            file.IsBundled = false;
            file.Size = long.Parse(stepEvent.FileSize);
            file.Schema = _datasetContext.GetById<FileSchema>(stepEvent.SchemaId);
            file.SchemaRevision = file.Schema.Revisions.OrderByDescending(o => o.Revision_NBR).Take(1).SingleOrDefault();
            file.DatasetFileConfig = _datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == stepEvent.SchemaId).FirstOrDefault();
            file.FlowExecutionGuid = stepEvent.FlowExecutionGuid;
            file.RunInstanceGuid = (stepEvent.RunInstanceGuid) ?? null;
        }        

        private FileSchema MapToFileSchema(FileSchemaDto dto)
        {
            string storageCode = _datasetContext.GetNextStorageCDE().ToString().PadLeft(7, '0');
            Dataset parentDataset = _datasetContext.GetById<Dataset>(dto.ParentDatasetId);
            bool isHumanResources = parentDataset.IsHumanResources;           //Figure out if Category == HR

            FileSchema schema = new FileSchema()
            {
                Name = dto.Name,
                CreatedBy = _userService.GetCurrentUser().AssociateId,
                SchemaEntity_NME = dto.SchemaEntity_NME,
                Extension = GetSchemaFileExtension(dto),
                Delimiter = dto.Delimiter,
                HasHeader = dto.HasHeader,
                SasLibrary = CommonExtensions.GenerateSASLibaryName(parentDataset),
                Description = dto.Description,
                StorageCode = storageCode,
                HiveDatabase = GenerateHiveDatabaseName(parentDataset.DatasetCategories.First()),
                HiveTable = FormatHiveTableNamePart(parentDataset.DatasetName) + "_" + FormatHiveTableNamePart(dto.Name),
                HiveTableStatus = ConsumptionLayerTableStatusEnum.NameReserved.ToString(),
                HiveLocation = RootBucket + "/" + GlobalConstants.ConvertedFileStoragePrefix.PARQUET_STORAGE_PREFIX + "/" + Configuration.Config.GetHostSetting("S3DataPrefix") + storageCode,
                CreatedDTM = DateTime.Now,
                LastUpdatedDTM = DateTime.Now,
                DeleteIssueDTM = DateTime.MaxValue,
                CreateCurrentView = dto.CreateCurrentView,
                CLA1396_NewEtlColumns = dto.CLA1396_NewEtlColumns,
                CLA1580_StructureHive = dto.CLA1580_StructureHive,
                CLA2472_EMRSend = dto.CLA2472_EMRSend,
                CLA1286_KafkaFlag = dto.CLA1286_KafkaFlag,
                CLA3014_LoadDataToSnowflake = dto.CLA3014_LoadDataToSnowflake,
                ObjectStatus = dto.ObjectStatus,
                SchemaRootPath = dto.SchemaRootPath,
                ParquetStorageBucket = GenerateParquetStorageBucket(isHumanResources, GlobalConstants.SaidAsset.DATA_LAKE_STORAGE, Config.GetDefaultEnvironmentName(), parentDataset.NamedEnvironmentType),
                ParquetStoragePrefix = GenerateParquetStoragePrefix(parentDataset.Asset.SaidKeyCode, parentDataset.NamedEnvironment, storageCode),
                ControlMTriggerName = GetControlMTrigger(dto)
            };
            
            schema.ConsumptionDetails = GenerateConsumptionLayers(dto, schema, parentDataset);           

            _datasetContext.Add(schema);

            return schema;
        }

        private FileExtension GetSchemaFileExtension(FileSchemaDto dto)
        {
            if (dto.FileExtensionId != 0)
            {
                return _datasetContext.GetById<FileExtension>(dto.FileExtensionId);
            }
            else if (dto.FileExtensionName != null)
            {
                return _datasetContext.FileExtensions.Where(w => w.Name == dto.FileExtensionName).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        private FileSchemaDto MapToDto(FileSchema scm)
        {
            return new FileSchemaDto()
            {
                Name = scm.Name,
                CreateCurrentView = scm.CreateCurrentView,
                Delimiter = scm.Delimiter,
                FileExtensionId = scm.Extension.Id,
                FileExtensionName = scm.Extension.Name,
                HasHeader = scm.HasHeader,
                SasLibrary = scm.SasLibrary,
                SchemaEntity_NME = scm.SchemaEntity_NME,
                SchemaId = scm.SchemaId,
                Description = scm.Description,
                ObjectStatus = scm.ObjectStatus,
                DeleteInd = scm.DeleteInd,
                DeleteIssuer = scm.DeleteIssuer,
                DeleteIssueDTM = scm.DeleteIssueDTM,
                HiveTable = scm.HiveTable,
                HiveDatabase = scm.HiveDatabase,
                HiveLocation = scm.HiveLocation,
                HiveStatus = scm.HiveTableStatus,
                StorageCode = scm.StorageCode,
                StorageLocation = Configuration.Config.GetHostSetting("S3DataPrefix") + scm.StorageCode + "\\",
                RawQueryStorage = (Configuration.Config.GetHostSetting("EnableRawQueryStorageInQueryTool").ToLower() == "true" && _datasetContext.SchemaMap.Any(w => w.MappedSchema.SchemaId == scm.SchemaId)) ? GlobalConstants.DataFlowTargetPrefixes.RAW_QUERY_STORAGE_PREFIX + Configuration.Config.GetHostSetting("S3DataPrefix") + scm.StorageCode + "\\" : Configuration.Config.GetHostSetting("S3DataPrefix") + scm.StorageCode + "\\",
                CLA1396_NewEtlColumns = scm.CLA1396_NewEtlColumns,
                CLA1580_StructureHive = scm.CLA1580_StructureHive,
                CLA2472_EMRSend = scm.CLA2472_EMRSend,
                CLA1286_KafkaFlag = scm.CLA1286_KafkaFlag,
                CLA3014_LoadDataToSnowflake = scm.CLA3014_LoadDataToSnowflake,
                SchemaRootPath = scm.SchemaRootPath,
                ParquetStorageBucket = scm.ParquetStorageBucket,
                ParquetStoragePrefix = scm.ParquetStoragePrefix,
                ConsumptionDetails = scm.ConsumptionDetails?.Select(c => c.Accept(new SchemaConsumptionDtoTransformer())).ToList(),
                ControlMTriggerName = scm.ControlMTriggerName,
                CreateDateTime = scm.CreatedDTM,
                UpdateDateTime = scm.LastUpdatedDTM
            };
        }

        private string FormatHiveTableNamePart(string part)
        {
            return part.Replace(" ", "").Replace("_", "").Replace("-", "").ToUpper();
        }

        private string GenerateHiveDatabaseName(Category cat)
        {
            string curEnv = Config.GetDefaultEnvironmentName().ToLower();
            string dbName = "dsc_" + cat.Name.ToLower();

            return (curEnv == "prod" || curEnv == "qual") ? dbName : $"{curEnv}_{dbName}";
        }

        /// <summary>
        /// Returns snowflake database name.  Only returns value for <see cref="SnowflakeConsumptionType.CategorySchemaParquet"/> type.
        /// </summary>
        /// <param name="isHumanResources"></param>
        /// <returns></returns>
        internal virtual string GetSnowflakeDatabaseName(bool isHumanResources)
        {
            return GetSnowflakeDatabaseName(isHumanResources, null, SnowflakeConsumptionType.CategorySchemaParquet);
        }

        /// <summary>
        /// Returns snowflake database name.  Will generate value for all <see cref="SnowflakeConsumptionType"/> types.
        /// </summary>
        /// <param name="isHumanResources"></param>
        /// <param name="datasetNamedEnvironmentType"></param>
        /// <param name="consumptionLayerType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual string GetSnowflakeDatabaseName(bool isHumanResources, string datasetNamedEnvironmentType, SnowflakeConsumptionType consumptionLayerType)
        {
            switch (consumptionLayerType)
            {
                case SnowflakeConsumptionType.CategorySchemaParquet:
                    return GenerateSnowflakeDatabaseName(isHumanResources, Config.GetDefaultEnvironmentName().ToUpper(), null, null);
                case SnowflakeConsumptionType.DatasetSchemaParquet:
                    return GenerateSnowflakeDatabaseName(isHumanResources, Config.GetDefaultEnvironmentName().ToUpper(), datasetNamedEnvironmentType, null);
                case SnowflakeConsumptionType.DatasetSchemaRaw:
                    return GenerateSnowflakeDatabaseName(isHumanResources, Config.GetDefaultEnvironmentName().ToUpper(), datasetNamedEnvironmentType, GlobalConstants.SnowflakeConsumptionLayerPrefixes.RAW_PREFIX);
                case SnowflakeConsumptionType.DatasetSchemaRawQuery:
                    return GenerateSnowflakeDatabaseName(isHumanResources, Config.GetDefaultEnvironmentName().ToUpper(), datasetNamedEnvironmentType, GlobalConstants.SnowflakeConsumptionLayerPrefixes.RAWQUERY_PREFIX);
                default:
                    throw new ArgumentOutOfRangeException(nameof(consumptionLayerType),"Unhandled SnowflakeConsumptionType generating snowflake database name");
            }
        }

        /// <summary>
        /// This method is used by <see cref="GetSnowflakeDatabaseName(bool)"/> or <see cref="GetSnowflakeDatabaseName(bool, string, SnowflakeConsumptionType)"/>.
        /// </summary>
        /// <param name="isHumanResources"></param>
        /// <param name="dscNamedEnvironment"></param>
        /// <param name="datasetNamedEnvironmentType"></param>
        /// <param name="consumptionLayerPrefix"></param>
        /// <returns></returns>
        internal string GenerateSnowflakeDatabaseName(bool isHumanResources, string dscNamedEnvironment, string datasetNamedEnvironmentType, string consumptionLayerPrefix)
        {
            string dbName = (isHumanResources) ? GlobalConstants.SnowflakeDatabase.WDAY : GlobalConstants.SnowflakeDatabase.DATA;

            if (!string.IsNullOrWhiteSpace(consumptionLayerPrefix))
            {
                dbName += consumptionLayerPrefix;
            }

            if (string.IsNullOrWhiteSpace(datasetNamedEnvironmentType))
            {
                dbName += dscNamedEnvironment;
                return dbName;
            }

            if (string.IsNullOrWhiteSpace(_dataFeatures.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue())
                && datasetNamedEnvironmentType == GlobalEnums.NamedEnvironmentType.NonProd.ToString()
                && (dscNamedEnvironment == GlobalConstants.Environments.QUAL || dscNamedEnvironment == GlobalConstants.Environments.PROD))
            {
                dbName += dscNamedEnvironment;
                dbName += GlobalConstants.Environments.NONPROD_SUFFIX.ToUpper();
            }
            else
            {
                dbName += dscNamedEnvironment;
            }

            return dbName;
        }

        /// <summary>
        /// Generates Snowflake schema name for all <see cref="SnowflakeConsumptionType"/> types.
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="consumptionType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual string GetSnowflakeSchemaName(Dataset dataset, SnowflakeConsumptionType consumptionType)
        {
            switch (consumptionType)
            {
                case SnowflakeConsumptionType.CategorySchemaParquet:
                    return GenerateCategoryBasedSnowflakeSchemaName(dataset);
                case SnowflakeConsumptionType.DatasetSchemaParquet:
                case SnowflakeConsumptionType.DatasetSchemaRaw:
                case SnowflakeConsumptionType.DatasetSchemaRawQuery:
                    return GenerateDatasetBasedSnowflakeSchemaName(dataset, bool.Parse(Configuration.Config.GetHostSetting("AlwaysSuffixSchemaNames")));
                default:
                    throw new ArgumentOutOfRangeException(nameof(consumptionType),$"Not configured for snowflakeconsumptionlayertype of {consumptionType}");
            }
        }


        /// <summary>
        /// Use this method by calling <see cref="GetSnowflakeSchemaName(Dataset, SnowflakeConsumptionType)"/>
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="alwaysSuffixSchemaNames"></param>
        /// <returns></returns>
        internal string GenerateDatasetBasedSnowflakeSchemaName(Dataset dataset, bool alwaysSuffixSchemaNames)
        {
            
            string cleansedDatasetName = dataset.DatasetName.Replace(" ", "").Replace("_", "").Replace("-", "").ToUpper();
            string schemaName = cleansedDatasetName;

            /*********************
            *  When CLA4260 feature flag is off, system will not allow to datasets with same name.  Therefore, we will not have duplicated snowflake schema.
            *  
            *  When CLA4260 feature flag is on, we will have multiple datasets with same name but different named environments.  
            *  Therefore, we need to ensure snowflake schema is unique.  The check where != "QUAL" is to ensure we generate same
            *  snowflake schema names between PROD and NonPROD (QUAL) schemas.  This will give users better query experience between QUAL and PROD.
            *********************/            
#pragma warning disable S2589 // Boolean expressions should not be gratuitous
            if (string.IsNullOrWhiteSpace(_dataFeatures.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue())
                && (alwaysSuffixSchemaNames 
                    || (!alwaysSuffixSchemaNames && dataset.NamedEnvironmentType == GlobalEnums.NamedEnvironmentType.NonProd && dataset.NamedEnvironment != "QUAL")
                    )
                )
#pragma warning restore S2589 // Boolean expressions should not be gratuitous
            {
                schemaName += "_" + dataset.NamedEnvironment;
            }

            return schemaName;
        }

        /// <summary>
        /// Use this method by calling <see cref="GetSnowflakeSchemaName(Dataset, SnowflakeConsumptionType)"/>
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        internal string GenerateCategoryBasedSnowflakeSchemaName(Dataset dataset)
        {
            return (dataset.IsHumanResources) ? "HR" : dataset.DatasetCategories.First().Name.ToUpper();
        }


        /// <summary>
        /// Generates appropriate bucket after evaluating various variables
        /// </summary>
        /// <param name="isHumanResources"></param>
        /// <param name="saidKeyCode">Data storage layer SAID asset</param>
        /// <param name="dscNamedEnvironment">DSC physical named environment</param>
        /// <param name="datasetNamedEnvironment">Dataset named environment</param>
        /// <remarks> This method is only used by this class, therefore, setting to 
        /// internal for exposure to Sentry.data.Core.Tests project for unit testing. </remarks>
        /// <returns></returns>
        internal string GenerateParquetStorageBucket(bool isHumanResources, string saidKeyCode, string dscNamedEnvironment, GlobalEnums.NamedEnvironmentType datasetNamedEnvironmentType)
        {
            string baseBucketName = (isHumanResources)
                ? GlobalConstants.AwsBuckets.HR_DATASET_BUCKET_AE2
                : GlobalConstants.AwsBuckets.BASE_DATASET_BUCKET_AE2;

            string namedEnvironment = dscNamedEnvironment.ToLower();
            if (string.IsNullOrWhiteSpace(_dataFeatures.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue())
                && datasetNamedEnvironmentType == GlobalEnums.NamedEnvironmentType.NonProd
                && (dscNamedEnvironment == GlobalConstants.Environments.QUAL || dscNamedEnvironment == GlobalConstants.Environments.PROD))
            {
                namedEnvironment += GlobalConstants.Environments.NONPROD_SUFFIX.ToLower();
            }

            string bucketName = baseBucketName.Replace("<saidkeycode>", saidKeyCode.ToLower()).Replace("<namedenvironment>", namedEnvironment.ToLower());
            return bucketName;
        }

        internal string GenerateParquetStoragePrefix(string saidKeyCode, string namedEnvironment, string storageCode)
        {
            if (string.IsNullOrEmpty(saidKeyCode))
            {
                throw new ArgumentNullException("saidKeyCode", "SAID keycode is required to generate parquet storage prefix");
            }
            if (string.IsNullOrEmpty(storageCode))
            {
                throw new ArgumentNullException("storageCode", "Storage code is required to generate parquet storage prefix");
            }

            string prefix;

            if (string.IsNullOrEmpty(namedEnvironment))
            {
                throw new ArgumentNullException("namedEnvironment", "Named environment is required to generate parquet storage prefix");
            }

            prefix = $"{GlobalConstants.ConvertedFileStoragePrefix.PARQUET_STORAGE_PREFIX}/{saidKeyCode.ToUpper()}/{namedEnvironment.ToUpper()}/{storageCode}";

            return prefix;
        }

        private string FormatSnowflakeTableNamePart(string part)
        {
            return part.Replace(" ", "").Replace("_", "").Replace("-", "").ToUpper();
        }

        internal BaseField AddRevisionField(BaseFieldDto row, SchemaRevision CurrentRevision, BaseField parentRow = null, SchemaRevision previousRevision = null)
        {
            BaseField newField = null;
            //Should we perform comparison to previous based on is incoming field new
            bool compare = (row.FieldGuid.ToString() != Guid.Empty.ToString() && previousRevision != null);

            //if comparing, pull field from previous version
            BaseFieldDto previousFieldDtoVersion = (compare) ? previousRevision.Fields.FirstOrDefault(w => w.FieldGuid == row.FieldGuid).ToDto() : null;
            
            bool changed = false;
            newField = row.ToEntity(parentRow, CurrentRevision);

            if (compare && previousFieldDtoVersion != null)
            {
                changed = previousFieldDtoVersion.CompareToEntity(newField);
                newField.LastUpdateDTM = (changed) ? CurrentRevision.LastUpdatedDTM : previousFieldDtoVersion.LastUpdatedDtm;
            }
            else
            {
                newField.CreateDTM = CurrentRevision.CreatedDTM;
                newField.LastUpdateDTM = CurrentRevision.CreatedDTM;
            }

            _datasetContext.Add(newField);

            //if there are child rows, perform a recursive call to this function
            if (row.ChildFields != null)
            {
                foreach (BaseFieldDto cRow in row.ChildFields)
                {
                    //When ParentField is set in ToEntity, field is automatically added as child field, the returned field therefore does not need to be added to the ChildField list
                    AddRevisionField(cRow, CurrentRevision, newField, previousRevision);
                }
            }

            return newField;
        }

        public static Object TryConvertTo<T>(Object input)
        {
            Object result = null;
            try
            {
                result = Convert.ChangeType(input, typeof(T));
                return result;
            }
            catch (Exception)
            {
                return result;
            }
        }

        public void ValidateCleanedFields(int schemaId, List<BaseFieldDto> fieldDtoList)
        {
            MethodBase mBase = System.Reflection.MethodBase.GetCurrentMethod();
           _logger.LogDebug($"schemaservice start method <{mBase.Name.ToLower()}>");

            FileSchema schema = _datasetContext.GetById<FileSchema>(schemaId);

            ValidationResults errors = ValidateCleanedFields(schema.Extension.Name, fieldDtoList);

            if (!errors.IsValid())
            {
                throw new ValidationException(errors);
            }

           _logger.LogDebug($"schemaservice end method <{mBase.Name.ToLower()}>");
        }

        private ValidationResults ValidateCleanedFields(string extensionName, List<BaseFieldDto> fieldDtoList)
        {
            //STEP 1:  Look for clones (duplicates) and add to results
            ValidationResults results = CloneWars(fieldDtoList);

            //STEP 2:  go through all fields and look for validation errors
            foreach (BaseFieldDto fieldDto in fieldDtoList)
            {
                fieldDto.Clean(extensionName);
                ValidationResults fieldValidationResults = fieldDto.Validate(extensionName);
                results.MergeInResults(fieldValidationResults);

                //recursively call validate again to validate all child fields of parent
                if (fieldDto.ChildFields.Any())
                {
                    results.MergeInResults(ValidateCleanedFields(extensionName, fieldDto.ChildFields));
                }
            }

            return results;
        }

        //look at fieldDtoList and returns a list of duplicates at that level only
        //this function DOES NOT drill into children since method that calls it will call this for every level that exists 
        private ValidationResults CloneWars(List<BaseFieldDto> fieldDtoList)
        {
            ValidationResults results = new ValidationResults();

            //STEP 1:  Find duplicates at the current level passed in
            var clones = fieldDtoList.Where(w => !w.DeleteInd).GroupBy(x => x.Name).Where(x => x.Count() > 1).Select(y => y.Key);

            //STEP 2:  IF ANY CLONES EXIST: Grab all clones that match distinct name list
            //this step is here because i need ALL duplicates and ordinal position to send error message
            if (clones.Any())
            {
                var cloneDetails =
                   from f in fieldDtoList
                   join c in clones on f.Name equals c
                   select new { f.Name, f.OrdinalPosition };

                //ADD ALL CLONE ERRORS to ValidationResults class, ValidationResults is what gets returned from Validate() and is used to display errors
                //NOTE: this code uses linq ToList() extension method on my cloneDetails IEnumerable to essentially go through each cloneDetail and call results.Add()
                //we are adding each errors to ValidationResults, this way I don't need to create a seperate hardened list but can add to the existing ValidationResults
                cloneDetails.ToList().ForEach(x => results.Add(x.OrdinalPosition.ToString(), $"({x.Name}) cannot be duplicated. "));
            }

            return results;
        }

        internal IList<SchemaConsumption> GenerateConsumptionLayers(FileSchemaDto dto, FileSchema schema, Dataset parentDataset)
        {
            List<SchemaConsumption> layerList = new List<SchemaConsumption>();

            //All schemas, regardless of when dataset is created, will have dataset based consumption layers generated
            layerList.AddRange(
            new List<SchemaConsumption>()
            {
                new SchemaConsumptionSnowflake()
                {
                    Schema = schema,
                    SnowflakeDatabase = GetSnowflakeDatabaseName(parentDataset.IsHumanResources, parentDataset.NamedEnvironmentType.ToString(), SnowflakeConsumptionType.DatasetSchemaParquet),
                    SnowflakeSchema = GetSnowflakeSchemaName(parentDataset, SnowflakeConsumptionType.DatasetSchemaParquet),
                    SnowflakeTable = FormatSnowflakeTableNamePart(dto.Name),
                    SnowflakeStatus = ConsumptionLayerTableStatusEnum.NameReserved.ToString(),
                    SnowflakeStage = GlobalConstants.SnowflakeStageNames.PARQUET_STAGE,
                    SnowflakeWarehouse = GlobalConstants.SnowflakeWarehouse.WAREHOUSE_NAME,
                    SnowflakeType = SnowflakeConsumptionType.DatasetSchemaParquet
                },
                new SchemaConsumptionSnowflake()
                {
                    Schema = schema,
                    SnowflakeDatabase = GetSnowflakeDatabaseName(parentDataset.IsHumanResources, parentDataset.NamedEnvironmentType.ToString(), SnowflakeConsumptionType.DatasetSchemaRawQuery),
                    SnowflakeSchema = GetSnowflakeSchemaName(parentDataset, SnowflakeConsumptionType.DatasetSchemaRawQuery),
                    SnowflakeTable = FormatSnowflakeTableNamePart(dto.Name),
                    SnowflakeStatus = ConsumptionLayerTableStatusEnum.NameReserved.ToString(),
                    SnowflakeStage = GlobalConstants.SnowflakeStageNames.RAWQUERY_STAGE,
                    SnowflakeWarehouse = GlobalConstants.SnowflakeWarehouse.WAREHOUSE_NAME,
                    SnowflakeType = SnowflakeConsumptionType.DatasetSchemaRawQuery
                },
                new SchemaConsumptionSnowflake()
                {
                    Schema = schema,
                    SnowflakeDatabase = GetSnowflakeDatabaseName(parentDataset.IsHumanResources, parentDataset.NamedEnvironmentType.ToString(), SnowflakeConsumptionType.DatasetSchemaRaw),
                    SnowflakeSchema = GetSnowflakeSchemaName(parentDataset, SnowflakeConsumptionType.DatasetSchemaRaw),
                    SnowflakeTable = FormatSnowflakeTableNamePart(dto.Name),
                    SnowflakeStatus = ConsumptionLayerTableStatusEnum.NameReserved.ToString(),
                    SnowflakeStage = GlobalConstants.SnowflakeStageNames.RAW_STAGE,
                    SnowflakeWarehouse = GlobalConstants.SnowflakeWarehouse.WAREHOUSE_NAME,
                    SnowflakeType = SnowflakeConsumptionType.DatasetSchemaRaw
                }
            });

            return layerList;
        }
    }
}
