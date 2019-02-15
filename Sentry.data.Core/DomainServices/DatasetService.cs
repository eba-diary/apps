using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sentry.Common.Logging;
using Sentry.Core;

namespace Sentry.data.Core
{
    public class DatasetService : IDatasetService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly ISecurityService _securityService;
        private readonly UserService _userService;

        public DatasetService(IDatasetContext datasetContext, ISecurityService securityService, UserService userService)
        {
            _datasetContext = datasetContext;
            _securityService = securityService;
            _userService = userService;
        }


        public DatasetDto GetDatasetDto(int id)
        {
            Dataset ds = _datasetContext.Datasets.Where(x => x.DatasetId == id && x.CanDisplay).FetchAllChildren(_datasetContext).FirstOrDefault();
            DatasetDto dto = new DatasetDto();
            MapToDto(ds, dto);

            return dto;
        }

        public DatasetDetailDto GetDatesetDetailDto(int id)
        {
            Dataset ds = _datasetContext.Datasets.Where(x => x.DatasetId == id && x.CanDisplay).FetchAllChildren(_datasetContext).FirstOrDefault();

            DatasetDetailDto dto = new DatasetDetailDto();
            MapToDetailDto(ds, dto);

            return dto;
        }

        public UserSecurity GetUserSecurityForDataset(int datasetId)
        {
            Dataset ds = _datasetContext.Datasets.Where(x => x.DatasetId == datasetId && x.CanDisplay).FetchSecurityTree(_datasetContext).FirstOrDefault();

            return _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());
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
            var query = _datasetContext.Datasets.Where(x => x.DatasetType == GlobalConstants.DataEntityCodes.DATASET &&
                                                                                        (x.Security == null || x.Security.Tickets.Any(y => y.Permissions.Any(z => z.Permission.PermissionCode == GlobalConstants.PermissionCodes.CAN_QUERY_DATASET && z.IsEnabled))));

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

        public AccessRequest GetAccessRequest(int datasetId)
        {
            Dataset ds = _datasetContext.GetById<Dataset>(datasetId);

            AccessRequest ar = new AccessRequest()
            {
                Permissions = _datasetContext.Permission.Where(x => x.SecurableObject == GlobalConstants.SecurableEntityName.DATASET).ToList(),
                ApproverList = new List<KeyValuePair<string, string>>(),
                DatasetId = ds.DatasetId,
                DatasetName = ds.DatasetName
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

        public string RequestAccessToDataset(AccessRequest request)
        {

            Dataset ds = _datasetContext.GetById<Dataset>(request.DatasetId);
            if (ds != null)
            {
                IApplicationUser user = _userService.GetCurrentUser();
                request.DatasetName = ds.DatasetName;
                request.SecurityId = ds.Security.SecurityId;
                request.RequestorsId = user.AssociateId;
                request.RequestorsName = user.DisplayName;
                request.IsProd = bool.Parse(Configuration.Config.GetHostSetting("RequireApprovalHPSMTickets"));
                request.RequestedDate = DateTime.Now;
                request.ApproverId = request.SelectedApprover;
                request.Permissions = _datasetContext.Permission.Where(x => request.SelectedPermissionCodes.Contains(x.PermissionCode) &&
                                                                                                                x.SecurableObject == GlobalConstants.SecurableEntityName.DATASET).ToList();
                return _securityService.RequestPermission(request);
            }

            return string.Empty;
        }
        public int CreateAndSaveNewDataset(DatasetDto dto)
        {
            Dataset ds = CreateDataset(dto);
            _datasetContext.Add(ds);

            DataElement de = CreateDataElement(dto);
            DatasetFileConfig dfc = CreateDatasetFileConfig(dto, ds);

            de.DatasetFileConfig = dfc;
            dfc.Schema = new List<DataElement> { de };

            List<RetrieverJob> jobList = new List<RetrieverJob>
            {
                CreateRetrieverJob(dfc, GlobalConstants.DataSourceName.DEFAULT_DROP_LOCATION),
                CreateRetrieverJob(dfc, GlobalConstants.DataSourceName.DEFAULT_S3_DROP_LOCATION)
            };

            dfc.RetrieverJobs = jobList;

            _datasetContext.Merge(dfc);
            _datasetContext.SaveChanges();

            return ds.DatasetId;
        }


        public void UpdateAndSaveDataset(DatasetDto dto)
        {
            Dataset ds = _datasetContext.GetById<Dataset>(dto.DatasetId);

            ds.DatasetInformation = dto.DatasetInformation;
            ds.OriginationCode = Enum.GetName(typeof(DatasetOriginationCode), dto.OriginationId);
            ds.ChangedDtm = DateTime.Now;

            if (dto.DatasetCategoryIds?.Count() > 0)
            {
                ds.DatasetCategories = _datasetContext.Categories.Where(x => dto.DatasetCategoryIds.Contains(x.Id)).ToList();
            }
            if (null != dto.CreationUserId && dto.CreationUserId.Length > 0)
            {
                ds.CreationUserName = dto.CreationUserId;
            }
            if (null != dto.DatasetDesc && dto.DatasetDesc.Length > 0)
            {
                ds.DatasetDesc = dto.DatasetDesc;
            }
            if (dto.DatasetDtm > DateTime.MinValue)
            {
                ds.DatasetDtm = dto.DatasetDtm;
            }
            if (null != dto.DatasetName && dto.DatasetName.Length > 0)
            {
                ds.DatasetName = dto.DatasetName;
            }
            if (null != dto.PrimaryOwnerId && dto.PrimaryOwnerId.Length > 0)
            {
                ds.PrimaryOwnerId = dto.PrimaryOwnerId;
            }
            if (null != dto.PrimaryContactId && dto.PrimaryContactId.Length > 0)
            {
                ds.PrimaryContactId = dto.PrimaryContactId;
            }
            if (dto.DataClassification > 0)
            {
                ds.DataClassification = dto.DataClassification;
            }

            //override the Dto.IsSecured for certain classifications.
            switch (dto.DataClassification)
            {
                case GlobalEnums.DataClassificationType.HighlySensitive:
                    dto.IsSecured = true;
                    break;
                case GlobalEnums.DataClassificationType.InternalUseOnly:
                    //don't override as it should flow from the form.
                    break;
                default:
                    dto.IsSecured = false;
                    break;
            }

            if (!ds.IsSecured && dto.IsSecured)
            {
                if (ds.Security == null)
                {
                    ds.Security = new Security(GlobalConstants.SecurableEntityName.DATASET)
                    {
                        CreatedById = _userService.GetCurrentUser().AssociateId
                    };
                }
                else
                {
                    ds.Security.EnabledDate = DateTime.Now;
                    ds.Security.UpdatedById = _userService.GetCurrentUser().AssociateId;
                }
            }
            else if (ds.IsSecured && !dto.IsSecured)
            {
                ds.Security.RemovedDate = DateTime.Now;
                ds.Security.UpdatedById = _userService.GetCurrentUser().AssociateId;
            }

            ds.IsSecured = dto.IsSecured;


            _datasetContext.SaveChanges();
        }


        public List<string> Validate(DatasetDto dto)
        {
            List<string> errors = new List<string>();
            if (dto.DatasetId == 0 && _datasetContext.Datasets.Where(w => w.DatasetName == dto.DatasetName &&
                                                                         w.DatasetCategories.Any(x => dto.DatasetCategoryIds.Contains(x.Id)) &&
                                                                         w.DatasetType == GlobalConstants.DataEntityCodes.DATASET).Count() > 0)
            {
                errors.Add("Dataset name already exists within category");
            }

            var currentFileExtension = _datasetContext.FileExtensions.FirstOrDefault(x => x.Id == dto.FileExtensionId).Name.ToLower();

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

        #region "private functions"

        private Dataset CreateDataset(DatasetDto dto)
        {
            Dataset ds = new Dataset()
            {
                DatasetId = dto.DatasetId,
                DatasetCategories = _datasetContext.Categories.Where(x => x.Id == dto.DatasetCategoryIds.First()).ToList(),
                DatasetName = dto.DatasetName,
                DatasetDesc = dto.DatasetDesc,
                DatasetInformation = dto.DatasetInformation,
                CreationUserName = dto.CreationUserId,
                PrimaryOwnerId = dto.PrimaryOwnerId,
                PrimaryContactId = dto.PrimaryContactId,
                UploadUserName = dto.UploadUserId,
                OriginationCode = Enum.GetName(typeof(DatasetOriginationCode), dto.OriginationId),
                DatasetDtm = dto.DatasetDtm,
                ChangedDtm = dto.ChangedDtm,
                DatasetType = GlobalConstants.DataEntityCodes.DATASET,
                DataClassification = dto.DataClassification,
                IsSecured = dto.IsSecured,
                CanDisplay = true,
                DatasetFiles = null,
                DatasetFileConfigs = null
            };

            switch (dto.DataClassification)
            {
                case GlobalEnums.DataClassificationType.HighlySensitive:
                    ds.IsSecured = true;
                    break;
                case GlobalEnums.DataClassificationType.InternalUseOnly:
                    ds.IsSecured = dto.IsSecured;
                    break;
                default:
                    ds.IsSecured = false;
                    break;
            }

            if (ds.IsSecured)
            {
                ds.Security = new Security(GlobalConstants.SecurableEntityName.DATASET)
                {
                    CreatedById = _userService.GetCurrentUser().AssociateId
                };
            }

            return ds;
        }

        private DataElement CreateDataElement(DatasetDto dto)
        {
            string storageCode = _datasetContext.GetNextStorageCDE().ToString();

            DataElement de = new DataElement()
            {
                DataElementCreate_DTM = DateTime.Now,
                DataElementChange_DTM = DateTime.Now,
                DataElement_CDE = GlobalConstants.DataElementCode.DATA_FILE,
                DataElement_DSC = GlobalConstants.DataElementDescription.DATA_FILE,
                DataElement_NME = dto.ConfigFileName,
                LastUpdt_DTM = DateTime.Now,
                SchemaIsPrimary = true,
                SchemaDescription = dto.ConfigFileDesc,
                SchemaName = dto.ConfigFileName,
                SchemaRevision = 1,
                SchemaIsForceMatch = false,
                FileFormat = _datasetContext.GetById<FileExtension>(dto.FileExtensionId).Name.ToUpper(),
                Delimiter = dto.Delimiter,
                StorageCode = storageCode,
                HiveDatabase = "Default",
                HiveTable = dto.DatasetName.Replace(" ", "").Replace("_", "").ToUpper() + "_" + dto.ConfigFileName.Replace(" ", "").ToUpper(),
                HiveTableStatus = HiveTableStatusEnum.NameReserved.ToString(),
                HiveLocation = Configuration.Config.GetHostSetting("AWSRootBucket") + GlobalConstants.ConvertedFileStoragePrefix.PARQUET_STORAGE_PREFIX + "/" + Configuration.Config.GetHostSetting("S3DataPrefix") + storageCode
            };

            return de;
        }

        private DatasetFileConfig CreateDatasetFileConfig(DatasetDto dto, Dataset ds)
        {
            DatasetFileConfig dfc = new DatasetFileConfig()
            {
                ConfigId = 0,
                Name = dto.ConfigFileName,
                Description = dto.ConfigFileDesc,
                FileTypeId = (int)FileType.DataFile,
                ParentDataset = ds,
                DatasetScopeType = _datasetContext.GetById<DatasetScopeType>(dto.DatasetScopeTypeId),
                FileExtension = _datasetContext.GetById<FileExtension>(dto.FileExtensionId)
            };

            return dfc;
        }

        private RetrieverJob CreateRetrieverJob(DatasetFileConfig dfc, string dataSourceName)
        {
            RetrieverJobOptions.Compression compression = new RetrieverJobOptions.Compression()
            {
                IsCompressed = false,
                CompressionType = null,
                FileNameExclusionList = new List<string>()
            };

            RetrieverJobOptions rjo = new RetrieverJobOptions()
            {
                OverwriteDataFile = false,
                TargetFileName = "",
                CreateCurrentFile = false,
                IsRegexSearch = true,
                SearchCriteria = "\\.",
                CompressionOptions = compression
            };

            DataSource dataSource = _datasetContext.DataSources.First(x => x.Name.Contains(dataSourceName));

            RetrieverJob rj = new RetrieverJob()
            {
                TimeZone = "Central Standard Time",
                RelativeUri = null,
                DataSource = dataSource,
                DatasetConfig = dfc,
                Created = DateTime.Now,
                Modified = DateTime.Now,
                IsGeneric = true,

                JobOptions = rjo
            };

            // Config Drop location
            if (dataSourceName == GlobalConstants.DataSourceName.DEFAULT_DROP_LOCATION)
            {
                CreateDropLocation(rj.GetUri().LocalPath, dfc);
            }

            if (dataSource.Is<S3Basic>())
            {
                rj.Schedule = "*/1 * * * *";
            }
            else if (dataSource.Is<DfsBasic>())
            {
                rj.Schedule = "Instant";
            }
            else
            {
                throw new NotImplementedException("This method does not support this type of Data Source");
            }

            return rj;
        }

        private void CreateDropLocation(string path, DatasetFileConfig dfc)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                StringBuilder errmsg = new StringBuilder();
                errmsg.AppendLine("Failed to Create Drop Location:");
                errmsg.AppendLine($"DatasetId: {dfc.ParentDataset?.DatasetId}");
                errmsg.AppendLine($"DatasetName: {dfc.ParentDataset?.DatasetName}");
                errmsg.AppendLine($"DropLocation: {path}");

                Logger.Error(errmsg.ToString(), ex);
            }
        }


        private void MapToDto(Dataset ds, DatasetDto dto)
        {
            IApplicationUser primaryOwner = _userService.GetByAssociateId(ds.PrimaryOwnerId);
            IApplicationUser primaryContact = _userService.GetByAssociateId(ds.PrimaryContactId);
            IApplicationUser uploader = _userService.GetByAssociateId(ds.UploadUserName);

            //map the ISecurable properties
            dto.Security = _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());
            dto.PrimaryOwnerId = ds.PrimaryOwnerId;
            dto.PrimaryContactId = ds.PrimaryContactId;
            dto.IsSecured = ds.IsSecured;

            dto.DatasetId = ds.DatasetId;
            dto.DatasetCategoryIds = ds.DatasetCategories.Select(x => x.Id).ToList();
            dto.DatasetName = ds.DatasetName;
            dto.DatasetDesc = ds.DatasetDesc;
            dto.DatasetInformation = ds.DatasetInformation;
            dto.DatasetType = ds.DatasetType;
            dto.DataClassification = ds.DataClassification;

            dto.CreationUserId = ds.CreationUserName;
            dto.CreationUserName = ds.CreationUserName;
            dto.PrimaryOwnerName = (primaryOwner != null ? primaryOwner.DisplayName : "Not Available");
            dto.PrimaryContactName = (primaryContact != null ? primaryContact.DisplayName : "Not Available");
            dto.PrimaryContactEmail = (primaryContact != null ? primaryContact.EmailAddress : "");
            dto.UploadUserId = ds.UploadUserName;
            dto.UploadUserName = (uploader != null ? uploader?.DisplayName : "Not Available");

            dto.DatasetDtm = ds.DatasetDtm;
            dto.ChangedDtm = ds.ChangedDtm;
            dto.CanDisplay = ds.CanDisplay;
            dto.TagIds = new List<string>();
            dto.OriginationId = (int)Enum.Parse(typeof(DatasetOriginationCode), ds.OriginationCode);
            dto.ConfigFileDesc = ds.DatasetFileConfigs?.First()?.Description;
            dto.ConfigFileName = ds.DatasetFileConfigs?.First()?.Name;
            dto.Delimiter = ds.DatasetFileConfigs?.First()?.Schema?.First()?.Delimiter;
            dto.FileExtensionId = ds.DatasetFileConfigs.First().FileExtension.Id;
            dto.DatasetScopeTypeId = ds.DatasetFileConfigs.First().DatasetScopeType.ScopeTypeId;
            dto.CategoryName = ds.DatasetCategories.First().Name;
            dto.MailtoLink = "mailto:?Subject=Dataset%20-%20" + ds.DatasetName + "&body=%0D%0A" + Configuration.Config.GetHostSetting("SentryDataBaseUrl") + "/Dataset/Detail/" + ds.DatasetId;
        }



        private void MapToDetailDto(Dataset ds, DatasetDetailDto dto)
        {
            MapToDto(ds, dto);

            IApplicationUser user = _userService.GetCurrentUser();

            dto.Downloads = _datasetContext.Events.Where(x => x.EventType.Description == GlobalConstants.EventType.DOWNLOAD && x.Dataset == ds.DatasetId).Count();
            dto.IsSubscribed = _datasetContext.IsUserSubscribedToDataset(_userService.GetCurrentUser().AssociateId, dto.DatasetId);
            dto.AmountOfSubscriptions = _datasetContext.GetAllUserSubscriptionsForDataset(_userService.GetCurrentUser().AssociateId, dto.DatasetId).Count;
            dto.Views = _datasetContext.Events.Where(x => x.EventType.Description == GlobalConstants.EventType.VIEWED && x.Dataset == ds.DatasetId).Count();
            dto.IsFavorite = ds.Favorities.Any(w => w.UserId == user.AssociateId);
            dto.DatasetFileConfigNames = ds.DatasetFileConfigs.ToDictionary(x => x.ConfigId.ToString(), y => y.Name);
            dto.DatasetScopeTypeNames = ds.DatasetScopeType.ToDictionary(x => x.Name, y => y.Description);
            dto.DistinctFileExtensions = ds.DatasetFiles.Select(x => Path.GetExtension(x.FileName).TrimStart('.').ToLower()).Distinct().ToList();
            dto.DatasetFileCount = ds.DatasetFiles.Count();
            dto.OriginationCode = ds.OriginationCode;
            dto.DataClassificationDescription = ds.DataClassification.GetDescription();
            dto.CategoryColor = ds.DatasetCategories.First().Color;
            dto.CategoryNames = ds.DatasetCategories.Select(x => x.Name).ToList();
            if (ds.DatasetFiles.Any())
            {
                dto.ChangedDtm = ds.DatasetFiles.Max(x => x.ModifiedDTM);
            }
        }

        #endregion

    }
}
