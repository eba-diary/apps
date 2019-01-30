using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sentry.Common.Logging;
using Sentry.data.Core.Entities.Metadata;
using static Sentry.data.Core.RetrieverJobOptions;

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
            Dataset ds = _datasetContext.GetById<Dataset>(id);
            DatasetDto dto = new DatasetDto();
            MapToDto(ds, dto);

            return dto;
        }

        public DatasetDetailDto GetDatesetDetailDto(int id)
        {
            Dataset ds = _datasetContext.GetById<Dataset>(id);
            DatasetDetailDto dto = new DatasetDetailDto();
            MapToDetailDto(ds, dto);

            return dto;
        }

        public UserSecurity GetUserSecurityForDataset(int datasetId)
        {
            Dataset ds = _datasetContext.GetById<Dataset>(datasetId);
            UserSecurity us = GetUserSecurity(ds);
            return us;
        }

        public UserSecurity GetUserSecurityForConfig(int configId)
        {
            DatasetFileConfig dfc = _datasetContext.GetById<DatasetFileConfig>(configId);
            if(dfc.ParentDataset != null)
            {
                return GetUserSecurity(dfc.ParentDataset);
            }
            return new UserSecurity();
        }

        public UserSecurity GetUserSecurity()
        {
            return GetUserSecurity(null);
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
            if (null != dto.CreationUserName && dto.CreationUserName.Length > 0)
            {
                ds.CreationUserName = dto.CreationUserName;
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
            if (null != dto.SecondaryOwnerId && dto.SecondaryOwnerId.Length > 0)
            {
                ds.SecondaryOwnerId = dto.SecondaryOwnerId;
            }
            if (dto.DataClassification > 0)
            {
                ds.DataClassification = dto.DataClassification;
            }

            if(ds.IsSecured != dto.IsSecured)
            {
                if(ds.Security == null)
                {
                    ds.Security = new Security(GlobalConstants.SecurableEntityName.DATASET);
                }

                if(ds.IsSecured && !dto.IsSecured){
                    ds.Security.RemovedDate = DateTime.Now;
                }else{
                    ds.Security.EnabledDate = DateTime.Now;
                }
                ds.IsSecured = dto.IsSecured;
                ds.Security.UpdatedById = _userService.GetCurrentUser().AssociateId;
            }


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

            if(dto.DataClassification == GlobalEnums.DataClassificationType.HighlySensitive && string.IsNullOrWhiteSpace(dto.SecondaryOwnerId))
            {
                errors.Add("Secondary owner is required");
            }
            if (dto.PrimaryOwnerId.Equals(dto.SecondaryOwnerId))
            {
                errors.Add("Secondary owner can not be the same as the primery owner");
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
                CreationUserName = dto.CreationUserName,
                PrimaryOwnerId = dto.PrimaryOwnerId,//done on purpose since namming flipped.
                SecondaryOwnerId = dto.SecondaryOwnerId,
                UploadUserName = dto.UploadUserName,
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
                //case GlobalEnums.DataClassificationType.Restricted:
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
                ds.Security = new Security(GlobalConstants.SecurableEntityName.DATASET);
            }

            return ds;
        }

        private DataElement CreateDataElement(DatasetDto dto)
        {
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
                StorageCode = _datasetContext.GetNextStorageCDE().ToString(),
                HiveDatabase = "Default",
                HiveTable = dto.DatasetName.Replace(" ", "").Replace("_", "").ToUpper() + "_" + dto.ConfigFileName.Replace(" ", "").ToUpper(),
                HiveTableStatus = HiveTableStatusEnum.NameReserved.ToString()
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
            Compression compression = new Compression()
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
            
            dto.DatasetId = ds.DatasetId;
            dto.DatasetCategoryIds = ds.DatasetCategories.Select(x => x.Id).ToList();
            dto.DatasetName = ds.DatasetName;
            dto.DatasetDesc = ds.DatasetDesc;
            dto.DatasetInformation = ds.DatasetInformation;
            dto.DatasetType = ds.DatasetType;
            dto.DataClassification = ds.DataClassification;
            dto.CreationUserName = ds.CreationUserName;
            dto.PrimaryOwnerId = ds.PrimaryOwnerId;
            dto.PrimaryOwnerName = (primaryOwner != null ? primaryOwner.DisplayName : ds.PrimaryOwnerId);
            dto.SecondaryOwnerId = ds.SecondaryOwnerId;
            if(ds.SecondaryOwnerId != null){
                IApplicationUser secondaryOwner = _userService.GetByAssociateId(ds.SecondaryOwnerId);
                if(secondaryOwner != null && secondaryOwner.DisplayName != null)
                {
                    dto.SecondaryOwnerName = secondaryOwner.DisplayName;
                }else{
                    dto.SecondaryOwnerName = ds.SecondaryOwnerId;
                }
            }
            dto.IsSecured = ds.IsSecured;
            dto.UploadUserName = ds.UploadUserName;
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

            dto.CanEditDataset = user.CanModifyDataset && (ds.PrimaryOwnerId == user.AssociateId || ds.SecondaryOwnerId == user.AssociateId);
            dto.Downloads = _datasetContext.Events.Where(x => x.EventType.Description == GlobalConstants.EventType.DOWNLOAD && x.Dataset == ds.DatasetId).Count();
            dto.IsSubscribed = _datasetContext.IsUserSubscribedToDataset(_userService.GetCurrentUser().AssociateId, dto.DatasetId);
            dto.AmountOfSubscriptions = _datasetContext.GetAllUserSubscriptionsForDataset(_userService.GetCurrentUser().AssociateId, dto.DatasetId).Count;
            dto.Views = _datasetContext.Events.Where(x => x.EventType.Description == GlobalConstants.EventType.VIEWED && x.Dataset == ds.DatasetId).Count();
            dto.IsFavorite = ds.Favorities.Any(w => w.UserId == user.AssociateId);
            dto.DatasetFileConfigNames = ds.DatasetFileConfigs.ToDictionary(x => x.ConfigId.ToString(), y => y.Name);
            dto.DatasetScopeTypeNames = ds.DatasetScopeType.ToDictionary(x => x.Name, y => y.Description);
            dto.DistinctFileExtensions = ds.DatasetFiles.Select(x => Path.GetExtension(x.FileName).TrimStart('.').ToLower()).ToList();
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


        private UserSecurity GetUserSecurity(Dataset ds)
        {
            UserSecurity us = new UserSecurity();
            IApplicationUser user = _userService.GetCurrentUser();

            if (ds != null) {
                if (ds.IsSecured)
                {
                    //if it is secured, call out to the secure service to get the user approved permissions.
                    us = _securityService.GetUserSecurity(ds.Security);
                }
                else
                {
                    //if it is not secure, it is public and open except for upload - this should only be allowed for one of the owners.
                    us = new UserSecurity()
                    {
                        CanConnectToDataset = true,
                        CanPreviewDataset = true,
                        CanQueryDataset = true,
                        CanViewFullDataset = true,
                        CanUploadToDataset = false
                    };
                }

                //they may not be approved to upload to this dataset, but if they are one of the owners they still should be able to.
                if (!us.CanUploadToDataset)
                { 
                    us.CanUploadToDataset = user.AssociateId == ds.PrimaryOwnerId || user.AssociateId == ds.SecondaryOwnerId;
                }

                //user must have permission to modify dataset AND must be one of the owners.
                us.CanEditDataset = user.CanModifyDataset && (user.AssociateId == ds.PrimaryOwnerId || user.AssociateId == ds.SecondaryOwnerId);
            }

            //this is only for the creation of a new dataset.
            us.CanCreateDataset = user.CanModifyDataset;


            return us;
        }

        #endregion

    }
}
