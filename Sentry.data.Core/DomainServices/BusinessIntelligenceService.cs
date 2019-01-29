using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Sentry.Common.Logging;

namespace Sentry.data.Core
{
    public class BusinessIntelligenceService : IBusinessIntelligenceService
    {

        private readonly IDatasetContext _reportContext;
        private readonly UserService _userService;

        public BusinessIntelligenceService(IDatasetContext reportContext, UserService userService)
        {
            _reportContext = reportContext;
            _userService = userService;
        }

        #region "Public Functions"


        public BusinessIntelligenceDto GetBusinessIntelligenceDto(int datasetId)
        {
            BusinessIntelligenceDto dto = new BusinessIntelligenceDto();
            Dataset ds = _reportContext.GetById<Dataset>(datasetId);
            MapToDto(ds, dto);
            return dto;
        }
        public BusinessIntelligenceDetailDto GetBusinessIntelligenceDetailDto(int datasetId)
        {
            BusinessIntelligenceDetailDto dto = new BusinessIntelligenceDetailDto();
            Dataset ds = _reportContext.GetById<Dataset>(datasetId);

            MapToDetailDto(ds, dto);

            return dto;
        }

        public BusinessIntelligenceHomeDto GetHomeDto()
        {
            return new BusinessIntelligenceHomeDto()
            {
                DatasetCount = _reportContext.GetReportCount(),
                Categories = _reportContext.Categories.Where(x => x.ObjectType == GlobalConstants.DataEntityCodes.REPORT).ToList(),
                CanManageReports = _userService.GetCurrentUser().CanManageReports
            };
        }

        public bool CreateAndSaveBusinessIntelligence(BusinessIntelligenceDto dto)
        {
            try
            {
                CreateDataset(dto);

                _reportContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("Error saving business intelligence - ", ex);
                return false;
            }
            return true;
        }

        public bool UpdateAndSaveBusinessIntelligence(BusinessIntelligenceDto dto)
        {
            try
            {
                Dataset ds = _reportContext.GetById<Dataset>(dto.DatasetId);

                UpdateDataset(dto, ds);
                UpdateDatasetFileConfig(dto, ds);

                _reportContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("Error saving business intelligence - ", ex);
                return false;
            }
            return true;
        }


        public void Delete(int id)
        {
            _reportContext.RemoveById<Dataset>(id);
            _reportContext.SaveChanges();
        }

        public List<string> Validate(BusinessIntelligenceDto dto)
        {
            List<string> errors = new List<string>();

            if (_reportContext.Datasets.Where(w => w.DatasetName == dto.DatasetName &&
                                                                        w.DatasetCategories.Any(x => dto.DatasetCategoryIds.Contains(x.Id)) &&
                                                                        w.DatasetType == GlobalConstants.DataEntityCodes.REPORT)?.Count() > 0)
            {
                errors.Add("Dataset name already exists within category");
            }

            return errors;
        }

        #endregion

        #region "Private Functions"

        private void UpdateDatasetFileConfig(BusinessIntelligenceDto dto, Dataset ds)
        {
            MapDatasetFileConig(dto, ds);
        }
        private void CreateDatasetFileConfig(BusinessIntelligenceDto dto, Dataset ds)
        {
            ds.DatasetFileConfigs = new List<DatasetFileConfig> { new DatasetFileConfig() };
            MapDatasetFileConig(dto, ds);
        }

        private DatasetFileConfig MapDatasetFileConig(BusinessIntelligenceDto dto, Dataset ds)
        {

            ds.DatasetFileConfigs.First().Name = dto.DatasetName;
            ds.DatasetFileConfigs.First().Description = dto.DatasetDesc;
            ds.DatasetFileConfigs.First().FileTypeId = dto.FileTypeId;
            ds.DatasetFileConfigs.First().ParentDataset = ds;
            ds.DatasetFileConfigs.First().DatasetScopeType = _reportContext.DatasetScopeTypes.Where(w => w.Name == "Point-in-Time").FirstOrDefault();
            ds.DatasetFileConfigs.First().FileExtension = _reportContext.FileExtensions.Where(w => w.Name == "ANY").FirstOrDefault();

            return ds.DatasetFileConfigs.First();
        }

        private void CreateDataset(BusinessIntelligenceDto dto)
        {
            Dataset ds = MapDataset(dto, new Dataset());
            CreateDatasetFileConfig(dto, ds);
            _reportContext.Add(ds);
        }

        private void UpdateDataset(BusinessIntelligenceDto dto, Dataset ds)
        {
            MapDataset(dto, ds);
        }

        private Dataset MapDataset(BusinessIntelligenceDto dto, Dataset ds)
        {
            ds.DatasetCategories = _reportContext.Categories.Where(x => dto.DatasetCategoryIds.Contains(x.Id)).ToList();
            ds.DatasetName = dto.DatasetName;
            ds.DatasetDesc = dto.DatasetDesc;
            ds.CreationUserName = dto.CreationUserName;
            ds.PrimaryOwnerId = dto.PrimaryOwnerId; //done on purpose since namming flipped.
            ds.UploadUserName = dto.UploadUserName;
            ds.OriginationCode = Enum.GetName(typeof(DatasetOriginationCode), 1);  //All reports are internal
            ds.DatasetDtm = dto.DatasetDtm;
            ds.ChangedDtm = dto.ChangedDtm;
            ds.DatasetType = GlobalConstants.DataEntityCodes.REPORT;
            ds.IsSensitive = false;
            ds.CanDisplay = true;
            ds.Metadata = new DatasetMetadata()
            {
                ReportMetadata = new ReportMetadata()
                {
                    Location = dto.Location,
                    LocationType = dto.LocationType,
                    Frequency = dto.FrequencyId
                }
            };
            ds.Tags = _reportContext.Tags.Where(x => dto.TagIds.Contains(x.TagId.ToString())).ToList();

            return ds;
        }

        //could probably be an extension.
        private void MapToDto(Dataset ds, BusinessIntelligenceDto dto)
        {
            string userDisplayname = _userService.GetByAssociateId(ds.PrimaryOwnerId)?.DisplayName;
           
                dto.DatasetId = ds.DatasetId;
                dto.DatasetCategoryIds = ds.DatasetCategories.Select(x => x.Id).ToList();
                dto.DatasetName = ds.DatasetName;
                dto.DatasetDesc = ds.DatasetDesc;
                dto.PrimaryOwnerName = (string.IsNullOrWhiteSpace(userDisplayname) ? ds.PrimaryOwnerId : userDisplayname);
                dto.PrimaryOwnerId = ds.PrimaryOwnerId;
                dto.CreationUserName = ds.CreationUserName;
                dto.UploadUserName = ds.UploadUserName;
                dto.DatasetDtm = ds.DatasetDtm;
                dto.ChangedDtm = ds.ChangedDtm;
                dto.S3Key = ds.S3Key;
                dto.IsSensitive = ds.IsSensitive;
                dto.DatasetType = ds.DatasetType;
                dto.Location = ds.Metadata.ReportMetadata.Location;
                dto.LocationType = ds.Metadata.ReportMetadata.LocationType;
                dto.FrequencyId = ds.Metadata.ReportMetadata.Frequency;
                dto.TagIds = ds.Tags.Select(x => x.TagId.ToString()).ToList();
                dto.FileTypeId = ds.DatasetFileConfigs.First().FileTypeId;
                dto.CanDisplay = ds.CanDisplay;
            dto.MailtoLink = "mailto:?Subject=Business%20Intelligence%20Exhibit%20-%20" + ds.DatasetName + "&body=%0D%0A" + Configuration.Config.GetHostSetting("SentryDataBaseUrl") + "/BusinessIntelligence/Detail/" + ds.DatasetId;

           // return dto;
        }

        private void MapToDetailDto(Dataset ds, BusinessIntelligenceDetailDto dto)
        {
            MapToDto(ds, dto);

            IApplicationUser user = _userService.GetCurrentUser();
            dto.IsFavorite = ds.Favorities.Any(w => w.UserId == user.AssociateId);
            dto.ObjectType = dto.DatasetType;
            dto.IsSubscribed = _reportContext.IsUserSubscribedToDataset(user.AssociateId, ds.DatasetId);
            dto.AmountOfSubscriptions = _reportContext.GetAllUserSubscriptionsForDataset(user.AssociateId, ds.DatasetId).Count;
            dto.Views = _reportContext.Events.Where(x => x.EventType.Description == GlobalConstants.EventType.VIEWED && x.Dataset == ds.DatasetId).Count();
            dto.FrequencyDescription = Enum.GetName(typeof(ReportFrequency), ds.Metadata.ReportMetadata.Frequency) ?? "Not Specified";
            dto.TagNames = ds.Tags.Select(x => x.Name).ToList();
            dto.CanManageReport = user.CanManageReports;
            dto.CategoryColor = ds.DatasetCategories.Count == 1 ? ds.DatasetCategories.First().Color : "gray";
            dto.CategoryNames = ds.DatasetCategories.Select(x => x.Name).ToList();

            //return dto;
        }

        #endregion

    }
}
