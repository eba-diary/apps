﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sentry.Common.Logging;
using StructureMap;

namespace Sentry.data.Core
{
    public class ConfigService : IConfigService
    {
        public IDatasetContext _datasetContext;
        public IMessagePublisher _publisher;
        public IUserService _userService;
        public IEventService _eventService;
        public IMessagePublisher _messagePublisher;
        public IEncryptionService _encryptService;
        public IJobService _jobService;
        public IEmailService _emailService;
        private IS3ServiceProvider s3ServiceProvider;
        private readonly ISecurityService _securityService;

        public ConfigService(IDatasetContext dsCtxt, IMessagePublisher publisher, 
            IUserService userService, IEventService eventService, IMessagePublisher messagePublisher,
            IEncryptionService encryptService, ISecurityService securityService,
            IJobService jobService, IEmailService emailService, IS3ServiceProvider s3ServiceProvider)
        {
            _datasetContext = dsCtxt;
            _publisher = publisher;
            _userService = userService;
            _eventService = eventService;
            _messagePublisher = messagePublisher;
            _encryptService = encryptService;
            _securityService = securityService;
            JobService = jobService;
            _emailService = emailService;
            S3ServiceProvider = s3ServiceProvider;
        }

        private IJobService JobService
        {
            get
            {
                return _jobService;
            }
            set
            {
                _jobService = value;
            }
        }

        private IS3ServiceProvider S3ServiceProvider { get; set; }

        public SchemaApiDTO GetSchemaApiDTO(int id)
        {
            DataElement de = _datasetContext.GetById<DataElement>(id);
            SchemaApiDTO dto = new SchemaApiDTO();
            MapToDto(de, dto);
            return dto;
        }

        public SchemaDetaiApilDTO GetSchemaDetailDTO(int id)
        {
            DataElement de = _datasetContext.GetById<DataElement>(id);
            SchemaDetaiApilDTO dto = new SchemaDetaiApilDTO();
            MaptToDetailDto(de, dto);
            return dto;
        }

        public SchemaDto GetSchemaDto(int id)
        {
            Schema s = _datasetContext.GetById<Schema>(id);            
            return s.MapToSchemaDto();
        }

        public IList<ColumnDTO> GetColumnDTO(int id)
        {
            return MapToDto(_datasetContext.GetById<DataElement>(id).DataObjects);
        }

        public List<string> Validate(DataElementDto dto)
        {
            List<string> errors = new List<string>();

            var currentFileExtension = _datasetContext.FileExtensions.FirstOrDefault(x => x.Id == dto.FileFormatId).Name.ToLower();

            if (currentFileExtension == "csv" && dto.Delimiter != ",")
            {
                errors.Add("File Extension CSV and it's delimiter do not match.");
            }

            if (currentFileExtension == "delimited" && string.IsNullOrWhiteSpace(dto.Delimiter))
            {
                errors.Add("File Extension Delimited is missing it's delimiter.");
            }

            return errors;
        }

        public List<string> Validate(DataSourceDto dto)
        {
            List<string> errors = new List<string>();

            AuthenticationType auth = _datasetContext.GetById<AuthenticationType>(Convert.ToInt32(dto.AuthID));

            switch (dto.SourceType)
            {
                case "DFSCustom":
                    if (dto.OriginatingId == 0 && _datasetContext.DataSources.Where(w => w is DfsCustom && w.Name == dto.Name).Count() > 0)
                    {
                        errors.Add("An DFS Custom Data Source is already exists with this name.");
                    }
                    break;
                case "FTP":
                    if (dto.OriginatingId == 0 && _datasetContext.DataSources.Where(w => w is FtpSource && w.Name == dto.Name).Count() > 0)
                    {
                        errors.Add("An FTP Data Source is already exists with this name.");
                    }
                    if (!(dto.BaseUri.ToString().StartsWith("ftp://")))
                    {
                        errors.Add("A valid FTP URI starts with ftp:// (i.e. ftp://foo.bar.com/base/dir)");
                    }
                    break;
                case "SFTP":
                    if (dto.OriginatingId == 0 && _datasetContext.DataSources.Where(w => w is SFtpSource && w.Name == dto.Name).Count() > 0)
                    {
                        errors.Add("An SFTP Data Source is already exists with this name.");
                    }
                    if (!(dto.BaseUri.ToString().StartsWith("sftp://")))
                    {
                        errors.Add("A valid SFTP URI starts with sftp:// (i.e. sftp://foo.bar.com//base/dir/)");
                    }
                    break;
                case "HTTPS":
                    if (dto.OriginatingId == 0 && _datasetContext.DataSources.Where(w => w is HTTPSSource && w.Name == dto.Name).Count() > 0)
                    {
                        errors.Add("An HTTPS Data Source is already exists with this name.");
                    }
                    if (!(dto.BaseUri.ToString().StartsWith("https://")))
                    {
                        errors.Add("A valid HTTPS URI starts with https:// (i.e. https://foo.bar.com/base/api/)");
                    }

                    //if token authentication, user must enter values for token header and value
                    if (auth.Is<TokenAuthentication>())
                    {
                        if (String.IsNullOrWhiteSpace(dto.TokenAuthHeader))
                        {
                            errors.Add("Token Authenticaion requires a token header");
                        }

                        if (String.IsNullOrWhiteSpace(dto.TokenAuthValue))
                        {
                            errors.Add("Token Authentication requires a token header value");
                        }
                    }

                    foreach (RequestHeader h in dto.RequestHeaders)
                    {
                        if (String.IsNullOrWhiteSpace(h.Key) || String.IsNullOrWhiteSpace(h.Value))
                        {
                            errors.Add("Request headers need to contain valid values");
                        }
                    }
                    break;
                case "GOOGLEAPI":
                    if (!(dto.BaseUri.ToString().StartsWith("https://")))
                    {
                        errors.Add("A valid GoogleApi URI starts with https:// (i.e. https://analyticsreporting.googleapis.com/)");
                    }
                    if (!(dto.BaseUri.ToString().Contains("googleapis.com")))
                    {
                        errors.Add("An invalid GoogleApi URI");
                    }
                    break;
                case "DFSBasic":
                default:
                    throw new NotImplementedException();
            }

            if (String.IsNullOrWhiteSpace(dto.PrimaryOwnerId))
            {
                errors.Add("Owner is requried.");
            }

            if (String.IsNullOrWhiteSpace(dto.PrimaryContactId))
            {
                errors.Add("Contact is requried.");
            }

            return errors;
        }

        public List<string> Validate(DatasetFileConfigDto dto)
        {
            List<string> errors = new List<string>();

            Dataset parent = _datasetContext.GetById<Dataset>(dto.ParentDatasetId);

            //remove any schemas which are marked for deletion
            if (parent.DatasetFileConfigs.Any(x => !x.DeleteInd && x.Name.ToLower() == dto.Name.ToLower()))
            {
                errors.Add("Dataset config with that name already exists within dataset");
            }

            return errors;
        }

        public bool CreateAndSaveNewDataSource(DataSourceDto dto)
        {
            try
            {
                DataSource ds = CreateDataSource(dto);
                _datasetContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating data source", ex);
                return false;
            }           

            return true;
        }

        public bool UpdateAndSaveDataSource(DataSourceDto dto)
        {
            try
            {
                DataSource dsrc = _datasetContext.GetById<DataSource>(dto.OriginatingId);
                UpdateDataSource(dto, dsrc);
                _datasetContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("datasource_save_error", ex);
                return false;
            }
        }

        public bool CreateAndSaveDatasetFileConfig(DatasetFileConfigDto dto)
        {
            try
            {
                DatasetFileConfig dfc = CreateDatasetFileConfig(dto);

                DataSource basicSource = _datasetContext.DataSources.First(x => x.Name.Contains(GlobalConstants.DataSourceName.DEFAULT_DROP_LOCATION));

                RetrieverJob rj = JobService.InstantiateJobsForCreation(dfc, basicSource);

                List<RetrieverJob> jobList = new List<RetrieverJob>
                {
                    rj,
                    JobService.InstantiateJobsForCreation(dfc, _datasetContext.DataSources.First(x => x.Name.Contains(GlobalConstants.DataSourceName.DEFAULT_S3_DROP_LOCATION)))
                };

                dfc.RetrieverJobs = jobList;

                Dataset parent = _datasetContext.GetById<Dataset>(dto.ParentDatasetId);
                List<DatasetFileConfig> dfcList = parent.DatasetFileConfigs.ToList();
                dfcList.Add(dfc);
                parent.DatasetFileConfigs = dfcList;

                _datasetContext.Merge(parent);
                _datasetContext.SaveChanges();


                JobService.CreateDropLocation(rj);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating Dataset File Config", ex);
                return false;
            }            
        }

        public bool UpdateAndSaveDatasetFileConfig(DatasetFileConfigDto dto)
        {
            try
            {
                DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(dto.ConfigId);
                UpdateDatasetFileConfig(dto, dfc);
                _datasetContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("datasetfileconfig_save_error", ex);
                return false;
            }
        }

        private void UpdateDatasetFileConfig(DatasetFileConfigDto dto, DatasetFileConfig dfc)
        {
            dfc.DatasetScopeType = _datasetContext.GetById<DatasetScopeType>(dto.DatasetScopeTypeId);
            dfc.FileTypeId = dto.FileTypeId;
            dfc.Description = dto.Description;
            dfc.FileExtension = _datasetContext.GetById<FileExtension>(dto.FileExtensionId);

            DataElement de = _datasetContext.DataElements.Where(w => w.DatasetFileConfig.ConfigId == dfc.ConfigId && w.DataElement_NME == dfc.Schemas.FirstOrDefault().DataElement_NME).FirstOrDefault();
            UpdateDataElement(dto, de);
        }

        private void UpdateDataElement(DatasetFileConfigDto dto, DataElement de)
        {
            
            de.CreateCurrentView = dto.Schemas.FirstOrDefault().CreateCurrentView;            
            de.SasLibrary = (dto.IsInSAS) ? dto.GenerateSASLibaryName(_datasetContext) : null;
            de.FileFormat = _datasetContext.GetById<FileExtension>(dto.FileExtensionId).Name.Trim();
            de.Delimiter = dto.Schemas.FirstOrDefault().Delimiter;
            de.HasHeader = dto.HasHeader;

            //if IsInSAS property changed to checked, send email communication.
            if (de.IsInSAS != dto.IsInSAS && dto.IsInSAS)
            {
                de.SendIncludeInSasEmail(_userService.GetCurrentUser(), _emailService);
            }
            de.IsInSAS = dto.IsInSAS;
        }

        public DatasetFileConfigDto GetDatasetFileConfigDto(int configId)
        {
            DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(configId);
            DatasetFileConfigDto dto = new DatasetFileConfigDto();
            MapToDatasetFileConfigDto(dfc, dto);
            return dto;
        }

        private DatasetFileConfig CreateDatasetFileConfig(DatasetFileConfigDto dto)
        {
            List<DataElement> deList = new List<DataElement>();
            DataElement de = CreateNewDataElement(dto.Schemas[0]);

            DatasetFileConfig dfc = new DatasetFileConfig()
            {
                Name = dto.Name,
                Description = dto.Description,
                FileTypeId = dto.FileTypeId,
                ParentDataset = _datasetContext.GetById<Dataset>(dto.ParentDatasetId),
                FileExtension = _datasetContext.GetById<FileExtension>(dto.FileExtensionId),
                DatasetScopeType = _datasetContext.GetById<DatasetScopeType>(dto.DatasetScopeTypeId),
                Schemas = deList
            };

            de.DatasetFileConfig = dfc;
            deList.Add(de);
            dfc.Schemas = deList;

            return dfc;
        }

        private DataElement CreateNewDataElement(DataElementDto dto)
        {
            Dataset ds = _datasetContext.GetById<Dataset>(dto.ParentDatasetId);
            string storageCode = _datasetContext.GetNextStorageCDE().ToString();

            DataElement de = new DataElement()
            {
                DataElementCreate_DTM = DateTime.Now,
                DataElementChange_DTM = DateTime.Now,
                DataElement_CDE = "F",
                DataElement_DSC = GlobalConstants.DataElementDescription.DATA_FILE,
                DataElement_NME = dto.DataElementName,
                LastUpdt_DTM = DateTime.Now,
                SchemaIsPrimary = true,
                SchemaDescription = dto.SchemaDescription,
                SchemaName = dto.SchemaName,
                SchemaRevision = 1,
                SchemaIsForceMatch = false,
                Delimiter = dto.Delimiter,
                HasHeader = dto.HasHeader,
                FileFormat = _datasetContext.GetById<FileExtension>(dto.FileExtensionId).Name.Trim(),
                StorageCode = storageCode,
                HiveDatabase = "Default",
                HiveTable = ds.DatasetName.Replace(" ", "").Replace("_", "").ToUpper() + "_" + dto.SchemaName.Replace(" ", "").ToUpper(),
                HiveTableStatus = HiveTableStatusEnum.NameReserved.ToString(),
                HiveLocation = Configuration.Config.GetHostSetting("AWSRootBucket") + "/" + GlobalConstants.ConvertedFileStoragePrefix.PARQUET_STORAGE_PREFIX + "/" + Configuration.Config.GetHostSetting("S3DataPrefix") + storageCode,
                CreateCurrentView = dto.CreateCurrentView,
                IsInSAS = dto.IsInSAS,
                SasLibrary = (dto.IsInSAS) ? dto.GenerateSASLibary(_datasetContext) : null
            };

            return de;
        }

        public void UpdateFields(int configId, int schemaId, List<SchemaRow> schemaRows)
        {
            DatasetFileConfig config = _datasetContext.GetById<DatasetFileConfig>(configId);

            try
            {
                SchemaRevision revision = new SchemaRevision()
                {
                    SchemaRevision_Name = "Another Revision " + (config.Schema.Revisions.Count() + 1).ToString(),
                    CreatedBy = _userService.GetCurrentUser().AssociateId
                };

                foreach (var row in schemaRows)
                {
                    switch (row.DataType.ToUpper())
                    {
                        case "INTEGER":
                            revision.Fields.Add(new IntegerField() {
                                Name = row.Name,
                                CreateDTM = DateTime.Now,
                                LastUpdateDTM = DateTime.Now,
                                ParentSchemaRevision = revision
                            });                            
                            break;
                        case "DECIMAL":
                            revision.Fields.Add(new DecimalField()
                            {
                                Name = row.Name,
                                CreateDTM = DateTime.Now,
                                LastUpdateDTM = DateTime.Now,
                                ParentSchemaRevision = revision,
                                Precision = Int32.Parse(row.Precision),
                                Scale = Int32.Parse(row.Scale)
                            });
                            break;
                        case "VARCHAR":
                            revision.Fields.Add(new VarcharField()
                            {
                                Name = row.Name,
                                CreateDTM = DateTime.Now,
                                LastUpdateDTM = DateTime.Now,
                                ParentSchemaRevision = revision
                            });
                            break;
                        case "DATE":
                            revision.Fields.Add(new DateField()
                            {
                                Name = row.Name,
                                CreateDTM = DateTime.Now,
                                LastUpdateDTM = DateTime.Now,
                                ParentSchemaRevision = revision
                            });
                            break;
                        case "TIMESTAMP":
                            revision.Fields.Add(new TimestampField()
                            {
                                Name = row.Name,
                                CreateDTM = DateTime.Now,
                                LastUpdateDTM = DateTime.Now,
                                ParentSchemaRevision = revision
                            });
                            break;
                        case "STRUCT":
                            revision.Fields.Add(new StructField()
                            {
                                Name = row.Name,
                                CreateDTM = DateTime.Now,
                                LastUpdateDTM = DateTime.Now,
                                ParentSchemaRevision = revision
                            });
                            break;
                        case "BIGINT":
                        default:
                            Logger.Info($"updatefields - datatype not supported ({row.DataType.ToUpper()})");
                            break;
                    }
                }

                config.Schema.AddRevision(revision);
                _datasetContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("configservice_updatefields - add new revision failed", ex);
            }
            


            DataElement schema = _datasetContext.GetById<DataElement>(schemaId);
            DataElement newRevision = null;
            DataObject DOBJ = null;
            Boolean newRev = false;
            if (schema.DataObjects.Count == 0)
            {
                //This is a new configuration with no schema defined.
                DOBJ = new DataObject()
                {
                    DataObjectCreate_DTM = DateTime.Now,
                    DataObjectChange_DTM = DateTime.Now,
                    LastUpdt_DTM = DateTime.Now,
                    DataObject_NME = schema.SchemaName,
                    DataObject_DSC = schema.SchemaDescription,
                    DataObject_CDE = config.FileExtension.Id.ToString(),
                    DataObjectCode_DSC = config.FileExtension.Name
                };

                schema.DataObjects.Add(DOBJ);
            }
            else
            {
                DOBJ = schema.DataObjects.Single();
            }

            List<DataObjectField> dofList = new List<DataObjectField>();

            foreach (SchemaRow sr in schemaRows)
            {
                DataObjectField dof = null;

                //Update Row
                if (sr.DataObjectField_ID != 0 && newRev == false)
                {
                    dof = DOBJ.DataObjectFields.Where(w => w.DataObjectField_ID == sr.DataObjectField_ID).FirstOrDefault();
                    dof.DataObjectField_NME = sr.Name;
                    dof.DataObjectField_DSC = sr.Description;
                    dof.LastUpdt_DTM = DateTime.Now;
                    dof.DataObjectFieldChange_DTM = DateTime.Now;
                }
                //Add New Row
                else
                {
                    dof = new DataObjectField();
                    dof.DataObject = DOBJ;
                    dof.DataObjectFieldCreate_DTM = DateTime.Now;
                    dof.DataObjectField_NME = sr.Name;
                    dof.DataObjectField_DSC = sr.Description;
                    dof.LastUpdt_DTM = DateTime.Now;
                    dof.DataObjectFieldChange_DTM = DateTime.Now;
                }


                if (sr.DataType != "ARRAY")
                {
                    dof.DataType = sr.DataType;
                }
                else
                {
                    dof.DataType = sr.DataType + "<" + sr.ArrayType + ">";
                }

                if (sr.Nullable != null) { dof.Nullable = sr.Nullable ?? null; }
                if (sr.Precision != null)
                {
                    if (sr.DataType == "DECIMAL")
                    {
                        dof.Precision = sr.Precision;
                    }
                    else
                    {
                        dof.Precision = null;
                    }
                }
                if (sr.Scale != null)
                {
                    if (sr.DataType == "DECIMAL")
                    {
                        dof.Scale = sr.Scale;
                    }
                    else
                    {
                        dof.Scale = null;
                    }
                }

                dofList.Add(dof);


            }

            DOBJ.DataObjectFields = dofList;

            _datasetContext.Merge(schema);
            _datasetContext.SaveChanges();

            if (newRev == true)
            {
                _datasetContext.Merge(newRevision);
                _datasetContext.SaveChanges();
            }

            //Send message to create hive table
            HiveTableCreateModel hiveCreate = new HiveTableCreateModel();
            schema.ToHiveCreateModel(hiveCreate);
            _messagePublisher.PublishDSCEvent(schema.DataElement_ID.ToString(), JsonConvert.SerializeObject(hiveCreate));

            //Send Email to have hive table added to SAS if option is checked
            schema.SendIncludeInSasEmail(_userService.GetCurrentUser(), _emailService);

        }


        public bool UpdateandSaveOAuthToken(HTTPSSource source, string newToken, DateTime tokenExpTime)
        {
            try
            {
                HTTPSSource updatedSource = (HTTPSSource)_datasetContext.GetById<DataSource>(source.Id);

                updatedSource.CurrentToken = _encryptService.EncryptString(newToken, Configuration.Config.GetHostSetting("EncryptionServiceKey"), source.IVKey).Item1;
                updatedSource.CurrentTokenExp = tokenExpTime;

                _datasetContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to save new OAuthToken", ex);
                return false;
            }
        }

        public DataSourceDto GetDataSourceDto(int Id)
        {
            DataSourceDto dto = new DataSourceDto();
            DataSource dsrc = _datasetContext.GetById<DataSource>(Id);
            MapToDto(dsrc, dto);
            return dto;
        }

        public UserSecurity GetUserSecurityForDataSource(int id)
        {
            DataSource ds = _datasetContext.DataSources.Where(x => x.Id == id).FetchSecurityTree(_datasetContext).FirstOrDefault();

            return _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());
        }

        public AccessRequest GetDataSourceAccessRequest(int dataSourceId)
        {
            DataSource ds = _datasetContext.GetById<DataSource>(dataSourceId);

            AccessRequest ar = new AccessRequest()
            {
                Permissions = _datasetContext.Permission.Where(x => x.SecurableObject == GlobalConstants.SecurableEntityName.DATASOURCE).ToList(),
                ApproverList = new List<KeyValuePair<string, string>>(),
                SecurableObjectId = ds.Id,
                SecurableObjectName = ds.Name
            };

            IApplicationUser primaryUser = _userService.GetByAssociateId(ds.PrimaryOwnerId);
            ar.ApproverList.Add(new KeyValuePair<string, string>(ds.PrimaryOwnerId, primaryUser.DisplayName + " (Owner)"));

            if (!string.IsNullOrWhiteSpace(ds.PrimaryContactId))
            {
                IApplicationUser secondaryUser = _userService.GetByAssociateId(ds.PrimaryContactId);
                ar.ApproverList.Add(new KeyValuePair<string, string>(ds.PrimaryContactId, secondaryUser.DisplayName + " (Contact)"));
            }

            return ar;
        }

        public string RequestAccessToDataSource(AccessRequest request)
        {

            DataSource ds = _datasetContext.GetById<DataSource>(request.SecurableObjectId);
            if (ds != null)
            {
                IApplicationUser user = _userService.GetCurrentUser();
                request.SecurableObjectName = ds.Name;
                request.SecurityId = ds.Security.SecurityId;
                request.RequestorsId = user.AssociateId;
                request.RequestorsName = user.DisplayName;
                request.IsProd = bool.Parse(Configuration.Config.GetHostSetting("RequireApprovalHPSMTickets"));
                request.RequestedDate = DateTime.Now;
                request.ApproverId = request.SelectedApprover;
                request.Permissions = _datasetContext.Permission.Where(x => request.SelectedPermissionCodes.Contains(x.PermissionCode) &&
                                                                                                                x.SecurableObject == GlobalConstants.SecurableEntityName.DATASOURCE).ToList();

                
                //Format the business reason here.
                StringBuilder sb = new StringBuilder();
                sb.Append($"Please grant the Ad Group {request.AdGroupName} the following permissions to the \"{request.SecurableObjectName}\" data source within Data.sentry.com.{ Environment.NewLine}");
                request.Permissions.ForEach(x => sb.Append($"{x.PermissionName} - {x.PermissionDescription} { Environment.NewLine}"));
                sb.Append($"Business Reason: {request.BusinessReason}{ Environment.NewLine}");
                sb.Append($"Requestor: {request.RequestorsId} - {request.RequestorsName}");

                request.BusinessReason = sb.ToString();

                return _securityService.RequestPermission(request);
            }

            return string.Empty;
        }

        public bool Delete(int id, bool logicalDelete = true)
        {
            try
            {
                DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(id);
                DataElement de = dfc.Schemas.FirstOrDefault();

                if (logicalDelete)
                {
                    Logger.Info($"configservice-delete-logical - configid:{id} configname:{dfc.Name}");
                    //Disable all associated RetrieverJobs
                    foreach (var job in dfc.RetrieverJobs)
                    {
                        _jobService.DisableJob(job.Id);
                    }

                    //Mark Object for delete to ensure they are not displaed in UI
                    //Goldeneye service will perform delete after determined amount of time
                    MarkForDelete(dfc);
                    MarkForDelete(de);
                    _datasetContext.SaveChanges();

                    //Send message to create hive table
                    HiveTableDeleteModel hiveDelete = new HiveTableDeleteModel();
                    de.ToHiveDeleteModel(hiveDelete);
                    _messagePublisher.PublishDSCEvent(de.DataElement_ID.ToString(), JsonConvert.SerializeObject(hiveDelete));

                }
                else
                {
                    Logger.Info($"configservice-delete-physical - configid:{id} configname:{dfc.Name}");
                    try
                    {
                        Logger.Info($"configservice-delete-disabledjobs - configid:{id} configname:{dfc.Name}");
                        //Ensure all associated RetrieverJobs are disabled
                        foreach (var job in dfc.RetrieverJobs)
                        {
                            _jobService.DisableJob(job.Id);
                        }

                        Logger.Info($"configservice-delete-deleteparquetstorage - configid:{id} configname:{dfc.Name}");
                        //Delete all parquet files under schema storage code
                        DeleteParquetFilesByStorageCode(de.StorageCode);

                        Logger.Info($"configservice-delete-deleterawstorage - configid:{id} configname:{dfc.Name}");
                        //Delete all raw data files under schema storage code
                        DeleteRawFilesByStorageCode(de.StorageCode);

                        Logger.Info($"configservice-delete-datasetfileparquetmetadata - configid:{id} configname:{dfc.Name}");
                        //Delete all DatasetFileParquet metadata  (inserts are managed outside of DSC code)
                        List<DatasetFileParquet> parquetFileList = _datasetContext.DatasetFileParquet.Where(w => w.SchemaId == de.DataElement_ID).ToList();
                        Logger.Info($"configservice-delete-datasetfileparquetmetadata - recordsfound:{parquetFileList.Count} configid:{id} configname:{dfc.Name}");
                        foreach (DatasetFileParquet record in parquetFileList)
                        {
                            _datasetContext.Remove(record);
                        }

                        Logger.Info($"configservice-delete-datasetfilereplymetadata - configid:{id} configname:{dfc.Name}");
                        //Delete all DatasetFileReply metadata  (inserts are managed outside of DSC code)
                        List<DatasetFileReply> replyList = _datasetContext.DatasetFileReply.Where(w => w.SchemaID == de.DataElement_ID).ToList();
                        Logger.Info($"configservice-delete-datasetfilereplymetadata - recordsfound:{parquetFileList.Count} configid:{id} configname:{dfc.Name}");
                        foreach (DatasetFileReply record in replyList)
                        {
                            _datasetContext.Remove(record);
                        }

                        Logger.Info($"configservice-delete-configmetadata - configid:{id} configname:{dfc.Name}");
                        //Delete all Schema metadata (will cascade delete to datafiles, dataelement, dataobject, dataobjectfield tables (including detail tables))
                        _datasetContext.Remove(dfc);
                        _datasetContext.SaveChanges();

                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"configservice-delete-permanant-failed - configid:{id} configname:{dfc.Name}", ex);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"configservice-delete-failed - configid:{id}", ex);
                return false;
            }
        }

        public UserSecurity GetUserSecurityForConfig(int id)
        {
            DatasetFileConfig dfc = _datasetContext.DatasetFileConfigs.Where(w => w.ConfigId == id).FirstOrDefault();
            return _securityService.GetUserSecurity(dfc.ParentDataset, _userService.GetCurrentUser());
        }

        public List<DatasetFileConfig> GetSchemaMarkedDeleted()
        {
            List<DatasetFileConfig> configList = _datasetContext.DatasetFileConfigs.Where(w => w.DeleteInd && w.DeleteIssueDTM < DateTime.Now.AddDays(Double.Parse(Configuration.Config.GetHostSetting("SchemaDeleteWaitDays")))).ToList();
            return configList;
        }

        public void DeleteParquetFilesByStorageCode(string storageCode)
        {
            S3ServiceProvider.DeleteS3Prefix($"parquet/{Configuration.Config.GetHostSetting("S3DataPrefix")}{storageCode}");
        }

        public void DeleteRawFilesByStorageCode(string storageCode)
        {
            S3ServiceProvider.DeleteS3Prefix($"{Configuration.Config.GetHostSetting("S3DataPrefix")}{storageCode}");
        }

        #region PrivateMethods
        private void MarkForDelete(DatasetFileConfig dfc)
        {
            dfc.DeleteInd = true;
            dfc.DeleteIssuer = _userService.GetCurrentUser().AssociateId;
            dfc.DeleteIssueDTM = DateTime.Now;
        }

        private void MarkForDelete(DataElement de)
        {
            de.DeleteInd = true;
            de.DeleteIssuer = _userService.GetCurrentUser().AssociateId;
            de.DeleteIssueDTM = DateTime.Now;
        }

        private void MapToDto(DataElement de, SchemaApiDTO dto)
        {
            dto.SchemaID = de.DataElement_ID;
            dto.Format = de.FileFormat;
            dto.Delimiter = de.Delimiter;
            dto.Header = de.HasHeader.ToString();
            dto.HiveDatabase = de.HiveDatabase;
            dto.HiveTable = de.HiveTable;
            dto.HiveStatus = de.HiveTableStatus;
            dto.HiveLocation = de.HiveLocation;
            dto.CurrentView = de.CreateCurrentView;
        }

        private void MapToDto(DataSource dsrc, DataSourceDto dto)
        {
            IApplicationUser primaryOwner = _userService.GetByAssociateId(dsrc.PrimaryOwnerId);
            IApplicationUser primaryContact = _userService.GetByAssociateId(dsrc.PrimaryContactId);

            dto.OriginatingId = dsrc.Id;
            dto.Name = dsrc.Name;
            dto.Description = dsrc.Description;
            dto.RetrunUrl = null;
            dto.SourceType = dsrc.SourceType;
            dto.AuthID = dsrc.SourceAuthType.AuthID.ToString();
            dto.IsUserPassRequired = dsrc.IsUserPassRequired;
            dto.PortNumber = dsrc.PortNumber;
            dto.BaseUri = dsrc.BaseUri;

            //Security Properites
            dto.IsSecured = dsrc.IsSecured;
            dto.PrimaryContactId = dsrc.PrimaryContactId;
            dto.PrimaryContactName = (primaryContact != null && primaryContact.AssociateId != "000000" ? primaryContact.DisplayName : "Not Available");
            dto.PrimaryContactEmail = (primaryContact != null && primaryContact.AssociateId != "000000" ? primaryContact.EmailAddress : "");
            dto.PrimaryOwnerId = dsrc.PrimaryOwnerId;
            dto.PrimaryOwnerName = (primaryOwner != null && primaryOwner.AssociateId != "000000" ? primaryOwner.DisplayName : "Not Available");
            dto.Security = _securityService.GetUserSecurity(dsrc, _userService.GetCurrentUser());
            dto.MailToLink = "mailto:" + dto.PrimaryContactEmail + "?Subject=Data%20Source%20Inquiry%20-%20" + dsrc.Name;

            MapDataSourceSpecificToDto(dsrc, dto);
        }

        private void MapDataSourceSpecificToDto(DataSource dsrc, DataSourceDto dto)
        {
            if (dsrc.Is<HTTPSSource>())
            {
                dto.ClientId = ((HTTPSSource)dsrc).ClientId;
                dto.ClientPrivateId = ((HTTPSSource)dsrc).ClientPrivateId;
                dto.TokenUrl = ((HTTPSSource)dsrc).TokenUrl;
                dto.TokenExp = ((HTTPSSource)dsrc).TokenExp;
                dto.Scope = ((HTTPSSource)dsrc).Scope;
                dto.RequestHeaders = ((HTTPSSource)dsrc).RequestHeaders;
                dto.TokenAuthHeader = ((HTTPSSource)dsrc).AuthenticationHeaderName;
            }
        }

        private void MaptToDetailDto(DataElement de, SchemaDetaiApilDTO dto)
        {
            MapToDto(de, dto);

            List<SchemaRow> rows = new List<SchemaRow>();
            DataObject dobj = de.DataObjects.FirstOrDefault();
            IList<DataObjectField> dofs = (dobj != null) ? dobj.DataObjectFields : null;

            if (dofs != null)
            {
                foreach (DataObjectField b in dofs)
                {
                    SchemaRow r = new SchemaRow()
                    {
                        Name = b.DataObjectField_NME,
                        DataObjectField_ID = b.DataObjectField_ID,
                        Description = b.DataObjectField_DSC,
                        LastUpdated = b.LastUpdt_DTM.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds
                    };

                    r.DataType = (!String.IsNullOrEmpty(b.DataType)) ? b.DataType.ToUpper() : "VARCHAR";
                    if (b.Precision != null) { r.Precision = b.Precision ?? null; }
                    if (b.Scale != null) { r.Scale = b.Scale ?? null; }
                    if (b.Nullable != null) { r.Nullable = b.Nullable ?? null; }
                    if (b.Length != null) { r.Length = b.Length ?? null; }
                    rows.Add(r);
                }
            }

            dto.Rows = rows;
        }
        private IList<ColumnDTO> MapToDto(IList<DataObject> objects)
        {
            IList<ColumnDTO> dtoList = new List<ColumnDTO>();
            foreach (DataObject table in objects)
            {
                foreach (DataObjectField field in table.DataObjectFields)
                {
                    ColumnDTO dto = new ColumnDTO()
                    {
                        Name = field.DataObjectField_NME,
                        DataType = field.DataType,
                        Length = field.Length,
                        Nullable = field.Nullable,
                        Precision = field.Precision,
                        Scale = field.Scale
                    };
                    dtoList.Add(dto);
                }
            }
            return dtoList;
        }


        private void UpdateDataSource(DataSourceDto dto, DataSource dsrc)
        {
            MapDataSource(dto, dsrc);
        }

        private void MapDataSource(DataSourceDto dto, DataSource dsrc)
        {
            dsrc.Name = dto.Name;
            dsrc.Description = dto.Description;
            dsrc.SourceType = dto.SourceType;
            dsrc.SourceAuthType = _datasetContext.GetById<AuthenticationType>(int.Parse(dto.AuthID));
            dsrc.IsUserPassRequired = dto.IsUserPassRequired;
            dsrc.PortNumber = dto.PortNumber;
            dsrc.BaseUri = dto.BaseUri;

            MapDataSourceSpecific(dto, dsrc);

            MapDataSourceSecurity(dto, dsrc);

        }

        private void MapDataSourceSecurity(DataSourceDto dto, DataSource dsrc)
        {
            dsrc.PrimaryOwnerId = dto.PrimaryOwnerId;
            dsrc.PrimaryContactId = dto.PrimaryContactId;

            if (dsrc.Security == null)
            {
                dsrc.Security = new Security(GlobalConstants.SecurableEntityName.DATASET)
                {
                    CreatedById = _userService.GetCurrentUser().AssociateId
                };
            }
            else
            {
                if (!dsrc.IsSecured && dto.IsSecured)
                {
                    dsrc.Security.EnabledDate = DateTime.Now;
                    dsrc.Security.UpdatedById = _userService.GetCurrentUser().AssociateId;
                    dsrc.Security.RemovedDate = null;
                }
                else if (dsrc.IsSecured && !dto.IsSecured)
                {
                    dsrc.Security.RemovedDate = DateTime.Now;
                    dsrc.Security.UpdatedById = _userService.GetCurrentUser().AssociateId;
                }

                dsrc.IsSecured = dto.IsSecured;
            }
        }

        private void MapDataSourceSpecific(DataSourceDto dto, DataSource dsrc)
        {
            if (dsrc.Is<HTTPSSource>())
            {
                ((HTTPSSource)dsrc).ClientId = dto.ClientId;
                ((HTTPSSource)dsrc).TokenUrl = dto.TokenUrl;
                ((HTTPSSource)dsrc).TokenExp = dto.TokenExp;
                ((HTTPSSource)dsrc).AuthenticationHeaderName = dto.TokenAuthHeader;
                ((HTTPSSource)dsrc).RequestHeaders = (dto.RequestHeaders.Any()) ? dto.RequestHeaders : null;

                UpdateClaims((HTTPSSource)dsrc, dto);
            }

            // only update if new value is supplied
            if (dto.TokenAuthValue != null)
            {
                ((HTTPSSource)dsrc).AuthenticationTokenValue = _encryptService.EncryptString(dto.TokenAuthValue, Configuration.Config.GetHostSetting("EncryptionServiceKey"), dto.IVKey).Item1;
            }

            // only update if new value is supplied
            if (dto.ClientPrivateId != null)
            {
                ((HTTPSSource)dsrc).ClientPrivateId = _encryptService.EncryptString(dto.ClientPrivateId, Configuration.Config.GetHostSetting("EncryptionServiceKey"), dto.IVKey).Item1;
            }

        }

        private void UpdateClaims(HTTPSSource dsrc, DataSourceDto dto)
        {
            //foreach claim type in dsrc
            // does claim type exist in dto?
            //if yes, update if value is different
            //if no, create OAuthClaim with dto value

            foreach (OAuthClaim item in dsrc.Claims)
            {
                switch (item.Type)
                {
                    case GlobalEnums.OAuthClaims.iss:
                        if (dto.ClientId != null && dto.ClientId != item.Value) { item.Value = dto.ClientId; }
                        break;
                    case GlobalEnums.OAuthClaims.aud:
                        if (dto.TokenUrl != null && dto.TokenUrl != item.Value) { item.Value = dto.TokenUrl; }
                        break;
                    case GlobalEnums.OAuthClaims.exp:
                        if (dto.TokenExp != 0 && dto.TokenExp.ToString() != item.Value) { item.Value = dto.TokenExp.ToString(); }
                        break;
                    case GlobalEnums.OAuthClaims.scope:
                        if (dto.Scope != null && dto.Scope != item.Value) { item.Value = dto.Scope; }
                        break;
                    default:
                        break;
                }
            }
        }

        private DataSource CreateDataSource(DataSourceDto dto)
        {
            DataSource source = null;

            AuthenticationType auth = _datasetContext.GetById<AuthenticationType>(Convert.ToInt32(dto.AuthID));

            switch (dto.SourceType)
            {
                case "DFSBasic":
                    source = new DfsBasic();
                    break;
                case "DFSCustom":
                    source = new DfsCustom();
                    break;
                case "FTP":
                    source = new FtpSource();
                    break;
                case "SFTP":
                    source = new SFtpSource();
                    break;
                case "HTTPS":
                    source = new HTTPSSource();
                    ((HTTPSSource)source).IVKey = _encryptService.GenerateNewIV();

                    //Process only if headers exist
                    if (dto.RequestHeaders.Any())
                    {
                        ((HTTPSSource)source).RequestHeaders = dto.RequestHeaders;
                    }

                    if (auth.Is<TokenAuthentication>())
                    {
                        ((HTTPSSource)source).AuthenticationHeaderName = dto.TokenAuthHeader;
                        ((HTTPSSource)source).AuthenticationTokenValue = _encryptService.EncryptString(dto.TokenAuthValue, Configuration.Config.GetHostSetting("EncryptionServiceKey"), ((HTTPSSource)source).IVKey).Item1;
                    }

                    if (auth.Is<OAuthAuthentication>())
                    {
                        ((HTTPSSource)source).ClientId = dto.ClientId;
                        ((HTTPSSource)source).ClientPrivateId = _encryptService.EncryptString(dto.ClientPrivateId, Configuration.Config.GetHostSetting("EncryptionServiceKey"), ((HTTPSSource)source).IVKey).Item1;
                        ((HTTPSSource)source).TokenUrl = dto.TokenUrl;
                        ((HTTPSSource)source).TokenExp = dto.TokenExp;
                        ((HTTPSSource)source).Scope = dto.Scope;
                    }
                    break;
                case "GOOGLEAPI":
                    source = new GoogleApiSource();
                    ((GoogleApiSource)source).IVKey = _encryptService.GenerateNewIV();

                    //Process only if headers exist
                    if (dto.RequestHeaders.Any())
                    {
                        ((GoogleApiSource)source).RequestHeaders = dto.RequestHeaders;
                    }

                    if (auth.Is<TokenAuthentication>())
                    {
                        ((GoogleApiSource)source).AuthenticationHeaderName = dto.TokenAuthHeader;
                        ((GoogleApiSource)source).AuthenticationTokenValue = _encryptService.EncryptString(dto.TokenAuthValue, Configuration.Config.GetHostSetting("EncryptionServiceKey"), ((HTTPSSource)source).IVKey).Item1;
                    }

                    if (auth.Is<OAuthAuthentication>())
                    {
                        ((GoogleApiSource)source).ClientId = dto.ClientId;
                        ((GoogleApiSource)source).ClientPrivateId = _encryptService.EncryptString(dto.ClientPrivateId, Configuration.Config.GetHostSetting("EncryptionServiceKey"), ((HTTPSSource)source).IVKey).Item1;
                        ((GoogleApiSource)source).TokenUrl = dto.TokenUrl;
                        ((GoogleApiSource)source).TokenExp = dto.TokenExp;
                        ((GoogleApiSource)source).Scope = dto.Scope;
                    }
                    break;
                default:
                    throw new NotImplementedException("SourceType is not configured for save");
            }

            source.Name = dto.Name;
            source.Description = dto.Description;
            source.SourceAuthType = auth;
            source.IsUserPassRequired = dto.IsUserPassRequired;
            source.BaseUri = dto.BaseUri;
            source.PortNumber = dto.PortNumber;
            source.IsSecured = dto.IsSecured;
            source.PrimaryOwnerId = dto.PrimaryOwnerId;
            source.PrimaryContactId = dto.PrimaryContactId;

            _datasetContext.Add(source);

            if (source.Is<HTTPSSource>() && auth.Is<OAuthAuthentication>())
            {
                CreateClaims(dto, (HTTPSSource)source);
            }

            if (source.IsSecured)
            {
                source.Security = new Security(GlobalConstants.SecurableEntityName.DATASOURCE)
                {
                    CreatedById = _userService.GetCurrentUser().AssociateId
                };
            }

            return source;
        }

        private void CreateClaims(DataSourceDto dto, HTTPSSource source)
        {
            List<OAuthClaim> claimsList = new List<OAuthClaim>();
            source.Claims = claimsList;
            OAuthClaim claim;

            claim = new OAuthClaim() { DataSourceId = source, Type = GlobalEnums.OAuthClaims.iss, Value = dto.ClientId };
            _datasetContext.Add(claim);
            claimsList.Add(claim);

            claim = new OAuthClaim() { DataSourceId = source, Type = GlobalEnums.OAuthClaims.scope, Value = dto.Scope };
            _datasetContext.Add(claim);
            claimsList.Add(claim);

            claim = new OAuthClaim() { DataSourceId = source, Type = GlobalEnums.OAuthClaims.aud, Value = dto.TokenUrl };
            _datasetContext.Add(claim);
            claimsList.Add(claim);

            claim = new OAuthClaim() { DataSourceId = source, Type = GlobalEnums.OAuthClaims.exp, Value = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Add(TimeSpan.FromMinutes(dto.TokenExp)).TotalSeconds.ToString() };
            _datasetContext.Add(claim);
            claimsList.Add(claim);
        }

        private void MapToDatasetFileConfigDto(DatasetFileConfig dfc, DatasetFileConfigDto dto)
        {
            dto.ConfigId = dfc.ConfigId;
            dto.Name = dfc.Name;
            dto.Description = dfc.Description;
            dto.DatasetScopeTypeId = dfc.DatasetScopeType.ScopeTypeId;
            dto.FileExtensionId = dfc.FileExtension.Id;
            dto.ParentDatasetId = dfc.ParentDataset.DatasetId;
            dto.StorageCode = dfc.GetStorageCode();
            dto.Security = _securityService.GetUserSecurity(null, _userService.GetCurrentUser());
            dto.CreateCurrentView = (dfc.Schemas.FirstOrDefault() != null) ? dfc.Schemas.FirstOrDefault().CreateCurrentView : false;
            dto.IsInSAS = (dfc.Schemas.FirstOrDefault() != null) ? dfc.Schemas.FirstOrDefault().IsInSAS : false;
            dto.Delimiter = dfc.Schemas.FirstOrDefault().Delimiter;
            dto.HasHeader = (dfc.Schemas.FirstOrDefault() != null) ? dfc.Schemas.FirstOrDefault().HasHeader : false;
            dto.Schema = GetSchemaDto(dfc.Schema.SchemaId);
        }
        #endregion
    }
}
