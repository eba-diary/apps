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
            return MapToDto(_reportContext.GetById<Dataset>(datasetId));
        }
        public BusinessIntelligenceDetailDto GetBusinessIntelligenceDetailDto(int datasetId)
        {
            return MapToDetailDto(_reportContext.GetById<Dataset>(datasetId));
        }

        public BusinessIntelligenceHomeDto GetHomeDto()
        {
            return new BusinessIntelligenceHomeDto()
            {
                DatasetCount = _reportContext.GetReportCount(),
                Categories = _reportContext.Categories.Where(x => x.ObjectType == GlobalConstants.DataEntityTypes.REPORT).ToList(),
                CanManageReports = _userService.GetCurrentUser().CanManageReports
            };
        }

        public bool CreateAndSaveBusinessIntelligence(BusinessIntelligenceDto dto)
        {
            try
            {
                Dataset ds = CreatDataset(dto);
                _reportContext.Add(ds); //setting it back to ds so we can use the datasetId.

                DatasetFileConfig dfc = CreateDatasetFileConig(dto, ds);
                _reportContext.Add(dfc);

                //pull the save here last just incase anything happens while "merging", it will not commit.
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

                Dataset dsToSave = CreatDataset(dto, ds);
                DatasetFileConfig dfc = CreateDatasetFileConig(dto, ds);

                _reportContext.Merge(dsToSave);
                _reportContext.Merge(dfc);
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
                                                                        w.DatasetCategories.Any(x=> dto.DatasetCategoryIds.Contains(x.Id)) &&
                                                                        w.DatasetType == GlobalConstants.DataEntityTypes.REPORT)?.Count() > 0 )
            {
                errors.Add("Dataset name already exists within category");
            }

            return errors;
        }

        #endregion

        #region "Private Functions"

        private DatasetFileConfig CreateDatasetFileConig(BusinessIntelligenceDto dto, Dataset ds)
        {
            DatasetFileConfig dfc = new DatasetFileConfig();
            if (ds.DatasetFileConfigs?.Count > 0)
            {
                dfc = ds.DatasetFileConfigs.First();
            }

            dfc.Name = dto.DatasetName;
            dfc.Description = dto.DatasetDesc;
            dfc.FileTypeId = dto.FileTypeId;
            dfc.ParentDataset = ds;
            dfc.DatasetScopeType = _reportContext.DatasetScopeTypes.Where(w => w.Name == "Point-in-Time").FirstOrDefault();
            dfc.FileExtension = _reportContext.FileExtensions.Where(w => w.Name == "ANY").FirstOrDefault();

            return dfc;
        }

        private Dataset CreatDataset(BusinessIntelligenceDto dto, Dataset ds = null)
        {
            if(ds == null) { ds = new Dataset(); }

            ds.DatasetCategories = _reportContext.Categories.Where(x => dto.DatasetCategoryIds.Contains(x.Id)).ToList();
            ds.DatasetName = dto.DatasetName;
            ds.DatasetDesc = dto.DatasetDesc;
            ds.CreationUserName = dto.CreationUserName;
            ds.SentryOwnerName = dto.SentryOwnerId; //done on purpose since namming flipped.
            ds.UploadUserName = dto.UploadUserName;
            ds.OriginationCode = Enum.GetName(typeof(DatasetOriginationCode), 1);  //All reports are internal
            ds.DatasetDtm = dto.DatasetDtm;
            ds.ChangedDtm = dto.ChangedDtm;
            ds.DatasetType = GlobalConstants.DataEntityTypes.REPORT;
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
        private BusinessIntelligenceDto MapToDto(Dataset ds)
        {
            if (ds is null) { return new BusinessIntelligenceDto(); }
            return MapToDetailDto(ds, false);
        }

        private BusinessIntelligenceDetailDto MapToDetailDto(Dataset ds, bool mapDetails = true)
        {
            string userDisplayname = _userService.GetByAssociateId(ds.SentryOwnerName)?.DisplayName;
            BusinessIntelligenceDetailDto dto = new BusinessIntelligenceDetailDto()
            {
                DatasetId = ds.DatasetId,
                DatasetCategoryIds = ds.DatasetCategories.Select(x => x.Id).ToList(),
                DatasetName = ds.DatasetName,
                DatasetDesc = ds.DatasetDesc,
                SentryOwnerName = (string.IsNullOrWhiteSpace(userDisplayname) ? ds.SentryOwnerName : userDisplayname),
                SentryOwnerId = ds.SentryOwnerName,
                CreationUserName = ds.CreationUserName,
                UploadUserName = ds.UploadUserName,
                DatasetDtm = ds.DatasetDtm,
                ChangedDtm = ds.ChangedDtm,
                S3Key = ds.S3Key,
                IsSensitive = ds.IsSensitive,
                DatasetType = ds.DatasetType,
                Location = ds.Metadata.ReportMetadata.Location,
                LocationType = ds.Metadata.ReportMetadata.LocationType,
                FrequencyId = ds.Metadata.ReportMetadata.Frequency,
                TagIds = ds.Tags.Select(x => x.TagId.ToString()).ToList(),
                FileTypeId = ds.DatasetFileConfigs.First().FileTypeId,
                CanDisplay = ds.CanDisplay,
                MailtoLink = "mailto:?Subject=Business%20Intelligence%20Exhibit%20-%20" + ds.DatasetName + "&body=%0D%0A" + Configuration.Config.GetHostSetting("SentryDataBaseUrl") + "/BusinessIntelligence/Detail/" + ds.DatasetId
        };


            //Details
            if (mapDetails)
            {
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
            }

            return dto;
        }

        #endregion

    }
}
