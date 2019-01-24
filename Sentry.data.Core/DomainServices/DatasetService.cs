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
    public class DatasetService :IDatasetService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly UserService _userService;

        public DatasetService(IDatasetContext datasetContext, UserService userService)
        {
            _datasetContext = datasetContext;
            _userService = userService;
        }


        public DatasetDto GetDatasetDto(int id)
        {
            return MapToDto(_datasetContext.GetById<Dataset>(id));
        }

        public DatasetDetailDto GetDatesetDetailDto(int id)
        {
            return MapToDetailDto(_datasetContext.GetById<Dataset>(id));
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
                ds.DatasetCategories = _datasetContext.Categories.Where(x=> dto.DatasetCategoryIds.Contains(x.Id)).ToList();
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
            if (null != dto.SentryOwnerId && dto.SentryOwnerId.Length > 0)
            {
                ds.SentryOwnerName = dto.SentryOwnerId;
            }
            if(dto.DataClassification > 0)
            {
                ds.DataClassification = dto.DataClassification;
            }
            _datasetContext.SaveChanges();
        }


        public List<string> Validate(DatasetDto dto)
        {
            List<string> errors = new List<string>();
            if (dto.DatasetId == 0 && _datasetContext.Datasets.Where(w => w.DatasetName == dto.DatasetName &&
                                                                         w.DatasetCategories.Any(x => dto.DatasetCategoryIds.Contains(x.Id)) &&
                                                                         w.DatasetType == GlobalConstants.DataEntityTypes.DATASET).Count() > 0)
            {
                errors.Add("Dataset name already exists within category");
            }
            return errors;
        }

        #region "private functions"

        private Dataset CreateDataset(DatasetDto dto)
        {
            Dataset ds = new Dataset()
            {
                DatasetId = dto.DatasetId,
                DatasetCategories = _datasetContext.Categories.Where(x=> x.Id == dto.DatasetCategoryIds.First()).ToList(),
                DatasetName = dto.DatasetName,
                DatasetDesc = dto.DatasetDesc,
                DatasetInformation = dto.DatasetInformation,
                CreationUserName = dto.CreationUserName,
                SentryOwnerName = dto.SentryOwnerId,//done on purpose since namming flipped.
                UploadUserName = dto.UploadUserName,
                OriginationCode = Enum.GetName(typeof(DatasetOriginationCode), dto.OriginationId),
                DatasetDtm = dto.DatasetDtm,
                ChangedDtm = dto.ChangedDtm,
                DatasetType = GlobalConstants.DataEntityTypes.DATASET,
                DataClassification = dto.DataClassification,
                IsSensitive = false,
                CanDisplay = true,
                DatasetFiles = null,
                DatasetFileConfigs = null
            };

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
            catch(Exception ex)
            {
                StringBuilder errmsg = new StringBuilder();
                errmsg.AppendLine("Failed to Create Drop Location:");
                errmsg.AppendLine($"DatasetId: {dfc.ParentDataset?.DatasetId}");
                errmsg.AppendLine($"DatasetName: {dfc.ParentDataset?.DatasetName}");
                errmsg.AppendLine($"DropLocation: {path}");

                Logger.Error(errmsg.ToString(), ex);
            }
        }


        private DatasetDto MapToDto(Dataset ds)
        {
            if (ds == null) { return new DatasetDto(); }
            return MapToDetailDto(ds, false);
        }

        private DatasetDetailDto MapToDetailDto(Dataset ds, bool mapDetails = true)
        {
            string userDisplayname = _userService.GetByAssociateId(ds.SentryOwnerName)?.DisplayName;
            //base level info
            DatasetDetailDto dto = new DatasetDetailDto()
            {
                DatasetId = ds.DatasetId,
                DatasetCategoryIds = ds.DatasetCategories.Select(x => x.Id).ToList(),
                DatasetName = ds.DatasetName,
                DatasetDesc = ds.DatasetDesc,
                DatasetInformation = ds.DatasetInformation,
                DatasetType = ds.DatasetType,
                DataClassification = ds.DataClassification,
                CreationUserName = ds.CreationUserName,
                SentryOwnerName = (string.IsNullOrWhiteSpace(userDisplayname) ? ds.SentryOwnerName : userDisplayname),
                SentryOwnerId = ds.SentryOwnerName,
                UploadUserName = ds.UploadUserName,
                DatasetDtm = ds.DatasetDtm,
                ChangedDtm = ds.ChangedDtm,
                IsSensitive = ds.IsSensitive,
                CanDisplay = ds.CanDisplay,
                TagIds = new List<string>(),
                OriginationId = (int)Enum.Parse(typeof(DatasetOriginationCode), ds.OriginationCode),
                ConfigFileDesc = ds.DatasetFileConfigs?.First()?.Description,
                ConfigFileName = ds.DatasetFileConfigs?.First()?.Name,
                Delimiter = ds.DatasetFileConfigs?.First()?.Schema?.First()?.Delimiter,
                FileExtensionId = ds.DatasetFileConfigs.First().FileExtension.Id,
                DatasetScopeTypeId = ds.DatasetFileConfigs.First().DatasetScopeType.ScopeTypeId,
                CategoryName = ds.DatasetCategories.First().Name,
                MailtoLink = "mailto:?Subject=Dataset%20-%20" + ds.DatasetName + "&body=%0D%0A" + Configuration.Config.GetHostSetting("SentryDataBaseUrl") + "/Dataset/Detail/" + ds.DatasetId
        };

            if (mapDetails)
            {
                //Details
                IApplicationUser user = _userService.GetCurrentUser();
                dto.CanDwnldSenstive = user.CanDwnldSenstive;
                dto.CanEditDataset = user.CanEditDataset;
                dto.CanManageConfigs = user.CanManageConfigs;
                dto.CanDwnldNonSensitive = user.CanDwnldNonSensitive;
                dto.CanEditDataset = user.CanEditDataset;
                dto.CanUpload = user.CanUpload;
                dto.CanQueryTool = user.CanQueryTool || user.CanQueryToolPowerUser;
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

            return dto;
        }


        #endregion

    }
}
