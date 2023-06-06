using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry.Core;
using Sentry.data.Core.DependencyInjection;
using Sentry.data.Core.DomainServices;
using Sentry.data.Core.Entities;
using Sentry.data.Core.Helpers;
using Sentry.data.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core
{
    public class DatasetService : BaseDomainService<DatasetService>, IDatasetService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly ISecurityService _securityService;
        private readonly IUserService _userService;
        private readonly IConfigService _configService;
        private readonly ISchemaService _schemaService;
        private readonly IQuartermasterService _quartermasterService;
        private readonly ObjectCache cache = MemoryCache.Default;
        private readonly ISAIDService _saidService;
        private readonly IDatasetFileService _datasetFileService;
        private readonly IGlobalDatasetProvider _globalDatasetProvider;
        private readonly IMessagePublisher _messagePublisher;

        public DatasetService(IDatasetContext datasetContext, ISecurityService securityService, 
                            IUserService userService, IConfigService configService,
                            ISchemaService schemaService,
                            IQuartermasterService quartermasterService, ISAIDService saidService,
                            IDatasetFileService datasetFileService,
                            IGlobalDatasetProvider globalDatasetProvider,
                            IMessagePublisher messagePublisher,
                            DomainServiceCommonDependency<DatasetService> commonDependency) : base(commonDependency)
        {
            _datasetContext = datasetContext;
            _securityService = securityService;
            _userService = userService;
            _configService = configService;
            _schemaService = schemaService;
            _quartermasterService = quartermasterService;
            _saidService = saidService;
            _datasetFileService = datasetFileService;
            _globalDatasetProvider = globalDatasetProvider;
            _messagePublisher = messagePublisher;
        }

        public DatasetSchemaDto GetDatasetSchemaDto(int id)
        {
            Dataset ds = _datasetContext.Datasets.Where(x => x.DatasetId == id && x.CanDisplay).FetchAllChildren(_datasetContext).FirstOrDefault();
            DatasetSchemaDto dto = new DatasetSchemaDto();
            MapToDto(ds, dto);

            return dto;
        }

        public DatasetDto GetDatasetDto(int id)
        {
            Dataset ds = _datasetContext.Datasets.Where(x => x.DatasetId == id && x.CanDisplay).FetchAllChildren(_datasetContext).FirstOrDefault();
            DatasetDto dto = new DatasetDto();
            MapToDto(ds, dto);

            return dto;
        }

        public DatasetDetailDto GetDatasetDetailDto(int id)
        {
            Dataset ds = _datasetContext.Datasets.Where(x => x.DatasetId == id && x.CanDisplay).FetchAllChildren(_datasetContext).FirstOrDefault();

            if (ds != null)
            {
                DatasetDetailDto dto = new DatasetDetailDto();
                MapToDetailDto(ds, dto);
                return dto;
            }
            else
            {
                return null;
            }            
        }

        public List<DatasetSummaryMetadataDTO> GetDatasetSummaryMetadataDTO()
        {
            List<DatasetSummaryMetadataDTO> summaryResults = cache["DatasetSummaryMetadata"] as List<DatasetSummaryMetadataDTO>;

            if (summaryResults == null)
            {
                //Create cache policy and set expiration policy based on config
                CacheItemPolicy policy = new CacheItemPolicy();

                //SlidingExpriration will restart the experation timer when the cache is accessed, if it has not already expired.
                policy.AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddHours(24));

                //Pull summarized metadata from datasetfile table
                summaryResults = _datasetContext.DatasetFileStatusActive.GroupBy(g => new { g.Dataset })
                .Select(s => new DatasetSummaryMetadataDTO
                {
                    DatasetId = s.Key.Dataset.DatasetId,
                    FileCount = s.Count(),
                    Max_Created_DTM = s.Max(m => m.CreatedDTM)
                }).ToList();

                //pull summarized metadata from events table
                var eventlist = _datasetContext.Events
                    .Where(x => x.EventType.Description == GlobalConstants.EventType.VIEWED && x.Dataset.HasValue)
                    .GroupBy(g => g.Dataset)
                    .Select(s => new { ds_id = s.Key, count = s.Count() })
                    .OrderBy(o => o.ds_id).ToList();

                //Add events metadata to summaryResults metadata
                foreach (DatasetSummaryMetadataDTO summary in summaryResults)
                {
                    summary.ViewCount = (eventlist.Any(w => w.ds_id == summary.DatasetId)) ? eventlist.First(w => w.ds_id == summary.DatasetId).count : 0;
                }

                //Assign result list to cache object
                cache.Set("DatasetSummaryMetadata", summaryResults, policy);
            }

            return summaryResults;
        }
        private List<DatasetSchemaDto> GetDatasetDtos(bool active)
        {
            IQueryable<Dataset> datasetQueryable = _datasetContext.Datasets.Where(x => x.CanDisplay && x.DatasetType == "DS");
            if (active)
            {
                datasetQueryable = datasetQueryable.Where(x => x.ObjectStatus == GlobalEnums.ObjectStatusEnum.Active);
            }

                List<Dataset> dsList = datasetQueryable.FetchAllChildren(_datasetContext).ToList();

                List<DatasetSchemaDto> dtoList = new List<DatasetSchemaDto>();
                foreach (Dataset ds in dsList)
                {
                    DatasetSchemaDto dto = new DatasetSchemaDto();
                    MapToDto(ds, dto);
                    dtoList.Add(dto);
                }
                return dtoList;
        }
        public List<DatasetSchemaDto> GetAllDatasetDto()
        {
            return GetDatasetDtos(false);
        } 
        public List<DatasetSchemaDto> GetAllActiveDatasetDto()
        {
            return GetDatasetDtos(true);
        }

        public IDictionary<int, string> GetDatasetList()
        {
            IDictionary<int, string> datasetList = _datasetContext.Datasets
                .Where(w => w.ObjectStatus == GlobalEnums.ObjectStatusEnum.Active)
                .Select(s => new { s.DatasetId, s.DatasetName })
                .ToDictionary(d => d.DatasetId, d => d.DatasetName);

            return datasetList;
        }

        public List<string> GetDatasetNamesForAsset(string asset)
        {
            return _datasetContext.Datasets.Where(ds => ds.Asset.SaidKeyCode.Equals(asset)).Select(ds => ds.DatasetName).ToList();
        }

        public List<string> GetInheritanceEnabledDatasetNamesForAsset(string asset)
        {
            return _datasetContext.Datasets.Where(ds => ds.Asset.SaidKeyCode.Equals(asset) && ds.Security.Tickets.Any(t => t.AddedPermissions.Any(p => p.Permission.PermissionCode == PermissionCodes.INHERIT_PARENT_PERMISSIONS && p.IsEnabled))).Select(ds => ds.DatasetName).ToList();
        }

        public List<Dataset> GetInheritanceEnabledDatasetsForAsset(string asset)
        {
            return _datasetContext.Datasets.Where(ds => ds.Asset.SaidKeyCode.Equals(asset) && ds.Security.Tickets.Any(t => t.AddedPermissions.Any(p => p.Permission.PermissionCode == PermissionCodes.INHERIT_PARENT_PERMISSIONS && p.IsEnabled))).ToList();
        }

        public UserSecurity GetUserSecurityForDataset(int datasetId)
        {
            Dataset ds = _datasetContext.Datasets.Where(x => x.DatasetId == datasetId && x.CanDisplay).FetchSecurityTree(_datasetContext).FirstOrDefault();
            return _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());
        }

        /// <summary>
        /// Retrieve all the permissions granted to the dataset with the given <paramref name="datasetId"/>.
        /// </summary>
        public DatasetPermissionsDto GetDatasetPermissions(int datasetId)
        {
            var result = new DatasetPermissionsDto();
            Dataset ds = _datasetContext.Datasets.Where(x => x.DatasetId == datasetId && x.CanDisplay).FetchSecurityTree(_datasetContext).FirstOrDefault();
            result.DatasetId = ds.DatasetId;
            result.DatasetName = ds.DatasetName;
            result.DatasetNamedEnvironment = ds.NamedEnvironment;
            result.DatasetSaidKeyCode = ds.Asset.SaidKeyCode;
            result.Permissions = _securityService.GetSecurablePermissions(ds);
            result.Approvers = _saidService.GetApproversByKeyCodeAsync(ds.Asset.SaidKeyCode).Result;
            result.InheritanceTicket = _securityService.GetSecurableInheritanceTicket(ds);
            return result;
        }

        public SecurityTicket GetLatestInheritanceTicket(int datasetId)
        {
            return _securityService.GetSecurableInheritanceTicket(_datasetContext.Datasets.Where(x => x.DatasetId == datasetId && x.CanDisplay).FetchSecurityTree(_datasetContext).LastOrDefault());
        }

        public UserSecurity GetUserSecurityForConfig(int configId)
        {
            DatasetFileConfig dfc = _datasetContext.DatasetFileConfigs.Where(x => x.ConfigId == configId).FetchSecurityTree(_datasetContext).FirstOrDefault();
            if (dfc.ParentDataset != null)
            {
                return _securityService.GetUserSecurity(dfc.ParentDataset, _userService.GetCurrentUser());
            }
            return new UserSecurity();
        }

        public List<Dataset> GetDatasetsForQueryTool()
        {
            //get all datasets where the there is a CanQueryData permission on the security
            //OR all public datasets (no security object)
            var query = _datasetContext.Datasets.Where(x => x.DatasetType == DataEntityCodes.DATASET && x.CanDisplay);

            query.FetchMany(x => x.DatasetCategories).ToFuture();
            List<Dataset> datasets = query.FetchSecurityTree(_datasetContext);

            IApplicationUser user = _userService.GetCurrentUser();

            List<Dataset> datasetsCanQuery = new List<Dataset>();

            foreach (Dataset ds in datasets)
            {
                var userSecurity = _securityService.GetUserSecurity(ds, user);
                if (userSecurity.CanQueryDataset)
                {
                    datasetsCanQuery.Add(ds);
                }
            }

            //now add in all the public datasets.

            return datasetsCanQuery;
        }

        public async Task<AccessRequest> GetAccessRequestAsync(int datasetId)
        {
            Dataset ds = _datasetContext.GetById<Dataset>(datasetId);

            var perms = GetDatasetPermissions(ds.DatasetId);

            AccessRequest ar = new AccessRequest()
            {
                ApproverList = new List<KeyValuePair<string, string>>(),
                SecurableObjectId = ds.DatasetId,
                SecurableObjectName = ds.DatasetName,
                SaidKeyCode = ds.Asset.SaidKeyCode,
            };

            if (perms != null && perms.InheritanceTicket != null)
            {
                var inheritancePerms = perms.InheritanceTicket.AddedPermissions.FirstOrDefault();
                if(inheritancePerms != null)
                {
                    ar.InheritanceStatus = inheritancePerms.IsEnabled;
                }
            }

            //determine the names of the default security groups
            var securityGroups = _securityService.GetDefaultSecurityGroupDtos(ds);
            ar.ConsumeDatasetGroupName = securityGroups.First(g => !g.IsAssetLevelGroup() && g.GroupType == DTO.Security.AdSecurityGroupType.Cnsmr).GetGroupName();
            ar.ProducerDatasetGroupName = securityGroups.First(g => !g.IsAssetLevelGroup() && g.GroupType == DTO.Security.AdSecurityGroupType.Prdcr).GetGroupName();
            ar.ConsumeAssetGroupName = securityGroups.First(g => g.IsAssetLevelGroup() && g.GroupType == DTO.Security.AdSecurityGroupType.Cnsmr).GetGroupName();
            ar.ProducerAssetGroupName = securityGroups.First(g => g.IsAssetLevelGroup() && g.GroupType == DTO.Security.AdSecurityGroupType.Prdcr).GetGroupName();

            //Set permission list based on if Dataset is secured (restricted)
            ar.Permissions = !ds.IsSecured
                ? _datasetContext.Permission.Where(x => x.SecurableObject == SecurableEntityName.DATASET && x.PermissionCode == PermissionCodes.CAN_MANAGE_SCHEMA).ToList()
                : _datasetContext.Permission.Where(x => x.SecurableObject == SecurableEntityName.DATASET).ToList();

            List<SAIDRole> prodCusts = await _saidService.GetApproversByKeyCodeAsync(ds.Asset.SaidKeyCode).ConfigureAwait(false);
            foreach(SAIDRole prodCust in prodCusts)
            {
                ar.ApproverList.Add(new KeyValuePair<string, string>(prodCust.AssociateId, prodCust.Name));
            }

            return ar;
        }

        public async Task<string> RequestAccessToDataset(AccessRequest request)
        {

            Dataset ds = _datasetContext.GetById<Dataset>(request.SecurableObjectId);
            if (ds != null)
            {
                IApplicationUser user = _userService.GetCurrentUser();
                
                request.SecurableObjectName = request.Scope == AccessScope.Asset ? ds.Asset.SaidKeyCode : ds.DatasetName;
                request.SecurableObjectNamedEnvironment = request.Scope == AccessScope.Asset ? null : ds.NamedEnvironment;
                request.SecurableObjectId = request.Scope == AccessScope.Asset ? ds.Asset.AssetId : request.SecurableObjectId;
                request.SecurityId = ds.Security.SecurityId;
                request.SaidKeyCode = ds.Asset.SaidKeyCode;
                request.RequestorsId = user.AssociateId;
                request.RequestorsName = user.DisplayName;
                request.IsProd = bool.Parse(Configuration.Config.GetHostSetting("RequireApprovalHPSMTickets"));
                request.RequestedDate = DateTime.Now;
                request.ApproverId = request.SelectedApprover;
                request.Permissions = _datasetContext.Permission.Where(x => request.SelectedPermissionCodes.Contains(x.PermissionCode) &&
                                                                                                                x.SecurableObject == SecurableEntityName.DATASET).ToList();
                request = BuildPermissionsForRequestType(request);
                return await _securityService.RequestPermission(request);
            }

            return string.Empty;
        }

        public async Task<string> RequestAccessRemoval(AccessRequest request)
        {
            Dataset ds = _datasetContext.GetById<Dataset>(request.SecurableObjectId);
            IApplicationUser user = _userService.GetCurrentUser();
            var security = _datasetContext.Security.Where(s => s.Tickets.Any(t => t.TicketId == request.TicketId)).FirstOrDefault();

            request.SecurableObjectName = request.Scope == AccessScope.Asset ? ds.Asset.SaidKeyCode : ds.DatasetName;
            request.SecurableObjectNamedEnvironment = request.Scope == AccessScope.Asset ? null : ds.NamedEnvironment;
            request.SecurableObjectId = request.Scope == AccessScope.Asset ? ds.Asset.AssetId : request.SecurableObjectId;
            if (security != null)
            {
                request.SecurityId = security.SecurityId;
            }
            request.RequestorsId = user.AssociateId;
            request.RequestorsName = user.DisplayName;
            request.IsProd = bool.Parse(Configuration.Config.GetHostSetting("RequireApprovalHPSMTickets"));
            request.RequestedDate = DateTime.Now;
            request.ApproverId = request.SelectedApprover;
            request.Permissions = new List<Permission>();
            request = BuildPermissionsForRequestType(request);
            return await _securityService.RequestPermission(request);
        }

        public AccessRequest BuildPermissionsForRequestType(AccessRequest request)
        {
            switch (request.Type)
            {
                case AccessRequestType.AwsArn:
                    request.Permissions.Add(_datasetContext.Permission.Where(x => x.PermissionCode == PermissionCodes.S3_ACCESS).First());
                    break;
                case AccessRequestType.SnowflakeAccount:
                    request.Permissions.Add(_datasetContext.Permission.Where(x => x.PermissionCode == PermissionCodes.SNOWFLAKE_ACCESS).First());
                    break;
                case AccessRequestType.RemovePermission:
                    request.Permissions.Add(_datasetContext.Permission.Where(x => request.SelectedPermissionCodes.Contains(x.PermissionCode) && x.SecurableObject == SecurableEntityName.DATASET).FirstOrDefault());
                    break;
                default:
                    break;
            }
            return request;
        }

        public async Task<DatasetResultDto> AddDatasetAsync(DatasetDto datasetDto)
        {
            IApplicationUser user = _userService.GetCurrentUser();
            UserSecurity security = _securityService.GetUserSecurity(null, user);

            if (security.CanCreateDataset)
            {
                datasetDto.UploadUserId = user.AssociateId;

                Dataset dataset = CreateDataset(datasetDto);

                await _datasetContext.AddAsync(dataset);
                await _datasetContext.SaveChangesAsync();

                GenerateSnowSchemaEventForDataset(dataset, false);

                // Create a Hangfire job that will setup the default security groups for this new dataset
                _securityService.EnqueueCreateDefaultSecurityForDataset(dataset.DatasetId);

                if (_dataFeatures.CLA4789_ImprovedSearchCapability.GetValue())
                {
                    GlobalDataset globalDataset = dataset.ToGlobalDataset();
                    await _globalDatasetProvider.AddUpdateGlobalDatasetAsync(globalDataset);
                }

                DatasetResultDto resultDto = dataset.ToDatasetResultDto();
                return resultDto;
            }
            else
            {
                throw new ResourceForbiddenException(user.AssociateId, nameof(security.CanCreateDataset), "AddDataset");
            }
        }

        public async Task<DatasetDto> GetDatasetAsync(int id)
        {
            Dataset dataset = await _datasetContext.GetByIdAsync<Dataset>(id);
            if(dataset != null)
            {
                var dto = new DatasetDto();
                MapToDto(dataset, dto);
                return dto;
            }
            throw new ResourceNotFoundException("GetDataset", id);
        }

        public int Create(DatasetDto dto)
        {
            Dataset ds = CreateDataset(dto);
            _datasetContext.Add(ds);
            return ds.DatasetId;
        }

        public void CreateExternalDependencies(int datasetId)
        {
            Dataset dataset = _datasetContext.GetById<Dataset>(datasetId);

            // Publish a message to create the Schema in Snowflake 
            GenerateSnowSchemaEventForDataset(dataset, false);

            // Create a Hangfire job that will setup the default security groups for this new dataset
            _securityService.EnqueueCreateDefaultSecurityForDataset(datasetId);


            if (_dataFeatures.CLA4789_ImprovedSearchCapability.GetValue())
            {
                //Coming from migration, only have to add environment dataset (global dataset should exist)
                EnvironmentDataset environmentDataset = dataset.ToEnvironmentDataset();
                _globalDatasetProvider.AddUpdateEnvironmentDatasetAsync(dataset.GlobalDatasetId.Value, environmentDataset).Wait();
            }
        }

        public int CreateAndSaveNewDataset(DatasetSchemaDto dto)
        {
            Dataset ds = CreateDataset(dto);
            _datasetContext.Add(ds);
            dto.DatasetId = ds.DatasetId;

            DatasetFileConfigDto configDto = dto.ToConfigDto();
            FileSchemaDto fileDto = dto.ToSchemaDto();
            
            configDto.SchemaId = _schemaService.CreateAndSaveSchema(fileDto);
            _configService.CreateAndSaveDatasetFileConfig(configDto);
            _schemaService.PublishSchemaEvent(dto.DatasetId, configDto.SchemaId);
            _datasetContext.SaveChanges();

            GenerateSnowSchemaEventForDataset(ds, false);

            // Create a Hangfire job that will setup the default security groups for this new dataset
            _securityService.EnqueueCreateDefaultSecurityForDataset(ds.DatasetId);

            if (_dataFeatures.CLA4789_ImprovedSearchCapability.GetValue())
            {
                GlobalDataset globalDataset = ds.ToGlobalDataset();
                fileDto.SchemaId = configDto.SchemaId;
                EnvironmentSchema environmentSchema = fileDto.ToEnvironmentSchema();
                globalDataset.EnvironmentDatasets.First().EnvironmentSchemas.Add(environmentSchema);

                _globalDatasetProvider.AddUpdateGlobalDatasetAsync(globalDataset).Wait();
            }

            return ds.DatasetId;
        }

        public async Task<DatasetResultDto> UpdateDatasetAsync(DatasetDto dto)
        {
            Dataset ds = _datasetContext.GetById<Dataset>(dto.DatasetId);

            //check exists
            if (ds != null)
            {
                //check permissions
                IApplicationUser user = _userService.GetCurrentUser();
                UserSecurity security = _securityService.GetUserSecurity(ds, user);

                if (security.CanEditDataset)
                {
                    //update
                    UpdateDataset(dto, ds);

                    //save
                    await _datasetContext.SaveChangesAsync();

                    await UpdateEnvironmentDatasetAsync(ds);

                    //map to result
                    DatasetResultDto resultDto = ds.ToDatasetResultDto();

                    //return
                    return resultDto;
                }
                else
                {
                    throw new ResourceForbiddenException(user.AssociateId, nameof(security.CanEditDataset), "UpdateDataset", dto.DatasetId);
                }
            }
            else
            {
                throw new ResourceNotFoundException("UpdateDataset", dto.DatasetId);
            }
        }

        public void UpdateAndSaveDataset(DatasetSchemaDto dto)
        {
            Dataset ds = _datasetContext.GetById<Dataset>(dto.DatasetId);

            //Verify that certain idempotent fields have not been changed
            ValidateIdempotentFields(dto, ds);

            UpdateDataset(dto, ds);

            _datasetContext.SaveChanges();

            UpdateEnvironmentDatasetAsync(ds).Wait();
        }

        private async Task UpdateEnvironmentDatasetAsync(Dataset dataset)
        {
            if (_dataFeatures.CLA4789_ImprovedSearchCapability.GetValue())
            {
                EnvironmentDataset environmentDataset = dataset.ToEnvironmentDataset();
                await _globalDatasetProvider.AddUpdateEnvironmentDatasetAsync(dataset.GlobalDatasetId.Value, environmentDataset).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Verify that certain idempotent fields have not been changed
        /// </summary>
        private static void ValidateIdempotentFields(DatasetSchemaDto dto, Dataset ds)
        {
            if (ds.DatasetName != dto.DatasetName)
            {
                throw new ValidationException(ValidationErrors.NAME_IS_IDEMPOTENT, "Dataset Name cannot be changed");
            }
            if (ds.ShortName != dto.ShortName)
            {
                throw new ValidationException(Dataset.ValidationErrors.datasetShortNameIdempotent, "Dataset Short Name cannot be changed");
            }
            if (ds.Asset.SaidKeyCode != dto.SAIDAssetKeyCode)
            {
                throw new ValidationException(ValidationErrors.SAID_ASSET_IDEMPOTENT, "Dataset Asset cannot be changed");
            }
            if (ds.NamedEnvironment != dto.NamedEnvironment)
            {
                throw new ValidationException(ValidationErrors.NAMED_ENVIRONMENT_IDEMPOTENT, "Dataset Named Environment cannot be changed");
            }
            if (ds.NamedEnvironmentType != dto.NamedEnvironmentType)
            {
                throw new ValidationException(ValidationErrors.NAMED_ENVIRONMENT_TYPE_IDEMPOTENT, "Dataset Named Environment Type cannot be changed");
            }
        }

        private void UpdateDataset(DatasetDto dto, Dataset ds)
        {
            ds.DatasetInformation = dto.DatasetInformation;
            ds.OriginationCode = Enum.GetName(typeof(DatasetOriginationCode), dto.OriginationId);
            ds.ChangedDtm = DateTime.Now;

            if (dto.DatasetCategoryIds?.Count() > 0)
            {
                ds.DatasetCategories = _datasetContext.Categories.Where(x => dto.DatasetCategoryIds.Contains(x.Id)).ToList();
            }
            else if (!string.IsNullOrEmpty(dto.CategoryName))
            {
                ds.DatasetCategories = _datasetContext.Categories.Where(x => x.Name.ToLower() == dto.CategoryName.ToLower()).ToList();
            }

            if (null != dto.CreationUserId && dto.CreationUserId.Length > 0)
            {
                ds.CreationUserName = dto.CreationUserId;
            }

            if (null != dto.DatasetDesc && dto.DatasetDesc.Length > 0)
            {
                ds.DatasetDesc = dto.DatasetDesc;
            }

            if (null != dto.PrimaryContactId && dto.PrimaryContactId.Length > 0)
            {
                ds.PrimaryContactId = dto.PrimaryContactId;
            }

            if (dto.DataClassification > 0)
            {
                ds.DataClassification = dto.DataClassification;
            }

            string userId = _userService.GetCurrentUser().AssociateId;

            if (ds.Security == null)
            {
                ds.Security = new Security(SecurableEntityName.DATASET)
                {
                    CreatedById = userId
                };
            }

            SetIsSecuredByDataClassification(dto);

            if (!ds.IsSecured && dto.IsSecured)
            {
                ds.Security.EnabledDate = DateTime.Now;
                ds.Security.UpdatedById = userId;
            }
            else if (ds.IsSecured && !dto.IsSecured)
            {
                ds.Security.RemovedDate = DateTime.Now;
                ds.Security.UpdatedById = userId;
            }

            ds.IsSecured = dto.IsSecured;
            ds.AlternateContactEmail = dto.AlternateContactEmail;
        }

        public bool Delete(int id, IApplicationUser user, bool logicalDelete)
        {
            string methodName = $"{nameof(DatasetService).ToLower()}_{nameof(Delete).ToLower()}";
            _logger.LogDebug($"{methodName} Method Start");

            bool result = true;
            Dataset ds = _datasetContext.GetById<Dataset>(id);

            bool HasCorrectStatus = false;

            if (
                (logicalDelete && (ds.ObjectStatus == GlobalEnums.ObjectStatusEnum.Pending_Delete ||
                                   ds.ObjectStatus == GlobalEnums.ObjectStatusEnum.Deleted)
                                   ) ||
                (!logicalDelete && ds.ObjectStatus == GlobalEnums.ObjectStatusEnum.Deleted))
            {
                HasCorrectStatus = true;
            }

            if (HasCorrectStatus)
            {
                return result;
            }


            if (logicalDelete)
            {
                _logger.LogInformation($"datasetservice-delete-logical - datasetid:{ds.DatasetId} datasetname:{ds.DatasetName}");

                try
                {
                    //Mark dataset for soft delete
                    MarkForDelete(ds, user);

                    if (_dataFeatures.CLA4789_ImprovedSearchCapability.GetValue())
                    {
                        _globalDatasetProvider.DeleteEnvironmentDatasetAsync(ds.DatasetId).Wait();
                    }

                    ////Mark Configs for soft delete to ensure no editing and jobs are disabled
                    foreach (DatasetFileConfig config in ds.DatasetFileConfigs)
                    {
                        _configService.Delete(config.ConfigId, user ?? _userService.GetCurrentUser(), logicalDelete);
                        DeleteDatasetFiles(ds.DatasetId, config.Schema.SchemaId);       //DELETE DATASETFILES UNDER SCHEMA
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"datasetservice-delete-logical failed");
                    result = false;
                }
            }
            else
            {
                _logger.LogInformation($"datasetservice-delete-physical - datasetid:{ds.DatasetId} datasetname:{ds.DatasetName}");

                try
                {
                    foreach (DatasetFileConfig config in ds.DatasetFileConfigs)
                    {
                        _configService.Delete(config.ConfigId, user, logicalDelete);
                    }

                    ds.ObjectStatus = GlobalEnums.ObjectStatusEnum.Deleted;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"datasetservice-delete failed");
                    result = false;
                }                    
            }

            _logger.LogDebug($"{methodName} Method End");

            return result;
        }       

        public async Task<ValidationException> ValidateAsync(DatasetSchemaDto dto)
        {
            ValidationResults results = new ValidationResults();

            ValidateDatasetName(dto, results);

            ValidateDatasetShortName(dto, results);

            if (String.IsNullOrWhiteSpace(dto.PrimaryContactId))
            {
                results.Add(Dataset.ValidationErrors.datasetContactRequired, "Contact is required.");
            }

            ValidateDatasetCategories(dto, results);

            if (dto.DatasetId == 0 && dto.DatasetScopeTypeId == 0)
            {
                results.Add(Dataset.ValidationErrors.datasetScopeRequired, "Dataset Scope is required");
            }

            if (String.IsNullOrWhiteSpace(dto.SAIDAssetKeyCode))
            {
                results.Add(ValidationErrors.SAID_ASSET_REQUIRED, "SAID Asset is required.");
            }

            if (dto.OriginationId == 0)
            {
                results.Add(Dataset.ValidationErrors.datasetOriginationRequired, "Dataset Origination is required");
            }

            //Validate the Named Environment selection using the QuartermasterService
            results.MergeInResults(await _quartermasterService.VerifyNamedEnvironmentAsync(dto.SAIDAssetKeyCode, dto.NamedEnvironment, dto.NamedEnvironmentType));

            //VALIDATE EMAIL ADDRESS
            if (!ValidationHelper.IsDSCEmailValid(dto.AlternateContactEmail))
            {
                results.Add(Dataset.ValidationErrors.datasetAlternateContactEmailFormatInvalid, "Alternate Contact Email must be valid sentry.com email address");
            }

            return new ValidationException(results);
        }

        public List<Dataset> GetDatasetMarkedDeleted()
        {
            List<Dataset> dsList = _datasetContext.Datasets.Where(w => w.DeleteInd && w.DeleteIssueDTM < DateTime.Now.AddDays(Double.Parse(Configuration.Config.GetHostSetting("DatasetDeleteWaitDays")))).ToList();
            return dsList;
        }

        public string SetDatasetFavorite(int datasetId, string associateId, bool removeForAllEnvironments)
        {
            try
            {
                Dataset ds = _datasetContext.GetById<Dataset>(datasetId);
                Favorite dsFavorite = ds.Favorities.FirstOrDefault(x => x.UserId == associateId);

                if (dsFavorite == null && !removeForAllEnvironments)
                {
                    Favorite f = new Favorite()
                    {
                        DatasetId = ds.DatasetId,
                        UserId = associateId,
                        Created = DateTime.Now
                    };

                    _datasetContext.Merge(f);
                    
                    if (_dataFeatures.CLA4789_ImprovedSearchCapability.GetValue())
                    {
                        _globalDatasetProvider.AddEnvironmentDatasetFavoriteUserIdAsync(datasetId, associateId).Wait();
                    }

                    _datasetContext.SaveChanges();

                    return "Successfully added favorite.";
                }
                else
                {
                    List<Favorite> favorites;
                    if (removeForAllEnvironments)
                    {
                        List<int> datasetIds = _datasetContext.Datasets.Where(x => x.GlobalDatasetId == ds.GlobalDatasetId).Select(x => x.DatasetId).ToList();
                        favorites = _datasetContext.Favorites.Where(x => datasetIds.Contains(x.DatasetId) && x.UserId == associateId).ToList();
                    }
                    else
                    {
                        favorites = new List<Favorite> { dsFavorite };
                    }

                    foreach (Favorite favorite in favorites)
                    {
                        _datasetContext.Remove(favorite);
                    }

                    if (_dataFeatures.CLA4789_ImprovedSearchCapability.GetValue())
                    {
                        _globalDatasetProvider.RemoveEnvironmentDatasetFavoriteUserIdAsync(datasetId, associateId, removeForAllEnvironments).Wait();
                    }

                    _datasetContext.SaveChanges();

                    return "Successfully removed favorite.";
                }
            }
            catch (Exception)
            {
                _datasetContext.Clear();
                throw;
            }
        }

        public IQueryable<DatasetFile> GetDatasetFileTableQueryable(int configId)
        {
            return _datasetContext.DatasetFileStatusActive.Where(x => x.DatasetFileConfig.ConfigId == configId && 
                                                                      x.ParentDatasetFileId == null &&
                                                                      !x.IsBundled);
        }

        public (int targetDatasetId, bool datasetExistsInTarget) DatasetExistsInTargetNamedEnvironment(string datasetName, string saidAssetKey, string targetNamedEnvironment)
        {
            if (string.IsNullOrWhiteSpace(datasetName))
            {
                throw new ArgumentNullException(nameof(datasetName));
            }
            if (string.IsNullOrWhiteSpace(saidAssetKey))
            {
                throw new ArgumentNullException(nameof(saidAssetKey));
            }
            if (string.IsNullOrWhiteSpace(targetNamedEnvironment))
            {
                throw new ArgumentNullException(nameof(targetNamedEnvironment));
            }

            int datasetId = _datasetContext.Datasets.Where(w => w.DatasetName == datasetName && w.Asset.SaidKeyCode == saidAssetKey && w.NamedEnvironment == targetNamedEnvironment && w.ObjectStatus == GlobalEnums.ObjectStatusEnum.Active).Select(s => s.DatasetId).FirstOrDefault();
            
            return (datasetId, (datasetId != 0));
        }

        #region "private functions"
        private static void ValidateDatasetCategories(DatasetSchemaDto dto, ValidationResults results)
        {
            if (dto.DatasetCategoryIds == null)
            {
                results.Add(Dataset.ValidationErrors.datasetCategoryRequired, "Category is required");
            }
            else
            {
                if (dto.DatasetCategoryIds.Count == 1 && dto.DatasetCategoryIds[0].Equals(0))
                {
                    results.Add(Dataset.ValidationErrors.datasetCategoryRequired, "Category is required");
                }
            }
        }

        private void ValidateDatasetName(DatasetSchemaDto dto, ValidationResults results)
        {
            if (string.IsNullOrEmpty(dto.DatasetName)) //if no name, add error
            {
                results.Add(Dataset.ValidationErrors.datasetNameRequired, "Dataset Name is required");
            }
            else //if name, make sure it is not duplicate
            {
                if (dto.DatasetId == 0 && dto.DatasetCategoryIds != null)
                {
                    Dataset existing = _datasetContext.Datasets.FirstOrDefault(w => w.DatasetName == dto.DatasetName && w.DatasetType == DataEntityCodes.DATASET);

                    if (existing != null)
                    {
                        if (_dataFeatures.CLA1797_DatasetSchemaMigration.GetValue())
                        {
                            results.Add(Dataset.ValidationErrors.datasetNameDuplicate, "Dataset name already exists. If attempting to create a copy of an existing dataset in a different named environment, please use dataset migration.");
                        }
                        else if (existing.NamedEnvironment == dto.NamedEnvironment)
                        {
                            results.Add(Dataset.ValidationErrors.datasetNameDuplicate, "Dataset name already exists for that named environment");
                        }
                    }
                }
            }
        }

        private void ValidateDatasetShortName(DatasetSchemaDto dto, ValidationResults results)
        {
            if (string.IsNullOrWhiteSpace(dto.ShortName))
            {
                results.Add(Dataset.ValidationErrors.datasetShortNameRequired, "Short Name is required");
            }
            else
            {
                if (new Regex(@"[^0-9a-zA-Z]").Match(dto.ShortName).Success)
                {
                    results.Add(Dataset.ValidationErrors.datasetShortNameInvalid, "Short Name can only contain alphanumeric characters");
                }
                if (dto.ShortName.Length > 12)
                {
                    results.Add(Dataset.ValidationErrors.datasetShortNameInvalid, "Short Name must be 12 characters or less");
                }
                if (dto.ShortName == SecurityConstants.ASSET_LEVEL_GROUP_NAME)
                {
                    results.Add(Dataset.ValidationErrors.datasetShortNameInvalid, $"Short Name cannot be \"{SecurityConstants.ASSET_LEVEL_GROUP_NAME}\"");
                }
                if (_datasetContext.Datasets.Any(d => d.ShortName == dto.ShortName && 
                    d.DatasetType == DataEntityCodes.DATASET && d.NamedEnvironment == dto.NamedEnvironment && dto.DatasetId != d.DatasetId))
                {
                    results.Add(Dataset.ValidationErrors.datasetShortNameDuplicate, "Short Name is already in use by another Dataset in that named environment");
                }
            }
        }

        private void DeleteDatasetFiles(int datasetId, int schemaId)
        {
            //DO NOT DELETE IF FEATURE IS OFF
            if (!_dataFeatures.CLA4049_ALLOW_S3_FILES_DELETE.GetValue())
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

        private void MarkForDelete(Dataset ds, IApplicationUser user)
        {
            GenerateSnowSchemaEventForDataset(ds, true);

            ds.CanDisplay = false;
            ds.DeleteInd = true;
            ds.DeleteIssuer = (user == null)? _userService.GetCurrentUser().AssociateId : user.AssociateId;
            ds.DeleteIssueDTM = DateTime.Now;
            ds.ObjectStatus = GlobalEnums.ObjectStatusEnum.Pending_Delete;
        }

        private Dataset CreateDataset(DatasetDto dto)
        {
            Asset asset = GetAsset(dto.SAIDAssetKeyCode);

            if (!dto.GlobalDatasetId.HasValue || dto.GlobalDatasetId == 0)
            {
                dto.GlobalDatasetId = _datasetContext.GetNextGlobalDatasetId();
            }

            Dataset ds = new Dataset()
            {
                DatasetId = dto.DatasetId,
                DatasetName = dto.DatasetName,
                ShortName = dto.ShortName,
                DatasetDesc = dto.DatasetDesc,
                DatasetInformation = dto.DatasetInformation,
                CreationUserName = dto.CreationUserId,
                PrimaryContactId = dto.PrimaryContactId,
                UploadUserName = dto.UploadUserId,
                OriginationCode = Enum.GetName(typeof(DatasetOriginationCode), dto.OriginationId),
                DatasetDtm = dto.DatasetDtm,
                ChangedDtm = dto.ChangedDtm,
                DatasetType = DataEntityCodes.DATASET,
                DataClassification = dto.DataClassification,
                CanDisplay = true,
                DatasetFiles = null,
                DatasetFileConfigs = null,
                DeleteInd = false,
                DeleteIssueDTM = DateTime.MaxValue,
                ObjectStatus = GlobalEnums.ObjectStatusEnum.Active,
                Asset = asset,
                NamedEnvironment = dto.NamedEnvironment,
                NamedEnvironmentType = dto.NamedEnvironmentType,
                AlternateContactEmail = dto.AlternateContactEmail,
                GlobalDatasetId = dto.GlobalDatasetId
            };

            if (dto.DatasetCategoryIds?.Any() == true)
            {
                ds.DatasetCategories = _datasetContext.Categories.Where(x => x.Id == dto.DatasetCategoryIds.First()).ToList();
            }
            else if (!string.IsNullOrEmpty(dto.CategoryName))
            {
                ds.DatasetCategories = _datasetContext.Categories.Where(x => x.Name.ToLower() == dto.CategoryName.ToLower()).ToList();
            }

            SetIsSecuredByDataClassification(dto);
            ds.IsSecured = dto.IsSecured;

            //All datasets get a Security entry regardless if restricted
            //  this allows security process for internally managed permissions
            //  which do not require dataset to be restricted (i.e. CanManageSchema).
            ds.Security = new Security(SecurableEntityName.DATASET)
            {
                CreatedById = _userService.GetCurrentUser().AssociateId
            };

            return ds;
        }

        private void SetIsSecuredByDataClassification(DatasetDto dto)
        {
            switch (dto.DataClassification)
            {
                case GlobalEnums.DataClassificationType.HighlySensitive:
                    dto.IsSecured = true;
                    break;
                case GlobalEnums.DataClassificationType.InternalUseOnly:                    
                    break;
                default:
                    dto.IsSecured = false;
                    break;
            }
        }

        private Dataset CreateDataset(DatasetSchemaDto dto)
        {
            Dataset newDS = CreateDataset((DatasetDto)dto);            

            return newDS;
        }

        /// <summary>
        /// Retrieves the Asset for the given SAID Asset Key Code.
        /// If one does not exist, it creates a new one.
        /// </summary>
        /// <param name="saidAssetKeyCode">The 4-character SAID asset key code</param>
        internal Asset GetAsset(string saidAssetKeyCode)
        {
            var asset = _datasetContext.Assets.FirstOrDefault(da => da.SaidKeyCode == saidAssetKeyCode);
            if (asset == null)
            {
                asset = new Asset()
                {
                    SaidKeyCode = saidAssetKeyCode,
                    Security = new Security(SecurableEntityName.ASSET)
                    {
                        CreatedById = _userService.GetCurrentUser().AssociateId
                    }
                };
            }

            return asset;
        }

        private void MapToDto(Dataset ds, DatasetDto dto)
        {
            IApplicationUser primaryContact = _userService.GetByAssociateId(ds.PrimaryContactId);
            IApplicationUser uploader = _userService.GetByAssociateId(ds.UploadUserName);

            //map the ISecurable properties
            dto.Security = _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());
            dto.PrimaryContactId = ds.PrimaryContactId;
            dto.IsSecured = ds.IsSecured;

            dto.DatasetId = ds.DatasetId;
            dto.GlobalDatasetId = ds.GlobalDatasetId;
            dto.DatasetCategoryIds = ds.DatasetCategories.Select(x => x.Id).ToList();
            dto.DatasetName = ds.DatasetName;
            dto.ShortName = ds.ShortName;
            dto.DatasetDesc = ds.DatasetDesc;
            dto.DatasetInformation = ds.DatasetInformation;
            dto.DatasetType = ds.DatasetType;
            dto.DataClassification = ds.DataClassification;
            dto.CategoryColor = ds.DatasetCategories.FirstOrDefault().Color;
            dto.ObjectStatus = ds.ObjectStatus;

            dto.CreationUserId = ds.CreationUserName;
            dto.CreationUserName = ds.CreationUserName;
            dto.PrimaryContactName = (primaryContact != null ? primaryContact.DisplayName : "Not Available");
            dto.PrimaryContactEmail = (primaryContact != null ? primaryContact.EmailAddress : "");
            dto.UploadUserId = ds.UploadUserName;
            dto.UploadUserName = (uploader != null ? uploader?.DisplayName : "Not Available");

            dto.DatasetDtm = ds.DatasetDtm;
            dto.ChangedDtm = ds.ChangedDtm;
            dto.CanDisplay = ds.CanDisplay;
            dto.TagIds = new List<string>();
            dto.OriginationId = (int)Enum.Parse(typeof(DatasetOriginationCode), ds.OriginationCode);
            dto.CategoryName = ds.DatasetCategories.First().Name;
            dto.MailtoLink = "mailto:?Subject=Dataset%20-%20" + ds.DatasetName + "%20(" + ds.NamedEnvironment + ")&body=%0D%0A" + GetUrl(ds.DatasetId);
            dto.CategoryNames = ds.DatasetCategories.Select(s => s.Name).ToList();
            dto.SAIDAssetKeyCode = ds.Asset.SaidKeyCode;
            dto.NamedEnvironment = ds.NamedEnvironment;
            dto.NamedEnvironmentType = ds.NamedEnvironmentType;
            dto.AlternateContactEmail = ds.AlternateContactEmail;
            dto.SnowflakeDatabases = new List<string>()
            {
                _schemaService.GetSnowflakeDatabaseName(ds.IsHumanResources, ds.NamedEnvironmentType.ToString(), SnowflakeConsumptionType.DatasetSchemaParquet),
                _schemaService.GetSnowflakeDatabaseName(ds.IsHumanResources, ds.NamedEnvironmentType.ToString(), SnowflakeConsumptionType.DatasetSchemaRaw),
                _schemaService.GetSnowflakeDatabaseName(ds.IsHumanResources, ds.NamedEnvironmentType.ToString(), SnowflakeConsumptionType.DatasetSchemaRawQuery)
            };
            dto.SnowflakeSchema = _schemaService.GetSnowflakeSchemaName(ds, SnowflakeConsumptionType.DatasetSchemaParquet);
            dto.SnowflakeWarehouse = SnowflakeWarehouse.WAREHOUSE_NAME;
        }

        private void MapToDto(Dataset ds, DatasetSchemaDto dto)
        {
            //Map DatasetDto properties
            MapToDto(ds, (DatasetDto)dto);

            dto.OriginationId = (int)Enum.Parse(typeof(DatasetOriginationCode), ds.OriginationCode);
            //Only populate if dataset is associated with schema
            if (ds.DatasetFileConfigs != null && ds.DatasetFileConfigs.Any())
            {
                dto.ConfigFileDesc = ds.DatasetFileConfigs?.First()?.Description;
                dto.ConfigFileName = ds.DatasetFileConfigs?.First()?.Name;
                dto.Delimiter = ds.DatasetFileConfigs?.First()?.Schema?.Delimiter;
                dto.FileExtensionId = ds.DatasetFileConfigs.First().FileExtension.Id;
                dto.DatasetScopeTypeId = ds.DatasetFileConfigs.First().DatasetScopeType.ScopeTypeId;
            }            
        }

        private void MapToDetailDto(Dataset ds, DatasetDetailDto dto)
        {
            //Map DatasetSchemaDto properites
            MapToDto(ds, dto);

            IApplicationUser user = _userService.GetCurrentUser();

            dto.Downloads = _datasetContext.Events.Where(x => x.EventType.Description == GlobalConstants.EventType.DOWNLOAD && x.Dataset == ds.DatasetId).Count();
            dto.IsSubscribed = _datasetContext.IsUserSubscribedToDataset(user.AssociateId, dto.DatasetId);
            dto.AmountOfSubscriptions = _datasetContext.GetAllUserSubscriptionsForDataset(user.AssociateId, dto.DatasetId).Count;
            dto.Views = _datasetContext.Events.Where(x => x.EventType.Description == GlobalConstants.EventType.VIEWED && x.Dataset == ds.DatasetId).Count();
            dto.IsFavorite = ds.Favorities.Any(w => w.UserId == user.AssociateId);
            dto.DatasetFileConfigSchemas = ds.DatasetFileConfigs.Where(w => !w.DeleteInd).Select(x => x.ToDatasetFileConfigSchemaDto()).ToList();
            dto.DatasetScopeTypeNames = ds.DatasetScopeType().ToDictionary(x => x.Name, y => y.Description);
            dto.DatasetFileCount = ds.DatasetFiles.Count();
            dto.OriginationCode = ds.OriginationCode;
            dto.DataClassificationDescription = ds.DataClassification.GetDescription();
            dto.CategoryColor = ds.DatasetCategories.First().Color;
            dto.CategoryNames = ds.DatasetCategories.Select(x => x.Name).ToList();
            dto.GroupAccessCount = _securityService.GetGroupAccessCount(ds);
            dto.SAIDAssetKeyCode = ds.Asset.SaidKeyCode;
            if (ds.DatasetFiles.Any())
            {
                dto.ChangedDtm = ds.DatasetFiles.Max(x => x.ModifiedDTM);
            }
            dto.DatasetRelatives = _datasetContext.Datasets.Where(w => w.DatasetName.Trim() == ds.DatasetName.Trim() && w.ObjectStatus == GlobalEnums.ObjectStatusEnum.Active)
                                    .Select(s => new DatasetRelativeDto(s.DatasetId, s.NamedEnvironment, GetUrl(s.DatasetId)))
                                    .ToList();
        }

        private string GetUrl(int datasetId)
        {
            return $"{Configuration.Config.GetHostSetting("SentryDataBaseUrl")}/Dataset/Detail/{datasetId}";
        }

        private void GenerateSnowSchemaEventForDataset(Dataset dataset, bool isDelete)
        {
            JObject datasetCreatedChangeInd = new JObject();
            datasetCreatedChangeInd.Add("dataset", isDelete ? "deleted" : "added");

            int datasetId = dataset.DatasetId;
            string jsonPayload;

            if (_dataFeatures.CLA5211_SendNewSnowflakeEvents.GetValue())
            {
                SnowConsumptionMessageModel snowModel = new SnowConsumptionMessageModel()
                {
                    EventType = isDelete ? SnowConsumptionMessageTypes.DELETE_REQUEST : SnowConsumptionMessageTypes.CREATE_REQUEST,
                    SchemaID = 0,
                    RevisionID = 0,
                    DatasetID = datasetId,
                    InitiatorID = _userService.GetCurrentUser().AssociateId,
                    ChangeIND = datasetCreatedChangeInd.ToString(Formatting.None)
                };
                jsonPayload = JsonConvert.SerializeObject(snowModel);
            }
            else
            {
                SnowSchemaCreateModel snowModel = new SnowSchemaCreateModel()
                {
                    DatasetID = datasetId,
                    InitiatorID = _userService.GetCurrentUser().AssociateId,
                    ChangeIND = datasetCreatedChangeInd.ToString(Formatting.None)
                };
                jsonPayload = JsonConvert.SerializeObject(snowModel);
            }

            try
            {
                _logger.LogInformation($"{nameof(GenerateSnowSchemaEventForDataset)} sending event: {jsonPayload}");

                string topicName = null;
                if (string.IsNullOrWhiteSpace(_dataFeatures.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()))
                {
                    topicName = new DscEventTopicHelper().GetDSCTopic(dataset);
                    _messagePublisher.Publish(topicName, datasetId.ToString(), jsonPayload);
                }
                else
                {
                    _messagePublisher.PublishDSCEvent(datasetId.ToString(), jsonPayload, topicName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GenerateSnowSchemaEventForDataset)} failed sending event: {jsonPayload}");
            }
        }
        
        #endregion

    }
}
