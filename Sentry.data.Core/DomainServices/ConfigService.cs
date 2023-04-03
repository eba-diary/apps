using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Common.Logging;
using Sentry.Configuration;
using Sentry.data.Core.DTO.Retriever;
using Sentry.data.Core.Entities;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Helpers;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class ConfigService : IConfigService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly IUserService _userService;
        private readonly IEventService _eventService;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IEncryptionService _encryptService;
        private readonly IJobService _jobService;
        private readonly IDataFlowService _dataFlowService;
        private readonly ISecurityService _securityService;
        private readonly ISchemaService _schemaService;
        private readonly IDataFeatures _featureFlags;
        private readonly ISAIDService _saidService;
        private readonly IDatasetFileService _datasetFileService;
        private readonly IGlobalDatasetProvider _globalDatasetProvider;

        public ConfigService(IDatasetContext dsCtxt, IUserService userService, IEventService eventService, 
            IMessagePublisher messagePublisher, IEncryptionService encryptService, ISecurityService securityService,
            IJobService jobService, ISchemaService schemaService, IDataFeatures dataFeatures, IDataFlowService dataFlowService, 
            ISAIDService saidService, IDatasetFileService datasetFileService, IGlobalDatasetProvider globalDatasetProvider)
        {
            _datasetContext = dsCtxt;
            _userService = userService;
            _eventService = eventService;
            _messagePublisher = messagePublisher;
            _encryptService = encryptService;
            _securityService = securityService;
            _jobService = jobService;
            _schemaService = schemaService;
            _featureFlags = dataFeatures;
            _dataFlowService = dataFlowService;
            _saidService = saidService;
            _datasetFileService = datasetFileService;
            _globalDatasetProvider = globalDatasetProvider;
        }

        public List<string> Validate(FileSchemaDto dto)
        {
            List<string> errors = new List<string>();

            var currentFileExtension = _datasetContext.FileExtensions.FirstOrDefault(x => x.Id == dto.FileExtensionId).Name.ToLower();
            
            if (currentFileExtension == "csv" && dto.Delimiter != ",")
            {
                errors.Add("File Extension CSV and it's delimiter do not match.");
            }

            if (currentFileExtension == "delimited" && string.IsNullOrWhiteSpace(dto.Delimiter))
            {
                errors.Add("File Extension Delimited is missing it's delimiter.");
            }

            if (dto.Name == null)
            {
                errors.Add("Configuration Name is required.");
            }
            else
            {
                if (String.IsNullOrWhiteSpace(dto.Name))
                {
                    errors.Add("Configuration Name is required.");
                }
                //else if (dto.Name.ToUpper() == "DEFAULT")
                //{
                //    errors.Add("Configuration Name cannot be named default.");
                //}
                else if (dto.Name.Length > 100)
                {
                    errors.Add("Configuration Name number of characters cannot be greater than 100.");
                }
            }


            if (dto.Description == null)
            {
                errors.Add("Configuration Description is required.");
            }
            else
            {
                if (String.IsNullOrWhiteSpace(dto.Description))
                {
                    errors.Add("Configuration Description is required..");
                }
                else if (dto.Description.Length > 2000)
                {
                    errors.Add("Configuration Description number of characters cannot be greater than 100.");
                }
            }

            return errors;
        }

        public List<string> Validate(DataSourceDto dto)
        {
            List<string> errors = new List<string>();

            AuthenticationType auth = _datasetContext.GetById<AuthenticationType>(Convert.ToInt32(dto.AuthID));

            switch (dto.SourceType)
            {
                case DataSourceDiscriminator.DFS_CUSTOM:
                    if (dto.OriginatingId == 0 && _datasetContext.DataSources.Where(w => w is DfsCustom && w.Name == dto.Name).Count() > 0)
                    {
                        errors.Add("An DFS Custom Data Source is already exists with this name.");
                    }
                    break;
                case DataSourceDiscriminator.FTP_SOURCE:
                    if (dto.OriginatingId == 0 && _datasetContext.DataSources.Where(w => w is FtpSource && w.Name == dto.Name).Count() > 0)
                    {
                        errors.Add("An FTP Data Source is already exists with this name.");
                    }
                    if (!(dto.BaseUri.ToString().StartsWith("ftp://")))
                    {
                        errors.Add("A valid FTP URI starts with ftp:// (i.e. ftp://foo.bar.com/base/dir)");
                    }
                    break;
                case DataSourceDiscriminator.SFTP_SOURCE:
                    if (dto.OriginatingId == 0 && _datasetContext.DataSources.Where(w => w is SFtpSource && w.Name == dto.Name).Count() > 0)
                    {
                        errors.Add("An SFTP Data Source is already exists with this name.");
                    }
                    if (!(dto.BaseUri.ToString().StartsWith("sftp://")))
                    {
                        errors.Add("A valid SFTP URI starts with sftp:// (i.e. sftp://foo.bar.com//base/dir/)");
                    }
                    break;
                case DataSourceDiscriminator.HTTPS_SOURCE:
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
                case DataSourceDiscriminator.GOOGLE_API_SOURCE:
                    if (!(dto.BaseUri.ToString().StartsWith("https://")))
                    {
                        errors.Add("A valid GoogleApi URI starts with https:// (i.e. https://analyticsreporting.googleapis.com/)");
                    }
                    if (!(dto.BaseUri.ToString().Contains("googleapis.com")))
                    {
                        errors.Add("An invalid GoogleApi URI");
                    }
                    break;
                case DataSourceDiscriminator.GOOGLE_BIG_QUERY_API_SOURCE:
                    if (!(dto.BaseUri.ToString().StartsWith("https://bigquery.googleapis.com")))
                    {
                        errors.Add("An invalid Google BigQuery URI");
                    }
                    break;
                case DataSourceDiscriminator.DEFAULT_DROP_LOCATION:
                default:
                    throw new NotImplementedException();
            }

            if (auth != null && auth.Is<OAuthAuthentication>())
            {
                if (string.IsNullOrWhiteSpace(dto.ClientId))
                {
                    errors.Add("OAuth requires a Client ID.");
                }
                
                if (string.IsNullOrWhiteSpace(dto.ClientPrivateId))
                {
                    errors.Add("OAuth requires a Client Private ID.");
                }
                if(!dto.Tokens.Any(t => !String.IsNullOrEmpty(t.TokenUrl) && !t.ToDelete))
                {
                    errors.Add("OAuth requires a Token URL.");
                }
                if (!dto.Tokens.Any(t => !t.ToDelete))
                {
                    errors.Add("OAuth requires at least one token.");
                }
                List<DataSourceTokenDto> tokensToValidate = dto.Tokens.Where(t => !t.ToDelete).ToList(); //skip validation on tokens we plan on removing 
                foreach (var token in tokensToValidate)
                {
                    if (string.IsNullOrWhiteSpace(token.TokenName))
                    {
                        errors.Add("OAuth requires a Token Name.");
                    }
                    if (dto.GrantType == GlobalEnums.OAuthGrantType.JwtBearer)
                    {
                        if (string.IsNullOrWhiteSpace(token.TokenUrl))
                        {
                            errors.Add("OAuth JWT requires a Token URL.");
                        }
                        if (string.IsNullOrWhiteSpace(token.Scope))
                        {
                            errors.Add("OAuth JWT requires a Token Scope.");
                        }
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(token.CurrentToken))
                        {
                            errors.Add("OAuth requires a Token Value.");
                        }
                        if (string.IsNullOrWhiteSpace(token.RefreshToken))
                        {
                            errors.Add("OAuth requires a Token Refresh Value.");
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(dto.PrimaryContactId))
            {
                errors.Add("Contact is requried.");
            }

            return errors;
        }

        public List<string> Validate(DatasetFileConfigDto dto)
        {
            List<string> errors = new List<string>();

            if (dto.ConfigId == 0)
            {
                Dataset parent = _datasetContext.GetById<Dataset>(dto.ParentDatasetId);

                if (dto.Name != null)
                {
                    if (dto.Name.Length > 100)
                    {
                        errors.Add("Configuration Name number of characters cannot be greater than 100.");
                    }
                    if (parent.DatasetFileConfigs.Any(x => !x.DeleteInd && x.Name.ToLower() == dto.Name.ToLower())) //remove any schemas which are marked for deletion
                    {
                        errors.Add("Dataset config with that name already exists within dataset");
                    }
                }

            }

            var currentFileExtension = _datasetContext.FileExtensions.FirstOrDefault(x => x.Id == dto.FileExtensionId).Name.ToLower();
            if (currentFileExtension == "delimited" && string.IsNullOrWhiteSpace(dto.Delimiter))
            {
                errors.Add("File Extension Delimited is missing it's delimiter.");
            }

            return errors;
        }

        public bool CreateAndSaveNewDataSource(DataSourceDto dto)
        {
            try
            {
                CreateDataSource(dto);
                _datasetContext.SaveChanges();
                _eventService.PublishSuccessEvent(GlobalConstants.EventType.CREATED_DATASOURCE, dto.Name + " was created.");

                return true;
            }
            catch (Exception ex)
            {
                _datasetContext.Clear();
                Logger.Error("Error creating data source", ex);

                return false;
            }
        }

        public bool UpdateAndSaveDataSource(DataSourceDto dto)
        {
            try
            {
                DataSource dsrc = _datasetContext.GetById<DataSource>(dto.OriginatingId);
                UpdateDataSource(dto, dsrc);
                _datasetContext.SaveChanges();
                _eventService.PublishSuccessEvent(GlobalConstants.EventType.UPDATED_DATASOURCE, dto.Name + " was updated.");

                return true;
            }
            catch (Exception ex)
            {
                _datasetContext.Clear();
                Logger.Error("datasource_save_error", ex);

                return false;
            }
        }

        public int Create(DatasetFileConfigDto dto)
        {
            DatasetFileConfig datasetFileConfig;
            try
            {
                datasetFileConfig = CreateDatasetFileConfig(dto);
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating Dataset File Config", ex);
                throw;
            }

            return datasetFileConfig.ConfigId;
        }

        public bool CreateAndSaveDatasetFileConfig(DatasetFileConfigDto dto)
        {
            try
            {
                CreateDatasetFileConfig(dto);
                _datasetContext.SaveChanges();

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

        public void UpdateDatasetFileConfig(DatasetFileConfigDto dto, DatasetFileConfig fileConfig)
        {
            if (dto.DatasetScopeTypeId > 0)
            {
                fileConfig.DatasetScopeType = _datasetContext.GetById<DatasetScopeType>(dto.DatasetScopeTypeId);
            }
            else if (!string.IsNullOrWhiteSpace(dto.DatasetScopeTypeName))
            {
                fileConfig.DatasetScopeType = _datasetContext.DatasetScopeTypes.FirstOrDefault(x => x.Name.ToLower() == dto.DatasetScopeTypeName.ToLower());
            }

            fileConfig.FileTypeId = dto.FileTypeId;
            fileConfig.Description = dto.Description;
            fileConfig.FileExtension = _datasetContext.GetById<FileExtension>(dto.FileExtensionId);
        }

        public DatasetFileConfigDto GetDatasetFileConfigDto(int configId)
        {
            DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(configId);

            UserSecurity us = _securityService.GetUserSecurity(dfc.ParentDataset, _userService.GetCurrentUser());

            if (us.CanPreviewDataset || us.CanViewFullDataset)
            {
                DatasetFileConfigDto dto = MapToDatasetFileConfigDto(dfc);
                return dto;
            }

            throw new SchemaUnauthorizedAccessException();            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datasetId"></param>
        /// <returns></returns>
        /// <exception cref="DatasetUnauthorizedAccessException">Thrown when user does not have access to dataset</exception>
        public List<DatasetFileConfigDto> GetDatasetFileConfigDtoByDataset(int datasetId)
        {
            UserSecurity us;
            try
            {
                us = _securityService.GetUserSecurity(_datasetContext.GetById<Dataset>(datasetId), _userService.GetCurrentUser());
            }
            catch (Exception ex)
            {
                Logger.Error($"configservice-validateviewpermissionsfordataset failed to retrieve UserSecurity object", ex);
                throw new DatasetUnauthorizedAccessException();
            }

            if (!(us.CanPreviewDataset || us.CanViewFullDataset || us.CanUploadToDataset || us.CanEditDataset || us.CanManageSchema))
            {
                try
                {
                    IApplicationUser user = _userService.GetCurrentUser();
                    Logger.Warn($"configservice-validateviewpermissionsfordataset unauthorized_access: Id:{user.AssociateId}");
                }
                catch (Exception ex)
                {
                    Logger.Error("configservice-validateviewpermissionsfordataset unauthorized_access", ex);
                }
                throw new DatasetUnauthorizedAccessException();
            }

            Dataset ds = _datasetContext.GetById<Dataset>(datasetId);
            if(ds == null)
            {
                throw new DatasetNotFoundException();
            }

            List<DatasetFileConfigDto> dtoList = new List<DatasetFileConfigDto>();
            foreach(DatasetFileConfig config in ds.DatasetFileConfigs)
            {
                dtoList.Add(GetDatasetFileConfigDto(config.ConfigId));
            }

            return dtoList;
        }

        private DatasetFileConfig CreateDatasetFileConfig(DatasetFileConfigDto dto)
        {
            DatasetFileConfig dfc = new DatasetFileConfig
            {
                Name = dto.Name,
                Description = dto.Description,
                FileTypeId = dto.FileTypeId,
                ParentDataset = _datasetContext.GetById<Dataset>(dto.ParentDatasetId),
                FileExtension = _datasetContext.GetById<FileExtension>(dto.FileExtensionId),
                ObjectStatus = dto.ObjectStatus,
                DeleteIssuer = null,
                DeleteIssueDTM = DateTime.MaxValue,
                IsSchemaTracked = true,
                Schema = _datasetContext.GetById<FileSchema>(dto.SchemaId)
            };

            if (!string.IsNullOrEmpty(dto.DatasetScopeTypeName))
            {
                dfc.DatasetScopeType = _datasetContext.DatasetScopeTypes.FirstOrDefault(x => x.Name.ToLower() == dto.DatasetScopeTypeName.ToLower());
            }
            else
            {
                dfc.DatasetScopeType = _datasetContext.GetById<DatasetScopeType>(dto.DatasetScopeTypeId);
            } 

            List<DatasetFileConfig> datasetFileConfigList = (dfc.ParentDataset.DatasetFileConfigs == null) ? new List<DatasetFileConfig>() : dfc.ParentDataset.DatasetFileConfigs.ToList();
            datasetFileConfigList.Add(dfc);
            _datasetContext.Add(dfc);
            dfc.ParentDataset.DatasetFileConfigs = datasetFileConfigList;

            return dfc;
        }

        public bool UpdateandSaveOAuthToken(HTTPSSource source, string newToken, DateTime tokenExpTime)
        {
            try
            {
                HTTPSSource updatedSource = (HTTPSSource)_datasetContext.GetById<DataSource>(source.Id);

                updatedSource.GetActiveTokens().FirstOrDefault().CurrentToken = _encryptService.EncryptString(newToken, Configuration.Config.GetHostSetting("EncryptionServiceKey"), source.IVKey).Item1;
                updatedSource.GetActiveTokens().FirstOrDefault().CurrentTokenExp = tokenExpTime;

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

        public async Task<AccessRequest> GetDataSourceAccessRequest(int dataSourceId)
        {
            DataSource ds = _datasetContext.GetById<DataSource>(dataSourceId);

            AccessRequest ar = new AccessRequest()
            {
                Permissions = _datasetContext.Permission.Where(x => x.SecurableObject == GlobalConstants.SecurableEntityName.DATASOURCE).ToList(),
                ApproverList = new List<KeyValuePair<string, string>>(),
                SecurableObjectId = ds.Id,
                SecurableObjectName = ds.Name
            };

            List<SAIDRole> prodCusts = await _saidService.GetApproversByKeyCodeAsync(ds.KeyCode).ConfigureAwait(false);
            foreach (SAIDRole prodCust in prodCusts)
            {
                ar.ApproverList.Add(new KeyValuePair<string, string>(prodCust.AssociateId, prodCust.Name));
            }

            return ar;
        }

        public async Task<string> RequestAccessToDataSource(AccessRequest request)
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
                request.IsAddingPermission = true;
                
                //Format the business reason here.
                StringBuilder sb = new StringBuilder();
                sb.Append($"Please grant the Ad Group {request.AdGroupName} the following permissions to the \"{request.SecurableObjectName}\" data source within Data.sentry.com.{ Environment.NewLine}");
                request.Permissions.ForEach(x => sb.Append($"{x.PermissionName} - {x.PermissionDescription} { Environment.NewLine}"));
                sb.Append($"Business Reason: {request.BusinessReason}{ Environment.NewLine}");
                sb.Append($"Requestor: {request.RequestorsId} - {request.RequestorsName}");

                request.BusinessReason = sb.ToString();

                return await _securityService.RequestPermission(request);
            }

            return string.Empty;
        }

        /// <summary>
        /// Config delete
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <param name="logicalDelete"></param>
        /// <returns></returns>
        public bool Delete(int id, IApplicationUser user, bool logicalDelete)
        {
            string methodName = $"{nameof(ConfigService).ToLower()}_{nameof(Delete).ToLower()}";
            Logger.Info($"{methodName} Method Start");

            bool returnResult = true;
            DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(id);

            bool IsAlreadyMarked = false;
            //Do not proceed with dataset file configuration is already marked for deletion
            //TODO: CLA-2765 - Remove deleteInd filter after testing completed
            if (logicalDelete && (dfc.DeleteInd || 
                dfc.ObjectStatus == GlobalEnums.ObjectStatusEnum.Pending_Delete || 
                dfc.ObjectStatus == GlobalEnums.ObjectStatusEnum.Deleted))
            {
                IsAlreadyMarked = true;
            }
            else if (!logicalDelete && dfc.ObjectStatus == GlobalEnums.ObjectStatusEnum.Deleted)
            {
                IsAlreadyMarked = true;
            }

            if (IsAlreadyMarked)
            {
                Logger.Info($"{methodName} Method End");
                return returnResult;
            }

            FileSchema scm = dfc.Schema;

            if (logicalDelete)
            {
                try
                {
                    Logger.Info($"{methodName} logical - configid:{id} configname:{dfc.Name}");

                    //Remove environment schema from dataset search index
                    Task deleteEnvironmentSchemaTask = Task.CompletedTask;
                    if (_featureFlags.CLA4789_ImprovedSearchCapability.GetValue())
                    {
                        deleteEnvironmentSchemaTask = _globalDatasetProvider.DeleteEnvironmentSchemaAsync(scm.SchemaId);
                    }

                    /*************************
                    * Legacy processing platform jobs where associated directly to datasetfileconfig object
                    * Disable all associated RetrieverJobs
                    *  
                    * These deletes should be refactored out once DatasetFileConfigs is refactor out
                    *************************/
                    bool jobsDeletedSuccessfully = true;
                    foreach (var job in dfc.RetrieverJobs)
                    {
                        bool successfullJobDelete = _jobService.Delete(job.Id, user, logicalDelete);
                        if (!successfullJobDelete)
                        {
                            jobsDeletedSuccessfully = successfullJobDelete;
                        }
                    }

                    //If any jobs failed to delete, then set returnResult = false
                    if (!jobsDeletedSuccessfully)
                    {
                        returnResult = jobsDeletedSuccessfully;
                    }

                    /*************************
                    *  Mark all dataflows, for deletion, associated with schema on new processing platform
                    *************************/
                    List<DataFlow> dataflows = new List<DataFlow>();
                    dataflows.AddRange(GetSchemaFlowByFileSchema(scm));
                    dataflows.AddRange(GetProducerFlowsByFileSchema(scm));

                    //finish environment schema delete to not waste time removing asset in data flow delete
                    deleteEnvironmentSchemaTask.Wait();

                    bool allDataFlowDeletesSuccessful = _dataFlowService.Delete(dataflows.Select(s => s.Id).ToList(), user, logicalDelete);
                    //If any dataflows failed to delete, then set returnResult = false
                    if (!allDataFlowDeletesSuccessful)
                    {
                        returnResult = allDataFlowDeletesSuccessful;
                    }

                    /*************************
                     * Mark objects for delete to ensure they are not displaed in UI
                     * WallEService, long running task within Goldeneye service, will perform delete after determined amount of time
                    *************************/
                    /* Mark dataset file config object for delete */
                    MarkForDelete(dfc);

                    /* Mark schema object for delete */
                    MarkForDelete(scm);

                    //DELETE ALL DATASETFILES UNDER SCHEMA BEING DELETED
                    DeleteDatasetFiles(dfc.ParentDataset.DatasetId, scm.SchemaId);
                }
                catch (Exception ex)
                {
                    Logger.Error($"{methodName} logical-failed - configid:{id}", ex);
                    returnResult = false;
                    return returnResult;
                }

                //If there are failures, we do not want to send out consumption layer events.
                if (!returnResult)
                {
                    return returnResult;
                }

                //We do not want to fail the delete due to events not being sent
                try
                {
                    GenerateConsumptionLayerDeleteEvent(dfc);
                }
                catch (AggregateException agEx)
                {
                    var flatArgExs = agEx.Flatten().InnerExceptions;
                    foreach (var ex in flatArgExs)
                    {
                        Logger.Error("Failed generating consumption layer event", ex);
                    }
                }
            }
            else
            {
                Logger.Info($"{methodName} physical - datasetid:{dfc.ParentDataset.DatasetId} configid:{id} configname:{dfc.Name}");
                try
                {
                    //Mark DatasetFileConfig record deleted
                    dfc.ObjectStatus = GlobalEnums.ObjectStatusEnum.Deleted;
                    dfc.Schema.ObjectStatus = GlobalEnums.ObjectStatusEnum.Deleted;


                    /*************************
                     * RetrieverJob associated with datasetfileconfigs are legacy jobs
                     * These deletes should be refactored out once DatasetFileConfigs is 
                     *   refactor out
                    **************************/
                    //Issue deletes for any retriever jobs associated with datasetfileconfig object                    
                    bool jobsDeletedSuccessfully = true;
                    foreach (var job in dfc.RetrieverJobs)
                    {
                        bool successfullJobDelete = _jobService.Delete(job.Id, user, logicalDelete);
                        if (!successfullJobDelete)
                        {
                            jobsDeletedSuccessfully = successfullJobDelete;
                        }
                    }

                    //If any jobs failed to delete, then set returnResult = false
                    if (!jobsDeletedSuccessfully)
                    {
                        returnResult = jobsDeletedSuccessfully;
                    }


                    /*************************
                     * Issue deletes for associated dataflows
                    *************************/
                    List<DataFlow> dataflows = new List<DataFlow>();
                    dataflows.AddRange(GetSchemaFlowByFileSchema(scm));
                    dataflows.AddRange(GetProducerFlowsByFileSchema(scm));

                    bool allDataFlowDeletesSuccessful = _dataFlowService.Delete(dataflows.Select(s => s.Id).ToList(), user, logicalDelete);
                    //If any dataflows failed to delete, then set returnResult = false
                    if (!allDataFlowDeletesSuccessful)
                    {
                        returnResult = allDataFlowDeletesSuccessful;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"{methodName} physical-failed - datasetid:{dfc.ParentDataset.DatasetId} configid:{id} configname:{dfc.Name}", ex);
                    return false;
                }
            }

            Logger.Info($"{methodName} Method End");
            return returnResult;
        }

        private IEnumerable<DataFlow> GetProducerFlowsByFileSchema(FileSchema scm)
        {
            List<DataFlow> producerFlowIdList = new List<DataFlow>();
            /* Get producer flow Ids which map to schema that are active*/
            List<Tuple<int, DataFlow>> producerSchemaMapIds = _datasetContext.SchemaMap.Where(w =>
                                            w.MappedSchema.SchemaId == scm.SchemaId
                                            && !w.DataFlowStepId.DataFlow.Name.Contains("FileSchemaFlow")
                                            && w.DataFlowStepId.DataFlow.ObjectStatus != GlobalEnums.ObjectStatusEnum.Deleted
                                            ).Select(s => new Tuple<int, DataFlow>(s.DataFlowStepId.Id, s.DataFlowStepId.DataFlow)).ToList();

            foreach (Tuple<int, DataFlow> item in producerSchemaMapIds)
            {
                /*  Add producer dataflow id to list if the
                 *  dataflow does not map to any other schema
                 *  
                 *  Or add producer dataflow id to list if all
                 *  other mapped schema are NOT Active    
                 */
                if (!_datasetContext.SchemaMap.Any(w => w.DataFlowStepId.Id == item.Item1 && w.MappedSchema != scm))
                {
                    producerFlowIdList.Add(item.Item2);
                }
                else if (!_datasetContext.SchemaMap.Any(w => w.DataFlowStepId.Id == item.Item1 && w.MappedSchema != scm && w.MappedSchema.ObjectStatus == GlobalEnums.ObjectStatusEnum.Active))
                {
                    producerFlowIdList.Add(item.Item2);
                }
            }

            return producerFlowIdList;
        }

        private List<DataFlow> GetSchemaFlowByFileSchema(FileSchema scm)
        {
            List<DataFlow> dataflowList = new List<DataFlow>();
            var schemaflowName = _dataFlowService.GetDataFlowNameForFileSchema(scm);
            DataFlow schemaFlow = _datasetContext.DataFlow.FirstOrDefault(w => w.Name == schemaflowName);

            //some legacy dataset\schema may not have associated schema flow
            if (schemaFlow == null)
            {
                Logger.Debug($"Schema Flow not found by name, attempting to detect by id...");

                SchemaMap mappedStep = _datasetContext.SchemaMap.SingleOrDefault(w => w.MappedSchema.SchemaId == scm.SchemaId && w.DataFlowStepId.Action.Name == "Schema Load" && w.DataFlowStepId.DataFlow.Name.StartsWith("FileSchema"));
                if (mappedStep != null)
                {
                    Logger.Debug($"detected schema flow by Id");
                    schemaFlow = mappedStep.DataFlowStepId.DataFlow;
                    dataflowList.Add(schemaFlow);
                }
                else
                {
                    Logger.Debug($"schema flow not detected by id");
                    Logger.Debug($"no schema flow associated with schema");
                }
            }
            else
            {
                dataflowList.Add(schemaFlow);
            }

            return dataflowList;
        }

        public UserSecurity GetUserSecurityForConfig(int id)
        {
            DatasetFileConfig dfc = _datasetContext.DatasetFileConfigs.Where(w => w.ConfigId == id).FirstOrDefault();
            return _securityService.GetUserSecurity(dfc.ParentDataset, _userService.GetCurrentUser());
        }

        public bool SyncConsumptionLayer(int datasetId, int schemaId)
        {
            if (datasetId == 0)
            {
                throw new ArgumentException("Argument is required", "datasetId");
            }

            bool RefreshAllSchema;
            Dataset ds = null;
            if (schemaId == 0) 
            {
                ds = _datasetContext.GetById<Dataset>(datasetId);
                RefreshAllSchema = true;
            }
            else
            {
                ds = _datasetContext.DatasetFileConfigs.FirstOrDefault(w => w.Schema.SchemaId == schemaId && w.ParentDataset.DatasetId == datasetId).ParentDataset;
                RefreshAllSchema = false;
            }

            if (ds == null)
            {
                return false;
            }

            IApplicationUser user = _userService.GetCurrentUser();
            UserSecurity us = _securityService.GetUserSecurity(ds, user);
            if (!us.CanManageSchema)
            {
                throw new SchemaUnauthorizedAccessException();
            }

            try
            {
                //Get list of schema(s) to refresh
                List<DatasetFileConfig> configList;
                configList = (RefreshAllSchema)
                    ? _datasetContext.DatasetFileConfigs.Where(w => w.ParentDataset.DatasetId == datasetId).ToList()
                    : _datasetContext.DatasetFileConfigs.Where(w => w.ParentDataset.DatasetId == datasetId && w.Schema.SchemaId == schemaId).ToList();
                
                GenerateSchemaCreateEvent(configList);

                //Write success event
                if (RefreshAllSchema)
                {
                    _eventService.PublishSuccessEventByDatasetId(GlobalConstants.EventType.SYNC_DATASET_SCHEMA, "Sync all schemas for dataset", datasetId);
                }
                else
                {
                    _eventService.PublishSuccessEventByConfigId(GlobalConstants.EventType.SYNC_DATASET_SCHEMA, "Sync specific schema", configList.First().ConfigId);
                }
                                
                return true;

            }
            catch (Exception ex)
            {
                Logger.Error("configservice-syncconsumptionlayer failed", ex);
                return false;
            }
        }

        #region PrivateMethods
        private string GetDSCEventTopic(DatasetFileConfig config)
        {
            string topicName = null;
            DscEventTopicHelper helper = new DscEventTopicHelper();
            topicName = helper.GetDSCTopic(config);
            if (string.IsNullOrEmpty(topicName))
            {
                throw new ArgumentException("Topic Name is null");
            }
            return topicName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <exception cref="AggregateException"></exception>
        private void GenerateConsumptionLayerDeleteEvent(DatasetFileConfig config)
        {
            var exceptionList = new List<Exception>();

            //Send message to delete hive table\views
            HiveTableDeleteModel hiveDelete = new HiveTableDeleteModel()
            {
                SchemaID = config.Schema.SchemaId,
                DatasetID = config.ParentDataset.DatasetId,
                InitiatorID = _userService.GetCurrentUser().AssociateId
            };

            try
            {
                Logger.Debug($"<generateconsumptionlayerdeleteevent> sending {hiveDelete.EventType.ToLower()} event...");

                string topicName = null;
                if (string.IsNullOrWhiteSpace(_featureFlags.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()))
                {
                    topicName = GetDSCEventTopic(config);
                    _messagePublisher.Publish(topicName, config.Schema.SchemaId.ToString(), JsonConvert.SerializeObject(hiveDelete));
                }
                else
                {
                    _messagePublisher.PublishDSCEvent(config.Schema.SchemaId.ToString(), JsonConvert.SerializeObject(hiveDelete), topicName);
                }
                Logger.Debug($"<generateconsumptionlayerdeleteevent> sent {hiveDelete.EventType.ToLower()} event");
            }
            catch (Exception ex)
            {
                Logger.Error($"<generateconsumptionlayerdeleteevent> failed sending event: {JsonConvert.SerializeObject(hiveDelete)}");
                exceptionList.Add(ex);
            }

            //Send message to delete snowflake table\views
            SnowTableDeleteModel snowDelete = new SnowTableDeleteModel()
            {
                SchemaID = config.Schema.SchemaId,
                DatasetID = config.ParentDataset.DatasetId,
                InitiatorID = _userService.GetCurrentUser().AssociateId
            };

            try
            {
                Logger.Debug($"<generateconsumptionlayerdeleteevent> sending {snowDelete.EventType.ToLower()} event...");

                string topicName = null;
                if (string.IsNullOrWhiteSpace(_featureFlags.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()))
                {
                    topicName = GetDSCEventTopic(config);
                    _messagePublisher.Publish(topicName, config.Schema.SchemaId.ToString(), JsonConvert.SerializeObject(snowDelete));
                }
                else
                {
                    _messagePublisher.PublishDSCEvent(config.Schema.SchemaId.ToString(), JsonConvert.SerializeObject(snowDelete), topicName);
                }

                Logger.Debug($"<generateconsumptionlayerdeleteevent> sent {snowDelete.EventType.ToLower()} event");               
            }
            catch (Exception ex)
            {
                Logger.Error($"<generateconsumptionlayerdeleteevent> failed sending event: {JsonConvert.SerializeObject(snowDelete)}");
                exceptionList.Add(ex);
            }

            if (exceptionList.Any())
            {
                throw new AggregateException("Failed sending consumption layer event", exceptionList);
            }
        }


        /// <summary>
        /// Generate the necessary consumption layer create events
        /// </summary>
        /// <param name="configList"></param>
        /// <exception cref="AggregateException">Thows exception when event could not be published</exception>
        private void GenerateSchemaCreateEvent(List<DatasetFileConfig> configList)
        {
            var exceptionList = new List<Exception>();

            //Refresh consumption layer
            foreach (DatasetFileConfig config in configList.Where(w => w.Schema.Revisions.Any() && !w.DeleteInd))
            {
                //Always generate hive table create event
                HiveTableCreateModel hiveModel = new HiveTableCreateModel()
                {
                    DatasetID = config.ParentDataset.DatasetId,
                    SchemaID = config.Schema.SchemaId,
                    RevisionID = config.GetLatestSchemaRevision().SchemaRevision_Id,
                    InitiatorID = _userService.GetCurrentUser().AssociateId
                };

                try
                {
                    Logger.Debug($"<generateschemacreateevent> sending {hiveModel.EventType.ToLower()} event...");

                    string topicName = null;
                    if (string.IsNullOrWhiteSpace(_featureFlags.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()))
                    {
                        topicName = GetDSCEventTopic(config);
                        _messagePublisher.Publish(topicName, hiveModel.SchemaID.ToString(), JsonConvert.SerializeObject(hiveModel));
                    }
                    else
                    {
                        _messagePublisher.PublishDSCEvent(hiveModel.SchemaID.ToString(), JsonConvert.SerializeObject(hiveModel), topicName);
                    }

                    Logger.Debug($"<generateschemacreateevent> sent {hiveModel.EventType.ToLower()} event");
                }
                catch (Exception ex)
                {
                    Logger.Error($"<generateschemacreateevent> failed sending event: {JsonConvert.SerializeObject(hiveModel)}");
                    exceptionList.Add(ex);
                }                

                //Always generate snowflake table create event
                SnowTableCreateModel snowModel = new SnowTableCreateModel()
                {
                    DatasetID = config.ParentDataset.DatasetId,
                    SchemaID = config.Schema.SchemaId,
                    RevisionID = config.GetLatestSchemaRevision().SchemaRevision_Id,
                    InitiatorID = _userService.GetCurrentUser().AssociateId
                };

                try
                {
                    Logger.Debug($"<generateschemacreateevent> sending {snowModel.EventType.ToLower()} event...");

                    string topicName = null;
                    if (string.IsNullOrWhiteSpace(_featureFlags.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()))
                    {
                        topicName = GetDSCEventTopic(config);
                        _messagePublisher.Publish(topicName, snowModel.SchemaID.ToString(), JsonConvert.SerializeObject(snowModel));
                    }
                    else
                    {
                        _messagePublisher.PublishDSCEvent(snowModel.SchemaID.ToString(), JsonConvert.SerializeObject(snowModel), topicName);
                    }
                    Logger.Debug($"<generateschemacreateevent> sent {snowModel.EventType.ToLower()} event");
                }
                catch (Exception ex)
                {
                    Logger.Error($"<generateschemacreateevent> failed sending event: {JsonConvert.SerializeObject(snowModel)}");
                    exceptionList.Add(ex);
                }
            }

            if (exceptionList.Any())
            {
                throw new AggregateException("Failed sending consumption layer event", exceptionList);
            }
        }

        private void MarkForDelete(DatasetFileConfig dfc)
        {
            dfc.DeleteInd = true;
            dfc.DeleteIssuer = _userService.GetCurrentUser().AssociateId;
            dfc.DeleteIssueDTM = DateTime.Now;
            dfc.ObjectStatus = GlobalEnums.ObjectStatusEnum.Pending_Delete;
        }

        private void MarkForDelete(FileSchema scm)
        {
            scm.DeleteInd = true;
            scm.DeleteIssuer = _userService.GetCurrentUser().AssociateId;
            scm.DeleteIssueDTM = DateTime.Now;
            scm.ObjectStatus = GlobalEnums.ObjectStatusEnum.Pending_Delete;
        }

        private void DeleteDatasetFiles(int datasetId, int schemaId)
        {
            //DO NOT DELETE IF FEATURE IS OFF
            if (!_featureFlags.CLA4049_ALLOW_S3_FILES_DELETE.GetValue())
            {
                return;
            }

            //GET ALL DATASETFILE IDS FOR SCHEMA
            List<DatasetFile> dbList = _datasetContext.DatasetFileStatusActive.Where(w => w.Schema.SchemaId == schemaId).ToList();

            //DELETE ALL FILES IN LIST
            if (dbList != null && dbList.Count > 0)
            {
                int[] idList = dbList.Select(s => s.DatasetFileId).ToArray();
                DeleteFilesParamDto dto = new DeleteFilesParamDto() { UserFileIdList = idList };
                _datasetFileService.Delete(datasetId, schemaId, dto);
            }
        }

        private void MapToDto(DataSource dsrc, DataSourceDto dto)
        {
            IApplicationUser primaryContact = _userService.GetByAssociateId(dsrc.PrimaryContactId);

            dto.OriginatingId = dsrc.Id;
            dto.Name = dsrc.Name;
            dto.Description = dsrc.Description;
            dto.ReturnUrl = null;
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
            dto.Security = _securityService.GetUserSecurity(dsrc, _userService.GetCurrentUser());
            dto.MailToLink = "mailto:" + dto.PrimaryContactEmail + "?Subject=Data%20Source%20Inquiry%20-%20" + dsrc.Name;

            MapDataSourceSpecificToDto(dsrc, dto);
        }

        private void MapDataSourceSpecificToDto(DataSource dsrc, DataSourceDto dto)
        {
            if (dsrc.Is<HTTPSSource>())
            {
                dto.ClientId = ((HTTPSSource)dsrc).ClientId;
                dto.ClientPrivateId = _encryptService.PrepEncryptedForDisplay(((HTTPSSource)dsrc).ClientPrivateId);
                dto.Tokens = new List<DataSourceTokenDto>();
                MapDataSourceTokensToDtoTokens(((HTTPSSource)dsrc).AllTokens, dto.Tokens);
                dto.RequestHeaders = ((HTTPSSource)dsrc).RequestHeaders;
                dto.TokenAuthHeader = ((HTTPSSource)dsrc).AuthenticationHeaderName;
                dto.SupportsPaging = ((HTTPSSource)dsrc).SupportsPaging;
                dto.GrantType = ((HTTPSSource)dsrc).GrantType;
            }
        }

        private void MapDtoTokensToDataSourceTokens(DataSourceDto dataSourceDto, HTTPSSource httpsSource)
        {
            foreach(DataSourceTokenDto dtoToken in dataSourceDto.Tokens)
            {
                if (dtoToken.ToDelete)
                {
                    _datasetContext.RemoveById<DataSourceToken>(dtoToken.Id);
                    var toRemove = httpsSource.AllTokens.First(t => t.Id == dtoToken.Id);
                    httpsSource.AllTokens.Remove(toRemove);
                }
                else
                {
                    bool update = false;
                    DataSourceToken token = new DataSourceToken();

                    if (dtoToken.Id != 0 && httpsSource.AllTokens.Any(dsToken => dsToken.Id == dtoToken.Id))
                    {
                        token = httpsSource.AllTokens.First(dsToken => dsToken.Id == dtoToken.Id);
                        update = true;
                    }

                    token.ParentDataSource = httpsSource;
                    token.TokenName = dtoToken.TokenName;
                    token.RefreshToken = EncryptString(dtoToken.RefreshToken, httpsSource.IVKey);
                    token.CurrentToken = EncryptString(dtoToken.CurrentToken, httpsSource.IVKey);
                    token.CurrentTokenExp = dtoToken.CurrentTokenExp;
                    token.TokenExp = dtoToken.TokenExp;
                    token.TokenUrl = dtoToken.TokenUrl;
                    token.Scope = dtoToken.Scope;
                    token.Enabled = dtoToken.Enabled;

                    if (update)
                    {
                        UpdateClaimsForToken(token, dataSourceDto);
                    }
                    else
                    {
                        CreateClaimsForToken(token, dataSourceDto, httpsSource);
                        httpsSource.AllTokens.Add(token);
                    }
                }
            }
        }

        private string EncryptString(string input, string ivKey)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            if (_encryptService.IsEncrypted(input))
            {
                return input.Replace(Indicators.ENCRYPTIONINDICATOR, "");
            }

            return _encryptService.EncryptString(input, Config.GetHostSetting("EncryptionServiceKey"), ivKey).Item1;
        }

        private void MapDataSourceTokensToDtoTokens(IList<DataSourceToken> dataSourceTokens, IList<DataSourceTokenDto> dtoTokens)
        {
            dtoTokens.Clear();
            foreach(var dataSourceToken in dataSourceTokens)
            {
                dtoTokens.Add(new DataSourceTokenDto
                {
                    Id = dataSourceToken.Id,
                    CurrentToken = _encryptService.PrepEncryptedForDisplay(dataSourceToken.CurrentToken),
                    RefreshToken = _encryptService.PrepEncryptedForDisplay(dataSourceToken.RefreshToken),
                    CurrentTokenExp = dataSourceToken.CurrentTokenExp,
                    TokenExp = dataSourceToken.TokenExp,
                    TokenName = dataSourceToken.TokenName,
                    TokenUrl = dataSourceToken.TokenUrl,
                    Scope = dataSourceToken.Scope,
                    Enabled = dataSourceToken.Enabled
                });
            }
        }

        private void UpdateDataSource(DataSourceDto dto, DataSource dsrc)
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
            string encryptionKey = Config.GetHostSetting("EncryptionServiceKey");

            if (dsrc.Is<HTTPSSource>())
            {
                ((HTTPSSource)dsrc).ClientId = dto.ClientId;
                ((HTTPSSource)dsrc).AuthenticationHeaderName = dto.TokenAuthHeader;
                ((HTTPSSource)dsrc).SupportsPaging = dto.SupportsPaging;

                if (dto.RequestHeaders.Any())
                {
                    ((HTTPSSource)dsrc).RequestHeaders = dto.RequestHeaders;
                }

                if (dsrc.SourceAuthType.Is<OAuthAuthentication>())
                {
                    ((HTTPSSource)dsrc).ClientPrivateId = EncryptString(dto.ClientPrivateId, ((HTTPSSource)dsrc).IVKey);
                    ((HTTPSSource)dsrc).GrantType = dto.GrantType;
                    MapDtoTokensToDataSourceTokens(dto, (HTTPSSource)dsrc);
                }
            }

            // only update if new value is supplied
            if (dto.TokenAuthValue != null)
            {
                ((HTTPSSource)dsrc).AuthenticationTokenValue = _encryptService.EncryptString(dto.TokenAuthValue, encryptionKey, dto.IVKey).Item1;
            }

        }

        internal void MapToDto(DataSourceToken token, DataSourceTokenDto dto)
        {
            dto.Id = token.Id;
            dto.CurrentToken = token.CurrentToken;
            dto.RefreshToken = token.RefreshToken;
            dto.CurrentTokenExp = token.CurrentTokenExp;
            dto.TokenName = token.TokenName;
            dto.TokenUrl = token.TokenUrl;
            dto.TokenExp = token.TokenExp;
            dto.Scope = token.Scope;
            dto.Enabled = token.Enabled;
        }

        internal void MapToDataSourceToken(DataSourceTokenDto dto, DataSourceToken token)
        {
            token.Id = dto.Id;
            token.CurrentToken = dto.CurrentToken;
            token.RefreshToken = dto.RefreshToken;
            token.CurrentTokenExp = dto.CurrentTokenExp;
            token.TokenName = dto.TokenName;
            token.TokenUrl = dto.TokenUrl;
            token.TokenExp = dto.TokenExp;
            token.Scope = dto.Scope;
            token.Enabled = dto.Enabled;
        }

        private DataSource CreateDataSource(DataSourceDto dto)
        {
            AuthenticationType auth = _datasetContext.GetById<AuthenticationType>(Convert.ToInt32(dto.AuthID));

            DataSource source = GetDataSourceByType(dto, auth);

            source.Name = dto.Name;
            source.Description = dto.Description;
            source.SourceAuthType = auth;
            source.IsUserPassRequired = dto.IsUserPassRequired;
            source.BaseUri = dto.BaseUri;
            source.PortNumber = dto.PortNumber;
            source.IsSecured = dto.IsSecured;
            source.PrimaryContactId = dto.PrimaryContactId;

            _datasetContext.Add(source);

            if (auth.Is<OAuthAuthentication>())
            {
                ((HTTPSSource)source).AllTokens = new List<DataSourceToken>();
                MapDtoTokensToDataSourceTokens(dto, (HTTPSSource)source);
            }

            if (source.IsSecured)
            {
                source.Security = new Security(SecurableEntityName.DATASOURCE)
                {
                    CreatedById = _userService.GetCurrentUser().AssociateId
                };
            }

            return source;
        }

        private DataSource GetDataSourceByType(DataSourceDto dto, AuthenticationType auth)
        {
            DataSource source;
            string ivKey = _encryptService.GenerateNewIV();
            string encryptionKey = Config.GetHostSetting("EncryptionServiceKey");

            switch (dto.SourceType)
            {
                case DataSourceDiscriminator.DEFAULT_DROP_LOCATION:
                    source = new DfsBasic();
                    break;
                case DataSourceDiscriminator.DFS_CUSTOM:
                    source = new DfsCustom();
                    break;
                case DataSourceDiscriminator.FTP_SOURCE:
                    source = new FtpSource();
                    break;
                case DataSourceDiscriminator.SFTP_SOURCE:
                    source = new SFtpSource();
                    break;
                case DataSourceDiscriminator.HTTPS_SOURCE:
                    source = new HTTPSSource()
                    {
                        IVKey = ivKey,
                        SupportsPaging = dto.SupportsPaging
                    };

                    //Process only if headers exist
                    if (dto.RequestHeaders.Any())
                    {
                        ((HTTPSSource)source).RequestHeaders = dto.RequestHeaders;
                    }

                    if (auth.Is<TokenAuthentication>())
                    {
                        ((HTTPSSource)source).AuthenticationHeaderName = dto.TokenAuthHeader;
                        ((HTTPSSource)source).AuthenticationTokenValue = _encryptService.EncryptString(dto.TokenAuthValue, encryptionKey, ivKey).Item1;
                    }

                    if (auth.Is<OAuthAuthentication>())
                    {
                        ((HTTPSSource)source).ClientId = dto.ClientId;
                        ((HTTPSSource)source).ClientPrivateId = _encryptService.EncryptString(dto.ClientPrivateId, encryptionKey, ivKey).Item1;
                        ((HTTPSSource)source).GrantType = dto.GrantType;
                    }
                    break;
                case DataSourceDiscriminator.GOOGLE_API_SOURCE:
                    source = new GoogleApiSource()
                    {
                        IVKey = ivKey
                    };

                    //Process only if headers exist
                    if (dto.RequestHeaders.Any())
                    {
                        ((GoogleApiSource)source).RequestHeaders = dto.RequestHeaders;
                    }

                    if (auth.Is<TokenAuthentication>())
                    {
                        ((GoogleApiSource)source).AuthenticationHeaderName = dto.TokenAuthHeader;
                        ((GoogleApiSource)source).AuthenticationTokenValue = _encryptService.EncryptString(dto.TokenAuthValue, encryptionKey, ivKey).Item1;
                    }

                    if (auth.Is<OAuthAuthentication>())
                    {
                        ((GoogleApiSource)source).ClientId = dto.ClientId;
                        ((GoogleApiSource)source).ClientPrivateId = _encryptService.EncryptString(dto.ClientPrivateId, encryptionKey, ivKey).Item1;
                    }
                    break;
                case DataSourceDiscriminator.GOOGLE_BIG_QUERY_API_SOURCE:
                    source = new GoogleBigQueryApiSource()
                    {
                        IVKey = ivKey,
                        ClientId = dto.ClientId,
                        ClientPrivateId = _encryptService.EncryptString(dto.ClientPrivateId, encryptionKey, ivKey).Item1
                    };

                    break;
                default:
                    throw new NotImplementedException("SourceType is not configured for save");
            }

            return source;
        }

        private void CreateClaimsForToken(DataSourceToken token, DataSourceDto dto, HTTPSSource source)
        {
            token.Claims = new List<OAuthClaim>();

            OAuthClaim claim = new OAuthClaim() { DataSourceId = source, Type = GlobalEnums.OAuthClaims.iss, Value = dto.ClientId };
            _datasetContext.Add(claim);
            token.Claims.Add(claim);

            claim = new OAuthClaim() { DataSourceId = source, Type = GlobalEnums.OAuthClaims.aud, Value = token.TokenUrl };
            _datasetContext.Add(claim);
            token.Claims.Add(claim);

            claim = new OAuthClaim() { DataSourceId = source, Type = GlobalEnums.OAuthClaims.exp, Value = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Add(TimeSpan.FromMinutes(token.TokenExp)).TotalSeconds.ToString() };
            _datasetContext.Add(claim);
            token.Claims.Add(claim);

            claim = new OAuthClaim() { DataSourceId = source, Type = GlobalEnums.OAuthClaims.scope, Value = token.Scope };
            _datasetContext.Add(claim);
            token.Claims.Add(claim);
        }

        private void UpdateClaimsForToken(DataSourceToken token, DataSourceDto dto)
        {
            var dtoToken = dto.Tokens.First(t => t.Id == token.Id);
            foreach (OAuthClaim claim in token.Claims)
            {
                if (claim.Type == GlobalEnums.OAuthClaims.iss && dto.ClientId != null && dto.ClientId != claim.Value)
                {
                    claim.Value = dto.ClientId;
                }
                else if (claim.Type == GlobalEnums.OAuthClaims.aud && dtoToken.TokenUrl != null && dtoToken.TokenUrl != claim.Value)
                {
                    claim.Value = dtoToken.TokenUrl;
                }
                else if (claim.Type == GlobalEnums.OAuthClaims.exp && dtoToken.TokenExp != 0 && dtoToken.TokenExp.ToString() != claim.Value)
                {
                    claim.Value = dtoToken.TokenExp.ToString();
                }
                else if (claim.Type == GlobalEnums.OAuthClaims.scope && dtoToken.Scope != null && dtoToken.Scope != claim.Value)
                {
                    claim.Value = dtoToken.Scope;
                }
            }
        }

        private DatasetFileConfigDto MapToDatasetFileConfigDto(DatasetFileConfig dfc)
        {
            return new DatasetFileConfigDto
            {
                ConfigId = dfc.ConfigId,
                Name = dfc.Schema.Name,
                Description = dfc.Schema.Description,
                DatasetScopeTypeId = dfc.DatasetScopeType.ScopeTypeId,
                FileExtensionId = dfc.Schema.Extension.Id,
                FileExtensionName = dfc.Schema.Extension.Name,
                ParentDatasetId = dfc.ParentDataset.DatasetId,
                StorageCode = dfc.Schema.StorageCode,
                StorageLocation = Configuration.Config.GetHostSetting("S3DataPrefix") + dfc.GetStorageCode() + "\\",
                Security = _securityService.GetUserSecurity(null, _userService.GetCurrentUser()),
                CreateCurrentView = dfc.Schema != null && dfc.Schema.CreateCurrentView,
                Delimiter = dfc.Schema?.Delimiter,
                HasHeader = dfc.Schema != null && dfc.Schema.HasHeader,
                IsTrackableSchema = dfc.IsSchemaTracked,
                HiveTable = dfc.Schema?.HiveTable,
                HiveDatabase = dfc.Schema?.HiveDatabase,
                HiveLocation = dfc.Schema?.HiveLocation,
                HiveTableStatus = dfc.Schema?.HiveTableStatus,
                Schema = (dfc.Schema != null) ? _schemaService.GetFileSchemaDto(dfc.Schema.SchemaId) : null,
                DeleteInd = dfc.DeleteInd,
                DeleteIssuer = dfc.DeleteIssuer,
                DeleteIssueDTM = dfc.DeleteIssueDTM,
                ObjectStatus = dfc.ObjectStatus,
                SchemaRootPath = dfc.Schema?.SchemaRootPath,
                ParquetStorageBucket = dfc.Schema?.ParquetStorageBucket,
                ParquetStoragePrefix = dfc.Schema?.ParquetStoragePrefix,
                ConsumptionDetails = dfc.Schema?.ConsumptionDetails?.Select(c => c.Accept(new SchemaConsumptionDtoTransformer())).ToList(),
                ControlMTriggerName = dfc.Schema?.ControlMTriggerName,
                HasDataFlow = _datasetContext.DataFlow.Any(df => df.DatasetId == dfc.ParentDataset.DatasetId &&
                                                                     df.SchemaId == dfc.Schema.SchemaId &&
                                                                     df.ObjectStatus == GlobalEnums.ObjectStatusEnum.Active)
            };
        }

        public Tuple<List<RetrieverJob>, List<DataFlowStepDto>> GetDataFlowDropLocationJobs(DatasetFileConfig config)
        {
            Tuple<List<RetrieverJob>, List<DataFlowStepDto>> jobTuple;
            List<RetrieverJob> retrieverList = new List<RetrieverJob>();
            List<DataFlowStepDto> stepList = new List<DataFlowStepDto>();
            try
            {
                stepList = new List<DataFlowStepDto>() { _dataFlowService.GetS3DropStepForFileSchema(config.Schema) };                
                retrieverList.Add(_datasetContext.RetrieverJob.FirstOrDefault(w => w.DataFlow.Id == stepList.First().DataFlowId));
                jobTuple = new Tuple<List<RetrieverJob>, List<DataFlowStepDto>>(retrieverList, stepList);
            }
            catch (DataFlowStepNotFound)
            {
                jobTuple = new Tuple<List<RetrieverJob>, List<DataFlowStepDto>>(retrieverList, stepList);
            }

            return jobTuple;
        }

        /// <summary>
        /// Finds Schema Flow metadata associated with the provided <see cref="DatasetFileConfig"/>.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        /// <remarks>
        /// Schema Flows will stop being created as seperate flows as part of CLA-3332. However, this
        /// code is still needed for existing data flows - until they're converted.
        /// </remarks>
        public Tuple<DataFlowDetailDto, List<RetrieverJob>> GetDataFlowForSchema(DatasetFileConfig config)
        {
            
            ///Determine all SchemaMap steps which reference this schema
            SchemaMap schemaMap = _datasetContext.SchemaMap.FirstOrDefault(w => w.MappedSchema == config.Schema 
                                                                && w.DataFlowStepId.DataAction_Type_Id == DataActionType.SchemaLoad 
                                                                && w.DataFlowStepId.DataFlow.ObjectStatus == GlobalEnums.ObjectStatusEnum.Active
                                                                && w.DataFlowStepId.DataFlow.SchemaId == 0);
            DataFlowDetailDto dfDto = (schemaMap != null) ? _dataFlowService.GetDataFlowDetailDto(schemaMap.DataFlowStepId.DataFlow.Id) : null;

            List<RetrieverJob> rjList = new List<RetrieverJob>();
            if (dfDto != null)
            {
                rjList.AddRange(_datasetContext.RetrieverJob.Where(w => w.DataFlow.Id == dfDto.Id).ToList());
            }

            Tuple<DataFlowDetailDto, List<RetrieverJob>> schemaDataflow = new Tuple<DataFlowDetailDto, List<RetrieverJob>>(dfDto, rjList);

            return schemaDataflow;
        }

        public List<Tuple<DataFlowDetailDto, List<RetrieverJob>>> GetExternalDataFlowsBySchema(DatasetFileConfig config)
        {
            List<Tuple<DataFlowDetailDto, List<RetrieverJob>>> externalJobList = new List<Tuple<DataFlowDetailDto, List<RetrieverJob>>>();

            //NOTE: BRING BACK ACTIVE/DISABLED DATAFLOWS TO ALLOW USER TO REACTIVATE IF DESIRED 
            ///Determine all SchemaMap steps which reference this schema
            List<SchemaMap> schemaMappings = _datasetContext.SchemaMap.Where(w =>   w.MappedSchema == config.Schema && 
                                                                                    w.DataFlowStepId.DataAction_Type_Id == DataActionType.SchemaMap && 
                                                                                    (   w.DataFlowStepId.DataFlow.ObjectStatus == GlobalEnums.ObjectStatusEnum.Active ||
                                                                                        (   w.DataFlowStepId.DataFlow.ObjectStatus == GlobalEnums.ObjectStatusEnum.Disabled
                                                                                            && _featureFlags.CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL.GetValue()        //REMOVE THIS LINE WHEN REMOVING FEATURE FLAG
                                                                                        )
                                                                                    )
                                                                            ).ToList();

            
            
            //For each dataflow, get the detaildto object and associated retrieverjobs.  Create new tuple and add to return list.
            foreach (SchemaMap item in schemaMappings)
            {
                DataFlowDetailDto dfDto = _dataFlowService.GetDataFlowDetailDto(item.DataFlowStepId.DataFlow.Id);
                List<RetrieverJob> rjList = new List<RetrieverJob>();
                if (dfDto != null)
                {
                    rjList.AddRange(_datasetContext.RetrieverJob.Where(w => w.DataFlow.Id == dfDto.Id).ToList());
                }
                externalJobList.Add(new Tuple<DataFlowDetailDto, List<RetrieverJob>>(dfDto, rjList));
            }

            //NOTE: BRING BACK ACTIVE/DISABLED DATAFLOWS TO ALLOW USER TO REACTIVATE IF DESIRED 
            //Get DataFlow id which populates data to schema
            int schemaDataFlowId = _datasetContext.DataFlow.Where(  w => w.SchemaId == config.Schema.SchemaId && 
                                                                    (   w.ObjectStatus == GlobalEnums.ObjectStatusEnum.Active || 
                                                                        (   w.ObjectStatus == GlobalEnums.ObjectStatusEnum.Disabled
                                                                            && _featureFlags.CLA4433_SEND_S3_SINK_CONNECTOR_REQUEST_EMAIL.GetValue()        //REMOVE THIS LINE WHEN REMOVING FEATURE FLAG
                                                                        )
                                                                    )
                                                                  )
                                                            .Select(s => s.Id).FirstOrDefault();
            if (schemaDataFlowId != 0)
            {
                DataFlowDetailDto schemaFlowDto = _dataFlowService.GetDataFlowDetailDto(schemaDataFlowId);
                List<RetrieverJob> schemaFlowRetrieverJobList = new List<RetrieverJob>();
                if (schemaFlowDto != null)
                {
                    schemaFlowRetrieverJobList.AddRange(_datasetContext.RetrieverJob.Where(w => w.DataFlow.Id == schemaDataFlowId).ToList());
                }
                externalJobList.Add(new Tuple<DataFlowDetailDto, List<RetrieverJob>>(schemaFlowDto, schemaFlowRetrieverJobList));
            }

            return externalJobList;
        }

        public static Object TryConvertTo<T>(Object input)
        {
            Object result = null;
            try
            {
                result = Convert.ChangeType(input, typeof(T));
            }
            catch
            {
            }

            return result;
        }
        #endregion
    }
}
