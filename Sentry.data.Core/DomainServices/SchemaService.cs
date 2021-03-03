﻿using Sentry.Common.Logging;
using Sentry.data.Core.Exceptions;
using Sentry.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Sentry.Core;
using System.Text.RegularExpressions;
using StructureMap;
using System.Data.Odbc;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Sentry.data.Core
{
    public class SchemaService : ISchemaService
    {
        public readonly IDataFlowService _dataFlowService;
        public readonly IJobService _jobService;
        private readonly IDatasetContext _datasetContext;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly ISecurityService _securityService;
        private readonly IDataFeatures _featureFlags;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IHiveOdbcProvider _hiveOdbcProvider;
        private string _bucket;

        public SchemaService(IDatasetContext dsContext, IUserService userService, IEmailService emailService,
            IDataFlowService dataFlowService, IJobService jobService, ISecurityService securityService,
            IDataFeatures dataFeatures, IMessagePublisher messagePublisher, IHiveOdbcProvider hiveOdbcProvider)
        {
            _datasetContext = dsContext;
            _userService = userService;
            _emailService = emailService;
            _dataFlowService = dataFlowService;
            _jobService = jobService;
            _securityService = securityService;
            _featureFlags = dataFeatures;
            _messagePublisher = messagePublisher;
            _hiveOdbcProvider = hiveOdbcProvider;
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

        public int CreateAndSaveSchema(FileSchemaDto schemaDto)
        {
            FileSchema newSchema;
            try
            {
                newSchema = CreateSchema(schemaDto);

                //_dataFlowService.CreateandSaveDataFlow(MapToDataFlowDto(newSchema));

                _dataFlowService.CreateDataFlowForSchema(newSchema);

                _datasetContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("schemaservice-createandsaveschema", ex);
                return 0;
            }

            return newSchema.SchemaId;
        }

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
                        Logger.Info($"{nameof(SchemaService).ToLower()}-{nameof(CreateAndSaveSchemaRevision).ToLower()} unauthorized_access: Id:{user.AssociateId}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"{nameof(SchemaService).ToLower()}-{nameof(CreateAndSaveSchemaRevision).ToLower()} unauthorized_access", ex);
                    }
                    throw new SchemaUnauthorizedAccessException();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{nameof(SchemaService).ToLower()}-{nameof(CreateAndSaveSchemaRevision).ToLower()} failed to retrieve UserSecurity object", ex);
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

                    _datasetContext.SaveChanges();

                    
                    GenerateConsumptionLayerCreateEvent(schema, JObject.Parse("{\"revision\":\"added\"}"));
                                        
                    return revision.SchemaRevision_Id;
                }

                return 0;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to add revision", ex);

                return 0;
            }
        }


        public bool UpdateAndSaveSchema(FileSchemaDto schemaDto)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            Logger.Info($"startmethod <{m.ReflectedType.Name.ToString()}>");
            Dataset parentDataset = _datasetContext.DatasetFileConfigs.FirstOrDefault(w => w.Schema.SchemaId == schemaDto.SchemaId).ParentDataset;
            IApplicationUser user = _userService.GetCurrentUser();
            UserSecurity us = _securityService.GetUserSecurity(parentDataset, user);

            if (!us.CanManageSchema)
            {
                throw new SchemaUnauthorizedAccessException();
            }
            
            //var SendSASNotification = false;
            string SASNotificationType = null;
            string CurrentViewNotificationType = null;
            JObject whatPropertiesChanged;
            FileSchema schema;
            /* Any exceptions saving schema changes, do not execute remaining line of code */
            try
            {
                schema = _datasetContext.GetById<FileSchema>(schemaDto.SchemaId);

                //Update/save schema within DSC metadata
                whatPropertiesChanged = UpdateSchema(schemaDto, schema);
                Logger.Info($"<{m.ReflectedType.Name.ToLower()}> Changes detected for {parentDataset.DatasetName}\\{schema.Name} | {whatPropertiesChanged.ToString()}");
                _datasetContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error($"<{m.ReflectedType.Name.ToLower()}> Failed schema save", ex);
                return false;
            }

            /* The remaining actions should all be executed even if one fails
             * If there are any exceptions, log the exceptions and continue on */
            var exceptions = new List<Exception>();

            /* Trigger email only if IsInSAS has changed, otherwise, let consumption layer event processing drive the email to SAS Admins */
            if (whatPropertiesChanged.ContainsKey("isinsas") &&
                (!whatPropertiesChanged.ContainsKey("createcurrentview") && !whatPropertiesChanged.ContainsKey("cla2429_snowflakecreatetable")))
            {

                CurrentViewNotificationType = (schemaDto.CreateCurrentView) ? "ADD" : "REMOVE";
                SASNotificationType = (schema.IsInSAS) ? "ADD" : "REMOVE";

                try
                {
                    Logger.Info($"<{m.ReflectedType.Name.ToLower()}> sending sas notification email for hive...");
                    SasNotification(schema, SASNotificationType, CurrentViewNotificationType, _userService.GetCurrentUser(), "HIVE");
                    Logger.Info($"<{m.ReflectedType.Name.ToLower()}> sent sas notification email for hive");
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }


                if (schema.CLA2429_SnowflakeCreateTable)
                {
                    try
                    {
                        Logger.Info($"<{m.ReflectedType.Name.ToLower()}> sending sas notification email for snowflake...");
                        SasNotification(schema, SASNotificationType, CurrentViewNotificationType, _userService.GetCurrentUser(), "SNOWFLAKE");
                        Logger.Info($"<{m.ReflectedType.Name.ToLower()}> sent sas notification email for snowflake...");
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }
            }

            /*
            * Generate consumption layer events to dsc event topic
            *  This ensures schema is updated appropriately with
            *  adjustements made within 
            */
            try
            {
                GenerateConsumptionLayerEvents(schema, whatPropertiesChanged);
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
                Logger.Error($"<{m.ReflectedType.Name.ToLower()}> Failed sending downstream notifications or events", new AggregateException(exceptions));
            }

            Logger.Info($"endmethod <{m.ReflectedType.Name.ToString()}>");
            return true;
            
        }

        private void GenerateConsumptionLayerEvents(FileSchema schema, JObject propertyDeltaList)
        {
            /*Generate create event when:
            *  - CreateCurrentView changes, regardless which way
            *  - CLA2429_SnowflakeCreateTable changes to true
            */
            if (
                //(propertyDeltaList.ContainsKey("createcurrentview") && propertyDeltaList.GetValue("createcurrentview").ToString().ToLower() == "true") ||
                (propertyDeltaList.ContainsKey("createcurrentview")) ||
                (propertyDeltaList.ContainsKey("cla2429_snowflakecreatetable") && propertyDeltaList.GetValue("cla2429_snowflakecreatetable").ToString().ToLower() == "true")
                )
            {
                GenerateConsumptionLayerCreateEvent(schema, propertyDeltaList);
            }

            /*Generate delete event when:
             *  - CLA2429_SnowflakeCreateTable changes to false
             */
            if (
                //(propertyDeltaList.ContainsKey("createcurrentview") && propertyDeltaList.GetValue("createcurrentview").ToString() == "false") ||
                (propertyDeltaList.ContainsKey("cla2429_snowflakecreatetable") && propertyDeltaList.GetValue("cla2429_snowflakecreatetable").ToString() == "false")
                )
            {
                GenerateConsumptionLayerDeleteEvent(schema, propertyDeltaList);
            }
        }

        private void GenerateConsumptionLayerCreateEvent(FileSchema schema, JObject propertyDeltaList)
        {
            //SchemaRevision latestRevision = null;
            var latestRevision = _datasetContext.SchemaRevision
                .Where(w => w.ParentSchema.SchemaId == schema.SchemaId)
                .Select(s => new {s.ParentSchema.SchemaId, s.SchemaRevision_Id, s.Revision_NBR })
                .OrderByDescending(o => o.Revision_NBR)
                .Take(1).FirstOrDefault();

            //Do nothing if there is no revision associated with schema
            if (latestRevision == null)
            { return; }
                        
            int dsId = _datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == schema.SchemaId).Select(s => s.ParentDataset.DatasetId).FirstOrDefault();

            bool sendHiveMessage = false;
            /* schema column updates trigger, this will trigger for initial schema column add along with any updates there after */
            if (propertyDeltaList.ContainsKey("revision") && propertyDeltaList.GetValue("revision").ToString().ToLower() == "added")
            {
                sendHiveMessage = true;
            }
            /* schema configuration trigger for IsInSAS */
            else if (propertyDeltaList.ContainsKey("isinsas") && propertyDeltaList.GetValue("isinsas").ToString().ToLower() == "true")
            {
                sendHiveMessage = true;
            }
            /* schema configuration trigger for createCurrentView regardless true\false */
            else if (propertyDeltaList.ContainsKey("createcurrentview"))
            {
                sendHiveMessage = true;
            }

            if (sendHiveMessage)
            {
                HiveTableCreateModel hiveCreate = new HiveTableCreateModel()
                {
                    SchemaID = latestRevision.SchemaId,
                    RevisionID = latestRevision.SchemaRevision_Id,
                    DatasetID = dsId,
                    HiveStatus = null,
                    InitiatorID = _userService.GetCurrentUser().AssociateId,
                    ChangeIND = propertyDeltaList.ToString(Formatting.None)
                };

                Logger.Debug($"<generateconsumptionlayercreateevent> sending {hiveCreate.EventType.ToLower()} event...");
                _messagePublisher.PublishDSCEvent(schema.SchemaId.ToString(), JsonConvert.SerializeObject(hiveCreate));
                Logger.Debug($"<generateconsumptionlayercreateevent> sent {hiveCreate.EventType.ToLower()} event");
            }


            bool sendSnowMessage = false;
            if (propertyDeltaList.ContainsKey("revision") && propertyDeltaList.GetValue("revision").ToString().ToLower() == "added" && schema.CLA2429_SnowflakeCreateTable)
            {
                sendSnowMessage = true;
            }
            else if (propertyDeltaList.ContainsKey("cla2429_snowflakecreatetable") && propertyDeltaList.GetValue("cla2429_snowflakecreatetable").ToString().ToLower() == "true")
            {
                sendSnowMessage = true;
            }
            /* schema configuration trigger for createCurrentView regardless true\false */
            else if (propertyDeltaList.ContainsKey("createcurrentview") && schema.CLA2429_SnowflakeCreateTable)
            {
                sendSnowMessage = true;
            }

            if (sendSnowMessage)
            {
                SnowTableCreateModel snowModel = new SnowTableCreateModel()
                {
                    DatasetID = dsId,
                    SchemaID = schema.SchemaId,
                    RevisionID = latestRevision.SchemaRevision_Id,
                    InitiatorID = _userService.GetCurrentUser().AssociateId,
                    ChangeIND = propertyDeltaList.ToString(Formatting.None)
                };

                Logger.Debug($"<generateconsumptionlayercreateevent> sending {snowModel.EventType.ToLower()} event...");
                _messagePublisher.PublishDSCEvent(snowModel.SchemaID.ToString(), JsonConvert.SerializeObject(snowModel));
                Logger.Debug($"<generateconsumptionlayercreateevent> sent {snowModel.EventType.ToLower()} event");
            }       
        }

        private void GenerateConsumptionLayerDeleteEvent(FileSchema schema, JObject propertyDeltaList)
        {
            //Do nothing if there is no revision associated with schema 
            if (!_datasetContext.SchemaRevision.Where(w => w.ParentSchema.SchemaId == schema.SchemaId).Any()) { return; }

            Dataset ds = _datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == schema.SchemaId).Select(s => s.ParentDataset).FirstOrDefault();





            SnowTableDeleteModel snowModel = new SnowTableDeleteModel()
            {
                DatasetID = ds.DatasetId,
                SchemaID = schema.SchemaId,
                InitiatorID = _userService.GetCurrentUser().AssociateId
            };

            _messagePublisher.PublishDSCEvent(snowModel.SchemaID.ToString(), JsonConvert.SerializeObject(snowModel));
        }

        /// <summary>
        /// Updates existing schema object from DTO.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="schema"></param>
        /// <returns> Returns list of properties that have changed.</returns>
        private JObject UpdateSchema(FileSchemaDto dto, FileSchema schema)
        {
            bool chgDetected = false;
            string whatPropertiesChanged = "{";

            if (schema.Name != dto.Name)
            {
                schema.Name = dto.Name;
                chgDetected = true;
            }
            if (schema.Delimiter != dto.Delimiter)
            {
                schema.Description = dto.Description;
                chgDetected = true;
            }

            if (schema.CreateCurrentView != dto.CreateCurrentView)
            {
                schema.CreateCurrentView = dto.CreateCurrentView;
                chgDetected = true;

                if (whatPropertiesChanged != "{")
                { whatPropertiesChanged += ","; }

                whatPropertiesChanged += $"\"createcurrentview\":\"{schema.CreateCurrentView.ToString().ToLower()}\"";
            }

            if (schema.Description != dto.Description)
            {
                schema.Description = dto.Description;
                chgDetected = true;
            }
            if (schema.Extension.Id != dto.FileExtensionId)
            {
                schema.Extension = _datasetContext.GetById<FileExtension>(dto.FileExtensionId);
                chgDetected = true;
            }
            if (schema.HasHeader != dto.HasHeader)
            {
                schema.HasHeader = dto.HasHeader;
                chgDetected = true;
            }
            if (schema.IsInSAS != dto.IsInSas)
            {
                schema.IsInSAS = dto.IsInSas;
                chgDetected = true;

                if (whatPropertiesChanged != "{")
                { whatPropertiesChanged += ","; }

                whatPropertiesChanged += $"\"isinsas\":\"{schema.IsInSAS.ToString().ToLower()}\"";
            }
            if (schema.CLA1396_NewEtlColumns != dto.CLA1396_NewEtlColumns)
            {
                schema.CLA1396_NewEtlColumns = dto.CLA1396_NewEtlColumns;
                chgDetected = true;
            }
            if (schema.CLA1580_StructureHive != dto.CLA1580_StructureHive)
            {
                schema.CLA1580_StructureHive = dto.CLA1580_StructureHive;
                chgDetected = true;
            }

            if (schema.CLA2472_EMRSend != dto.CLA2472_EMRSend)
            {
                schema.CLA2472_EMRSend = dto.CLA2472_EMRSend;
                chgDetected = true;
            }

            if (schema.CLA2429_SnowflakeCreateTable != dto.CLA2429_SnowflakeCreateTable)
            {
                schema.CLA2429_SnowflakeCreateTable = dto.CLA2429_SnowflakeCreateTable;
                chgDetected = true;

                if (whatPropertiesChanged != "{")
                { whatPropertiesChanged += ","; }

                whatPropertiesChanged += $"\"cla2429_snowflakecreatetable\":\"{schema.CLA2429_SnowflakeCreateTable.ToString().ToLower()}\"";
            }

            if (schema.CLA1286_KafkaFlag != dto.CLA1286_KafkaFlag)
            {
                schema.CLA1286_KafkaFlag = dto.CLA1286_KafkaFlag;
                chgDetected = true;
            }


            if (chgDetected)
            {
                schema.LastUpdatedDTM = DateTime.Now;
                schema.UpdatedBy = _userService.GetCurrentUser().AssociateId;
            }

            whatPropertiesChanged += "}";

            return JObject.Parse(whatPropertiesChanged);
            //return Newtonsoft.Json.JsonConvert.DeserializeObject<object>(whatPropertiesChanged);

            //return whatPropertiesChanged;           
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
                        Logger.Info($"schemacontroller-fetSchemarevisiondtobyschema unauthorized_access: Id:{user.AssociateId}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("schemacontroller-fetSchemarevisiondtobyschema unauthorized_access", ex);
                    }
                    throw new SchemaUnauthorizedAccessException();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"schemacontroller-fetSchemarevisiondtobyschema failed to retrieve UserSecurity object", ex);
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
            SchemaRevision revision = _datasetContext.SchemaRevision.FirstOrDefault(w => w.SchemaRevision_Id == revisionId);

            return revision.Fields.Where(w => w.ParentField == null).OrderBy(o => o.OrdinalPosition).ToList().ToDto();
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
                UserSecurity us;
                us = _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());
                if (!(us.CanPreviewDataset || us.CanViewFullDataset || us.CanUploadToDataset || us.CanEditDataset || us.CanManageSchema))
                {
                    try
                    {
                        IApplicationUser user = _userService.GetCurrentUser();
                        Logger.Info($"schemacontroller-getlatestschemarevisiondtobyschema unauthorized_access: Id:{user.AssociateId}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("schemacontroller-getlatestschemarevisiondtobyschema unauthorized_access", ex);
                    }
                    throw new SchemaUnauthorizedAccessException();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"schemacontroller-getlatestschemarevisiondtobyschema failed to retrieve UserSecurity object", ex);
                throw new SchemaUnauthorizedAccessException();
            }

            SchemaRevision revision = _datasetContext.SchemaRevision.Where(w => w.ParentSchema.SchemaId == schemaId).OrderByDescending(o => o.Revision_NBR).Take(1).FirstOrDefault();

            return (revision == null) ? null : revision.ToDto();
        }

        public List<DatasetFile> GetDatasetFilesBySchema(int schemaId)
        {
            List<DatasetFile> fileList = _datasetContext.DatasetFile.Where(w => w.Schema.SchemaId == schemaId).ToList();
            return fileList;
        }

        public DatasetFile GetLatestDatasetFileBySchema(int schemaId)
        {
            DatasetFile file = _datasetContext.DatasetFile.OrderBy(x => x.CreateDTM).FirstOrDefault(w => w.Schema.SchemaId == schemaId);
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
                        Logger.Info($"schemacontroller-{System.Reflection.MethodBase.GetCurrentMethod().Name.ToLower()} unauthorized_access: Id:{user.AssociateId}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"schemacontroller-{System.Reflection.MethodBase.GetCurrentMethod().Name.ToLower()} unauthorized_access", ex);
                    }
                    throw new SchemaUnauthorizedAccessException();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"schemacontroller-{System.Reflection.MethodBase.GetCurrentMethod().Name.ToLower()} failed to retrieve UserSecurity object", ex);
                throw new SchemaUnauthorizedAccessException();
            }

            FileSchemaDto schemaDto = GetFileSchemaDto(id);

            string hiveView = $"vw_{schemaDto.HiveTable}";

            //Get connection object
            OdbcConnection connection = _hiveOdbcProvider.GetConnection(schemaDto.HiveDatabase);

            //Does table exist
            bool tableExists = _hiveOdbcProvider.CheckViewExists(connection, hiveView);

            //If table does not exist
            if (!tableExists)
            {
                throw new HiveTableViewNotFoundException("Table or view not found");
            }

            //Query table for rows
            System.Data.DataTable result = _hiveOdbcProvider.GetTopNRows(_hiveOdbcProvider.GetConnection(schemaDto.HiveDatabase), hiveView, rows);

            List<Dictionary<string, object>> dicRows = new List<Dictionary<string, object>>();
            Dictionary<string, object> dicRow = null;
            foreach (System.Data.DataRow dr in result.Rows)
            {
                dicRow = new Dictionary<string, object>();
                foreach (System.Data.DataColumn col in result.Columns)
                {
                    //R
                    string colName = col.ColumnName.Split('.')[1];
                    dicRow.Add(colName, dr[col]);
                }
                dicRows.Add(dicRow);
            }

            return dicRows;
        }

        public bool RegisterRawFile(FileSchema schema, string objectKey, string versionId, DataFlowStepEvent stepEvent)
        {
            if (objectKey == null)
            {
                Logger.Debug($"schemaservice-registerrawfile no-objectkey-input");
                return false;
            }

            if (schema == null)
            {
                Logger.Debug($"schemaservice-registerrawfile no-schema-input");
                return false;
            }

            if (stepEvent == null)
            {
                Logger.Debug($"schemaservice-registerrawfile no-stepevent-input");
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
                    previousFileList = _datasetContext.DatasetFile.Where(w => w.Schema.SchemaId == stepEvent.SchemaId && w.FileName == file.FileName && w.ParentDatasetFileId == null && w.DatasetFileId != file.DatasetFileId).ToList();

                    if (previousFileList.Any())
                    {
                        Logger.Debug($"schemaservice-registerrawfile setting-parentdatasetfileid detected {previousFileList.Count} file(s) to be updated");
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
                Logger.Error($"schemaservice-registerrawfile-failed", ex);

                return false;
            }           

            return true;
        }

        private void MapToDatasetFile(DataFlowStepEvent stepEvent, string fileKey, string fileVersionId, DatasetFile file)
        {
            file.DatasetFileId = 0;
            file.FileName = Path.GetFileName(fileKey);
            file.Dataset = _datasetContext.GetById<Dataset>(stepEvent.DatasetID);
            file.UploadUserName = "";
            file.DatasetFileConfig = null;
            file.FileLocation = stepEvent.StepTargetPrefix + Path.GetFileName(fileKey).Trim();
            file.CreateDTM = DateTime.ParseExact(stepEvent.FlowExecutionGuid, GlobalConstants.DataFlowGuidConfiguration.GUID_FORMAT, null);
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

        

        private FileSchema CreateSchema(FileSchemaDto dto)
        {
            string storageCode = _datasetContext.GetNextStorageCDE().ToString();
            Dataset parentDataset = _datasetContext.GetById<Dataset>(dto.ParentDatasetId);
            FileSchema schema = new FileSchema()
            {
                Name = dto.Name,
                CreatedBy = _userService.GetCurrentUser().AssociateId,
                SchemaEntity_NME = dto.SchemaEntity_NME,
                Extension = (dto.FileExtensionId != 0) ? _datasetContext.GetById<FileExtension>(dto.FileExtensionId) : (dto.FileExtenstionName != null) ? _datasetContext.FileExtensions.Where(w => w.Name == dto.FileExtenstionName).FirstOrDefault() : null,
                Delimiter = dto.Delimiter,
                HasHeader = dto.HasHeader,
                IsInSAS = dto.IsInSas,
                SasLibrary = CommonExtensions.GenerateSASLibaryName(_datasetContext.GetById<Dataset>(dto.ParentDatasetId)),
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
                SnowflakeDatabase = GenerateSnowflakeDatabaseName(),
                SnowflakeSchema = GenerateSnowflakeSchema(parentDataset.DatasetCategories.First()),
                SnowflakeTable = FormateSnowflakeTableNamePart(parentDataset.DatasetName) + "_" + FormateSnowflakeTableNamePart(dto.Name),
                SnowflakeStatus = ConsumptionLayerTableStatusEnum.NameReserved.ToString(),
                CLA1396_NewEtlColumns = dto.CLA1396_NewEtlColumns,
                CLA1580_StructureHive = dto.CLA1580_StructureHive,
                CLA2472_EMRSend = dto.CLA2472_EMRSend,
                CLA2429_SnowflakeCreateTable = dto.CLA2429_SnowflakeCreateTable,
                CLA1286_KafkaFlag = dto.CLA1286_KafkaFlag
            };
            _datasetContext.Add(schema);
            return schema;
        }

        private FileSchemaDto MapToDto(FileSchema scm)
        {
            return new FileSchemaDto()
            {
                Name = scm.Name,
                CreateCurrentView = scm.CreateCurrentView,
                Delimiter = scm.Delimiter,
                FileExtensionId = scm.Extension.Id,
                HasHeader = scm.HasHeader,
                IsInSas = scm.IsInSAS,
                SasLibrary = scm.SasLibrary,
                SchemaEntity_NME = scm.SchemaEntity_NME,
                SchemaId = scm.SchemaId,
                Description = scm.Description,
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
                FileExtenstionName = scm.Extension.Name,
                SnowflakeDatabase = scm.SnowflakeDatabase,
                SnowflakeTable = scm.SnowflakeTable,
                SnowflakeSchema = scm.SnowflakeSchema,
                SnowflakeStatus = scm.SnowflakeStatus,
                CLA1396_NewEtlColumns = scm.CLA1396_NewEtlColumns,
                CLA1580_StructureHive = scm.CLA1580_StructureHive,
                CLA2472_EMRSend = scm.CLA2472_EMRSend,
                CLA2429_SnowflakeCreateTable = scm.CLA2429_SnowflakeCreateTable,
                CLA1286_KafkaFlag = scm.CLA1286_KafkaFlag
            };

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaId">Schema Id</param>
        /// <param name="revisionId">Revision Id</param>
        /// <param name="initiatorId">Who initiatied change</param>
        /// <param name="changeIndicator">What schema change initiatied event creation</param>
        /// <param name="externalSystemIndictator">What consumption layer is this for: Hive or Snowflake</param>
        /// <returns></returns>
        public bool SasAddOrUpdateNotification(int schemaId, int revisionId, string initiatorId, JObject changeIndicator, string externalSystemIndictator)
        {
            SchemaRevision rev = null;
            try
            {
                rev = _datasetContext.SchemaRevision.Where(w => w.SchemaRevision_Id == revisionId && w.ParentSchema.SchemaId == schemaId).FirstOrDefault();

                //Use incoming initiator id.  If invalid or not supplied, use CreatedBy id on revision.
                IApplicationUser user = !string.IsNullOrWhiteSpace(initiatorId) ? _userService.GetByAssociateId(initiatorId) : _userService.GetByAssociateId(rev.CreatedBy);

                //Determine if IsInSAS property changed
                bool isInSAS = (changeIndicator.ContainsKey("isinsas") && changeIndicator.GetValue("isinsas").ToString().ToLower() == "true");
                //Determine if CurrentView property changed
                bool currentView = (changeIndicator.ContainsKey("createcurrentview") && changeIndicator.GetValue("createcurrentview").ToString().ToLower() == "true");

                string sasNotificationType = "UPDATE";
                string currentViewNotficationType = string.Empty;
                if (isInSAS)
                {
                    sasNotificationType = "ADD";
                }
                else if (rev.ParentSchema.IsInSAS && currentView)
                {
                    sasNotificationType = "ADD";
                    currentViewNotficationType = "ADD";
                }
                else if (rev.ParentSchema.IsInSAS && changeIndicator.ContainsKey("cla2429_snowflakecreatetable") && changeIndicator.GetValue("cla2429_snowflakecreatetable").ToString().ToLower() == "true" )
                {
                    sasNotificationType = "ADD";
                    if (rev.ParentSchema.CreateCurrentView)
                    {
                        currentViewNotficationType = "ADD";
                    }
                }
                else if (rev.ParentSchema.IsInSAS && changeIndicator.ContainsKey("revision") && changeIndicator.GetValue("revision").ToString().ToLower() == "added")
                {
                    sasNotificationType = "UPDATE";
                }

                //string sasNotificationType = (
                //    //changeIndicator.ToLower().Contains("revision:add") ||                                   /* triggered by updated to schema columns */
                //    isInSAS ||                                   /* triggered by schema configuration change to isinsas property */
                //    currentView && rev.ParentSchema.IsInSAS)     /* triggered by schema configuration change to currentview property*/
                //    ? "ADD" 
                //    : "UPDATE";

                
                SasNotification(rev.ParentSchema, sasNotificationType, currentViewNotficationType, user, externalSystemIndictator);

                return true;
            }
            catch (Exception ex)
            {
                int revId = (rev != null) ? rev.SchemaRevision_Id : 0;
                Logger.Error($"Failed sending SAS notification - revision:{revId}", ex);

                return false;
            }
        }

        public bool SasDeleteNotification(int schemaId, string initiatorId, string externalSystemIndictator)
        {
            FileSchema schema = null;
            IApplicationUser user;

            try
            {
                
                schema = _datasetContext.FileSchema.FirstOrDefault(w => w.SchemaId == schemaId);

                //Use incoming initiator id.  If invalid or not supplied, use PrimaryContactId id on Parent dataset.
                if (!string.IsNullOrWhiteSpace(initiatorId))
                {
                    user = _userService.GetByAssociateId(initiatorId);
                }
                else
                {
                    var contact = _datasetContext.DatasetFileConfigs.Where(w => w.Schema.SchemaId == schemaId).Select(s => s.ParentDataset.PrimaryContactId).FirstOrDefault();
                    user = _userService.GetByAssociateId(contact);
                }

                SasNotification(schema, "REMOVE", null, user, externalSystemIndictator);

                return true;
            }
            catch (Exception ex)
            {
                int scmId = (schema != null) ? schema.SchemaId : 0;
                Logger.Error($"<sasdeletenotification> Failed sending SAS delete notification - schemaId:{schemaId}", ex);

                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="sasNotificationType"></param>
        /// <param name="currentViewNotificationType"></param>
        /// <param name="changeInitiator"></param>
        /// <param name="externalSystemIndicator"></param>
        /// <exception cref="Sentry.data.Core.Exceptions.SASNotificationNotSentException">Notification was not sent successfully to SAS</exception>
        private void SasNotification(FileSchema schema, string sasNotificationType, string currentViewNotificationType, IApplicationUser changeInitiator, string externalSystemIndicator)
        {
            MethodBase m = MethodBase.GetCurrentMethod();

            StringBuilder bodySb = new StringBuilder();
            try
            {
                string subject = null;
                IApplicationUser user = changeInitiator;
                //Ensure properties are initialized
                sasNotificationType = (sasNotificationType == null) ? string.Empty : sasNotificationType;
                currentViewNotificationType = (currentViewNotificationType == null) ? string.Empty : currentViewNotificationType;
                subject = GenerateSchemaSyncNotification(schema, externalSystemIndicator, sasNotificationType, currentViewNotificationType, bodySb, subject, user);

                string ccEmailList = Configuration.Config.GetHostSetting("EmailDSCSupportAsCC") == "true" ? $"{user.EmailAddress};DSCSupport@sentry.com" : $"{user.EmailAddress}";

                if (bodySb.Length > 0)
                {
                    bodySb.Append($"<p>Thank you from your friendly data.sentry.com Administration team</p>");

                    Logger.Info($"<{m.ReflectedType.Name.ToLower()}> Sending email to {Configuration.Config.GetHostSetting("SASAdministrationEmail")} and including CCs ({ccEmailList})");
                    _emailService.SendGenericEmail(Configuration.Config.GetHostSetting("SASAdministrationEmail"), subject, bodySb.ToString(), ccEmailList);
                    Logger.Info($"<{m.ReflectedType.Name.ToLower()}> Email sent");
                }
                else
                {
                    Logger.Warn($"SAS Notification was not configured");
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"schema-name:{schema.Name}");
                sb.Append($"|schema-id:{schema.SchemaId}");
                sb.Append($"|notification-type:{sasNotificationType}");
                sb.Append($"|current-view-notification-type:{currentViewNotificationType}");
                sb.Append($"|change-initiator:{changeInitiator.AssociateId}");
                sb.Append($"|external-system-indicator:{externalSystemIndicator}");
                sb.Append($"|notification-body:{bodySb.ToString()}");

                throw new SASNotificationNotSentException($"Failed sending SAS Notification: {sb.ToString()}", ex);
            }            
        }

        private static string GenerateSchemaSyncNotification(FileSchema schema, string systemIndicator, string sasNotificationType, string currentViewNotificationType, StringBuilder bodySb, string subject, IApplicationUser user)
        {
            string libraryName = schema.SasLibrary;
            string viewName;

            if (systemIndicator.ToUpper() == "SNOWFLAKE")
            {
                libraryName += "_SNFC";
                viewName = $"{schema.SnowflakeDatabase}.{schema.SnowflakeSchema}.vw_{schema.SnowflakeTable}";
            }
            else if (systemIndicator.ToUpper() == "HIVE")
            {
                viewName = $"vw_{schema.HiveTable}";
            }
            else
            {
                throw new Exception();
            }


            switch (sasNotificationType.ToUpper())
            {
                //Addition of all schema views to SAS0
                case "ADD":
                    subject = $"Library Add Request to {libraryName}";
                    bodySb.AppendLine($"<p>{user.DisplayName} has requested the following to be <strong style=\"color:#008000;\">ADDED</strong> to {libraryName}:</p>");
                    bodySb.AppendLine($"<p>- {viewName}</p>");
                    //Include current view if checked
                    if (systemIndicator != "SNOWFLAKE" && (currentViewNotificationType == "ADD" || schema.CreateCurrentView))
                    {
                        bodySb.AppendLine($"<p>- {viewName}_cur</p>");
                    }
                    Logger.Debug($"Configuring SAS Notification to ADD all view(s)  {bodySb.ToString()}");
                    break;
                //Removal of all schema views from SAS
                case "REMOVE":
                    subject = $"Library Remove Request from {libraryName}";
                    bodySb.AppendLine($"<p>{user.DisplayName} has requested the following to be <strong style=\"color:#FF0000;\">REMOVED</strong> from {libraryName}:</p>");
                    bodySb.AppendLine($"<p>- {viewName}</p>");
                    //if current view is being updated to unchecked or is currently checked, ensure it is removed from SAS
                    if (systemIndicator != "SNOWFLAKE" && (currentViewNotificationType.ToUpper() == "REMOVE" || schema.CreateCurrentView))
                    {
                        bodySb.AppendLine($"<p>- {viewName}_cur</p>");
                    }
                    Logger.Debug($"Configuring SAS Notification to REMOVE all view(s)  {bodySb.ToString()}");
                    break;
                //Update of all SAS libraries
                case "UPDATE":
                    subject = $"Library Refresh Request from {libraryName}";
                    bodySb.AppendLine($"<p>{user.DisplayName} has requested the following to be updated in {libraryName}:</p>");
                    bodySb.AppendLine($"<p>- {viewName}</p>");
                    if (systemIndicator != "SNOWFLAKE" && schema.CreateCurrentView)
                    {
                        bodySb.AppendLine($"<p>- {viewName}_cur</p>");
                    }
                    Logger.Debug($"Configuring SAS Notification to UDPATE all view(s)  {bodySb.ToString()}");
                    break;
                //Current View propery can be changed independently of IsInSAS property
                //  Ensure notification is sent for current view propery changes if IsInSAS is checked
                default:
                    if (schema.IsInSAS && currentViewNotificationType.ToUpper() == "ADD")
                    {
                        Logger.Debug($"Configuring SAS Notification to ADD current view");
                        subject = $"Library Add Request from {libraryName}";
                        bodySb.AppendLine($"<p>{user.DisplayName} has requested the following to be <strong style=\"color:#008000;\">ADDED</strong> to {libraryName}:</p>");
                        bodySb.AppendLine($"<p>- {viewName}_cur</p>");
                        Logger.Debug($"Configuring SAS Notification to ADD current view  {bodySb.ToString()}");
                    }
                    else if (schema.IsInSAS && currentViewNotificationType.ToUpper() == "REMOVE")
                    {
                        Logger.Debug($"Configuring SAS Notification to REMOVE current view");
                        subject = $"Library Remove Request from {libraryName}";
                        bodySb.AppendLine($"<p>{user.DisplayName} has requested the following to be <strong style=\"color:#FF0000;\">REMOVED</strong> from {libraryName}:</p>");
                        bodySb.AppendLine($"<p>- {viewName}_cur</p>");
                        Logger.Debug($"Configuring SAS Notification to REMOVE current view  {bodySb.ToString()}");
                    }
                    break;
            }

            return subject;
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

        private string GenerateSnowflakeDatabaseName()
        {
            string curEnv = Config.GetDefaultEnvironmentName().ToUpper();
            string dbName = "DATA_" + curEnv;
            return dbName;
        }

        private string GenerateSnowflakeSchema(Category cat)
        {
            return cat.Name.ToUpper();
        }

        private string FormateSnowflakeTableNamePart(string part)
        {
            return part.Replace(" ", "").Replace("_", "").Replace("-", "").ToUpper();
        }

        private BaseField AddRevisionField(BaseFieldDto row, SchemaRevision CurrentRevision, BaseField parentRow = null, SchemaRevision previousRevision = null)
        {
            BaseField newField = null;
            //Should we perform comparison to previous based on is incoming field new
            bool compare = (row.FieldGuid.ToString() != Guid.Empty.ToString() && previousRevision != null);

            //if comparing, pull field from previous version
            BaseFieldDto previousFieldDtoVersion = (compare) ? previousRevision.Fields.FirstOrDefault(w => w.FieldGuid == row.FieldGuid).ToDto() : null;
            
            bool changed = false;
            newField = row.ToEntity(parentRow, CurrentRevision);

            if (!compare)
            {
                newField.CreateDTM = CurrentRevision.CreatedDTM;
                newField.LastUpdateDTM = CurrentRevision.CreatedDTM;
            }
            else
            {
                changed = previousFieldDtoVersion.CompareToEntity(newField);
                newField.LastUpdateDTM = (changed) ? CurrentRevision.LastUpdatedDTM : previousFieldDtoVersion.LastUpdatedDtm;
            }

            _datasetContext.Add(newField);

            //if there are child rows, perform a recursive call to this function
            if (newField != null && row.ChildFields != null)
            {
                foreach (BaseFieldDto cRow in row.ChildFields)
                {
                    newField.ChildFields.Add(AddRevisionField(cRow, CurrentRevision, newField, previousRevision));
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

        public void Validate(int schemaId, List<BaseFieldDto> fieldDtoList)
        {
            FileSchema schema = _datasetContext.GetById<FileSchema>(schemaId);
            ValidationResults errors = new ValidationResults();

            errors.MergeInResults(Validate(schema, fieldDtoList));

            if (!errors.IsValid())
            {
                throw new ValidationException(errors);
            }
        }

        private ValidationResults Validate(FileSchema scm, List<BaseFieldDto> fieldDtoList)
        {
            ValidationResults results = new ValidationResults();
            foreach(BaseFieldDto fieldDto in fieldDtoList)
            {
                //Field name cannot be blank
                if (string.IsNullOrWhiteSpace(fieldDto.Name))
                {
                    results.Add(fieldDto.OrdinalPosition.ToString(), $"({fieldDto.Name}) Name cannot be empty string");
                }

                //Field name cannot contain spaces
                if (fieldDto.Name != null && fieldDto.Name.Any(char.IsWhiteSpace))
                {
                    results.Add(fieldDto.OrdinalPosition.ToString(), $"({fieldDto.Name}) Name cannot contain spaces");
                }

                //Field name cannot contain special characters
                string specialCharacters = @"-:,;{}()";
                string specialCharPattern = $"[{specialCharacters}]";
                if (fieldDto.Name != null && Regex.IsMatch(fieldDto.Name, specialCharPattern))
                {
                    results.Add(fieldDto.OrdinalPosition.ToString(), $"({fieldDto.Name}) Name cannot contain special characters ({specialCharacters})");
                }

                //Field name cannot contain uppercase letters
                if (fieldDto.Name != null && fieldDto.Name.Any(char.IsUpper))
                {
                    results.Add(fieldDto.OrdinalPosition.ToString(), $"({fieldDto.Name}) Name cannot contain uppercase letters");
                }

                //Struct has children
                if (fieldDto.FieldType == GlobalConstants.Datatypes.STRUCT && !fieldDto.HasChildren)
                {
                    results.Add(fieldDto.OrdinalPosition.ToString(), $"({fieldDto.Name}) STRUCTs are required to have children");
                }

                //Varchar Length
                if (fieldDto.FieldType == GlobalConstants.Datatypes.VARCHAR && (fieldDto.Length < 1 || fieldDto.Length > 65535))
                {
                    results.Add(fieldDto.OrdinalPosition.ToString(), $"({fieldDto.Name}) VARCHAR length ({fieldDto.Length}) is required to be between 1 and 65535");
                }

                //Decimal Precision
                if (fieldDto.FieldType == GlobalConstants.Datatypes.DECIMAL && (fieldDto.Precision < 1 || fieldDto.Precision > 38))
                {
                    results.Add(fieldDto.OrdinalPosition.ToString(), $"({fieldDto.Name}) Precision ({fieldDto.Precision}) is required to be between 1 and 38");
                }

                //Decimal Scale
                if (fieldDto.FieldType == GlobalConstants.Datatypes.DECIMAL && (fieldDto.Scale < 1 || fieldDto.Scale > 38))
                {
                    results.Add(fieldDto.OrdinalPosition.ToString(), $"({fieldDto.Name}) Scale ({fieldDto.Scale}) is required to be between 1 and 38");
                }

                //Decimal Scale and Precision dependency
                if (fieldDto.FieldType == GlobalConstants.Datatypes.DECIMAL && fieldDto.Scale > fieldDto.Precision)
                {
                    results.Add(fieldDto.OrdinalPosition.ToString(), $"({fieldDto.Name}) Scale ({fieldDto.Scale}) needs to be less than or equal to Precision ({fieldDto.Precision})");
                }

                results.MergeInResults(ValidateFieldtoFileSchema(scm, fieldDto));

                if (fieldDto.ChildFields.Any())
                {
                    results.MergeInResults(Validate(scm, fieldDto.ChildFields));
                }
            }
            return results;
        }

        private ValidationResults ValidateFieldtoFileSchema(FileSchema scm, BaseFieldDto fieldDto)
        {
            ValidationResults results = new ValidationResults();
            string extension = scm.Extension.Name;
            if (extension == "FIXEDWIDTH" && fieldDto.Length == 0)
            {
                results.Add(fieldDto.OrdinalPosition.ToString(), $"({fieldDto.Name}) Length ({fieldDto.Length}) needs to be greater than zero for FIXEDWIDTH schema");
            }

            if (extension == "FIXEDWIDTH" && fieldDto.FieldType == GlobalConstants.Datatypes.DECIMAL && fieldDto.Length != 0 && fieldDto.Length < fieldDto.Precision)
            {
                results.Add(fieldDto.OrdinalPosition.ToString(), $"({fieldDto.Name}) Length ({fieldDto.Length}) needs to be equal or greater than specified precision for FIXEDWIDTH schema");
            }

            if (extension == "FIXEDWIDTH" && (fieldDto.FieldType == GlobalConstants.Datatypes.TIMESTAMP || fieldDto.FieldType == GlobalConstants.Datatypes.DATE) && fieldDto.SourceFormat != null && fieldDto.Length < fieldDto.SourceFormat.Length)
            {
                results.Add(fieldDto.OrdinalPosition.ToString(), $"({fieldDto.Name}) Length ({fieldDto.Length}) needs to be equal or greater than specified format for FIXEDWIDTH schema");
            }

            if (extension == "FIXEDWIDTH" && (fieldDto.FieldType == GlobalConstants.Datatypes.TIMESTAMP) && fieldDto.SourceFormat == null && fieldDto.Length < GlobalConstants.Datatypes.Defaults.TIMESTAMP_DEFAULT.Length)
            {
                results.Add(fieldDto.OrdinalPosition.ToString(), $"({fieldDto.Name}) Length ({fieldDto.Length}) needs to be equal or greater than default format length ({GlobalConstants.Datatypes.Defaults.TIMESTAMP_DEFAULT.Length}) for FIXEDWIDTH schema");
            }

            if (extension == "FIXEDWIDTH" && (fieldDto.FieldType == GlobalConstants.Datatypes.DATE) && fieldDto.SourceFormat == null && fieldDto.Length < GlobalConstants.Datatypes.Defaults.DATE_DEFAULT.Length)
            {
                results.Add(fieldDto.OrdinalPosition.ToString(), $"({fieldDto.Name}) Length ({fieldDto.Length}) needs to be equal or greater than default format length ({GlobalConstants.Datatypes.Defaults.DATE_DEFAULT.Length}) for FIXEDWIDTH schema");
            }

            if (extension == "FIXEDWIDTH" && fieldDto.FieldType == GlobalConstants.Datatypes.VARCHAR && fieldDto.Length == 0)
            {
                results.Add(fieldDto.OrdinalPosition.ToString(), $"({fieldDto.Name}) Length ({fieldDto.Length}) needs to be equal or greater than specified precision for FIXEDWIDTH schema");
            }

            return results;
        }
    }
}