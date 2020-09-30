using Sentry.Common.Logging;
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
        private string _bucket;

        public SchemaService(IDatasetContext dsContext, IUserService userService, IEmailService emailService,
            IDataFlowService dataFlowService, IJobService jobService, ISecurityService securityService,
            IDataFeatures dataFeatures, IMessagePublisher messagePublisher)
        {
            _datasetContext = dsContext;
            _userService = userService;
            _emailService = emailService;
            _dataFlowService = dataFlowService;
            _jobService = jobService;
            _securityService = securityService;
            _featureFlags = dataFeatures;
            _messagePublisher = messagePublisher;
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

            try
            {
                UserSecurity us = _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());
                if (!(us.CanEditDataset))
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

                    //Send message to create hive table
                    HiveTableCreateModel hiveCreate = new HiveTableCreateModel()
                    {
                        SchemaID = revision.ParentSchema.SchemaId,
                        RevisionID = revision.SchemaRevision_Id,
                        DatasetID = ds.DatasetId,
                        HiveStatus = null,
                        InitiatorID = _userService.GetCurrentUser().AssociateId
                    };
                    _messagePublisher.PublishDSCEvent(schemaId.ToString(), JsonConvert.SerializeObject(hiveCreate));

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
            
            var SendSASNotification = false;
            string SASNotificationType = null;
            string CurrentViewNotificationType = null;
            try
            {
                FileSchema schema = _datasetContext.GetById<FileSchema>(schemaDto.SchemaId);
                var SchemaRevisionExists = _datasetContext.SchemaRevision.Where(w => w.ParentSchema == schema).Any();

                #region SAS Notification Determination Logic
                //      This logic needs to be determine prior to mapping DTO to schema so change detection occurs properly
                //      Notification logic occurs after changes successfully saved to database

                /*
                 * Detect change within IsInSAS property when
                 *      Schema Revision exists
                 * if change,
                 *      set notification trigger to true
                 *      set type of notification
                 */
                if (SchemaRevisionExists && schema.IsInSAS != schemaDto.IsInSas)
                {
                    SendSASNotification = true;
                    SASNotificationType = (schemaDto.IsInSas) ? "ADD" : "REMOVE";
                }

                /*
                 * Determine change within CurrentView property when 
                 *      Schema Revision exists
                 *      IsInSAS is true or when IsInSAS has changed to false
                 * if change,
                 *      set notification trigger to true
                 *      set type of notification
                 */
                if (SchemaRevisionExists && (schemaDto.IsInSas || (SASNotificationType != null && SASNotificationType.ToUpper() == "REMOVE")) && schema.CreateCurrentView != schemaDto.CreateCurrentView)
                {
                    SendSASNotification = true;
                    CurrentViewNotificationType = (schemaDto.CreateCurrentView) ? "ADD" : "REMOVE";
                }
                #endregion

                UpdateAndSaveSchema(schemaDto, schema);
                _datasetContext.SaveChanges();

                //Send notification to SAS
                if (SendSASNotification)
                {
                    SasNotification(schema, SASNotificationType, CurrentViewNotificationType, _userService.GetCurrentUser());
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("schemaservice-updateandsaveschema", ex);
                return false;
            }
        }

        private void UpdateAndSaveSchema(FileSchemaDto dto, FileSchema schema)
        {
            bool chgDetected = false;
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

            if (chgDetected)
            {
                schema.LastUpdatedDTM = DateTime.Now;
                schema.UpdatedBy = _userService.GetCurrentUser().AssociateId;
            } 
            
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
                if (!(us.CanPreviewDataset || us.CanViewFullDataset || us.CanUploadToDataset || us.CanEditDataset))
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
                if (!(us.CanPreviewDataset || us.CanViewFullDataset || us.CanUploadToDataset || us.CanEditDataset))
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

            return revision.ToDto();
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
                HiveTableStatus = HiveTableStatusEnum.NameReserved.ToString(),
                HiveLocation = RootBucket + "/" + GlobalConstants.ConvertedFileStoragePrefix.PARQUET_STORAGE_PREFIX + "/" + Configuration.Config.GetHostSetting("S3DataPrefix") + storageCode,
                CreatedDTM = DateTime.Now,
                LastUpdatedDTM = DateTime.Now,
                DeleteIssueDTM = DateTime.MaxValue,
                CreateCurrentView = dto.CreateCurrentView,
                CLA1396_NewEtlColumns = dto.CLA1396_NewEtlColumns,
                CLA1580_StructureHive = dto.CLA1580_StructureHive
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
                CLA1396_NewEtlColumns = scm.CLA1396_NewEtlColumns,
                CLA1580_StructureHive = scm.CLA1580_StructureHive
            };

        }

        public bool SasUpdateNotification(int schemaId, int revisionId, string initiatorId)
        {
            SchemaRevision rev = null;
            IApplicationUser user;
            try
            {
                rev = _datasetContext.SchemaRevision.Where(w => w.SchemaRevision_Id == revisionId && w.ParentSchema.SchemaId == schemaId).FirstOrDefault();
                bool fieldChanges = rev.Fields.Where(w => w.LastUpdateDTM == rev.LastUpdatedDTM).Any();
                
                //Use incoming initiator id.  If invalid or not supplied, use CreatedBy id on revision.
                if (!string.IsNullOrWhiteSpace(initiatorId))
                {
                    user = _userService.GetByAssociateId(initiatorId);
                }
                else
                {
                    user = _userService.GetByAssociateId(rev.CreatedBy);
                }
                
                if (fieldChanges && rev.Revision_NBR == 1)
                {
                    SasNotification(rev.ParentSchema, "ADD", null, user);
                }
                else if (fieldChanges)
                {
                    SasNotification(rev.ParentSchema, "UPDATE", null, user);
                }

                return true;
            }
            catch (Exception ex)
            {
                int revId = (rev != null) ? rev.SchemaRevision_Id : 0;
                Logger.Error($"Failed sending SAS email - revision:{revId}", ex);

                return false;
            }
        }


        private void SasNotification(FileSchema schema, string sasNotificationType, string currentViewNotificationType, IApplicationUser changeInitiator)
        {
            StringBuilder bodySb = new StringBuilder();
            string subject = null;
            IApplicationUser user = changeInitiator;
            //Ensure properties are initialized
            sasNotificationType = (sasNotificationType == null) ? string.Empty : sasNotificationType;
            currentViewNotificationType = (currentViewNotificationType == null) ? string.Empty : currentViewNotificationType;

            switch (sasNotificationType.ToUpper())
            {
                //Addition of all schema views to SAS
                case "ADD":
                    Logger.Debug($"Configuring SAS Notification to ADD all view(s)");
                    subject = $"Library Add Request to {schema.SasLibrary}";
                    bodySb.AppendLine($"<p>{user.DisplayName} has requested the following to be added to {schema.SasLibrary}:</p>");
                    bodySb.AppendLine($"<p>- vw_{schema.HiveTable}</p>");
                    //Include current view if checked
                    if (currentViewNotificationType == "ADD" || schema.CreateCurrentView)
                    {
                        bodySb.AppendLine($"<p>- vw_{schema.HiveTable}_cur</p>");
                    }
                    break;
                //Removal of all schema views from SAS
                case "REMOVE":
                    Logger.Debug($"Configuring SAS Notification to REMOVE all view(s)");
                    subject = $"Library Remove Request from {schema.SasLibrary}";
                    bodySb.AppendLine($"<p>{user.DisplayName} has requested the following to be removed from {schema.SasLibrary}:</p>");
                    bodySb.AppendLine($"<p>- vw_{schema.HiveTable}</p>");
                    //if current view is being updated to unchecked or is currently checked, ensure it is removed from SAS
                    if (currentViewNotificationType.ToUpper() == "REMOVE" || schema.CreateCurrentView)
                    {
                        bodySb.AppendLine($"<p>- vw_{schema.HiveTable}_cur</p>");
                    }
                    break;
                //Update of all SAS libraries
                case "UPDATE":
                    Logger.Debug($"Configuring SAS Notification to UDPATE all view(s)");
                    subject = $"Library Refresh Request from {schema.SasLibrary}";
                    bodySb.AppendLine($"<p>{user.DisplayName} has requested the following to be updated in {schema.SasLibrary}:</p>");
                    bodySb.AppendLine($"<p>- vw_{schema.HiveTable}</p>");
                    if (schema.CreateCurrentView)
                    {
                        bodySb.AppendLine($"<p>- vw_{schema.HiveTable}_cur</p>");
                    }
                    break;
                //Current View propery can be changed independently of IsInSAS property
                //  Ensure notification is sent for current view propery changes if IsInSAS is checked
                default:
                    if (schema.IsInSAS && currentViewNotificationType.ToUpper() == "ADD")
                    {
                        Logger.Debug($"Configuring SAS Notification to ADD current view");
                        subject = $"Library Add Request from {schema.SasLibrary}";
                        bodySb.AppendLine($"<p>{user.DisplayName} has requested the following to be added to {schema.SasLibrary}:</p>");
                        bodySb.AppendLine($"<p>- vw_{schema.HiveTable}_cur</p>");
                    }
                    else if (schema.IsInSAS && currentViewNotificationType.ToUpper() == "REMOVE")
                    {
                        Logger.Debug($"Configuring SAS Notification to REMOVE current view");
                        subject = $"Library Remove Request from {schema.SasLibrary}";
                        bodySb.AppendLine($"<p>{user.DisplayName} has requested the following to be removed from {schema.SasLibrary}:</p>");
                        bodySb.AppendLine($"<p>- vw_{schema.HiveTable}_cur</p>");
                    }
                    break;
            }

            string ccEmailList = Configuration.Config.GetHostSetting("EmailDSCSupportAsCC") == "true" ? $"{user.EmailAddress};DSCSupport@sentry.com" : $"{user.EmailAddress}";

            if (bodySb.Length > 0)
            {
                bodySb.Append($"<p>Thank you from your friendly data.sentry.com Administration team</p>");

                _emailService.SendGenericEmail(Configuration.Config.GetHostSetting("SASAdministrationEmail"), subject, bodySb.ToString(), ccEmailList);

            }
            else
            {
                Logger.Warn($"SAS Notification was not configured");
            }
        }

        private DataFlowDto MapToDataFlowDto(FileSchema scm)
        {
            DataFlowDto dto = new DataFlowDto()
            {
                CreatedBy = _userService.GetCurrentUser().AssociateId,
                CreateDTM = DateTime.Now,
                Name = $"SchemaFlow_{scm.StorageCode}",
                IngestionType = GlobalEnums.IngestionType.User_Push
            };

            SchemaMapDto scmDto = new SchemaMapDto()
            {
                SchemaId = scm.SchemaId,
                SearchCriteria = "\\.",
            };

            return dto;
        }

        private string FormatHiveTableNamePart(string part)
        {
            return part.Replace(" ", "").Replace("_", "").Replace("-", "").ToUpper();
        }

        private string GenerateHiveDatabaseName(Category cat)
        {            
            return "dsc_" + cat.Name.ToLower();
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
