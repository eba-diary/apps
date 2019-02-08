using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sentry.Common.Logging;

namespace Sentry.data.Core
{
    public class BusinessIntelligenceService : IBusinessIntelligenceService
    {

        private readonly IDatasetContext _datasetContext;
        private readonly IEmailService _emailService;
        private readonly ISecurityService _securityService;
        private readonly UserService _userService;

        public BusinessIntelligenceService(IDatasetContext datasetContext, IEmailService emailService, ISecurityService securityService, UserService userService)
        {
            _datasetContext = datasetContext;
            _emailService = emailService;
            _userService = userService;
            _securityService = securityService;
        }

        #region "Public Functions"


        public BusinessIntelligenceDto GetBusinessIntelligenceDto(int datasetId)
        {
            BusinessIntelligenceDto dto = new BusinessIntelligenceDto();
            Dataset ds = _datasetContext.GetById<Dataset>(datasetId);
            MapToDto(ds, dto);
            return dto;
        }
        public BusinessIntelligenceDetailDto GetBusinessIntelligenceDetailDto(int datasetId)
        {
            BusinessIntelligenceDetailDto dto = new BusinessIntelligenceDetailDto();
            Dataset ds = _datasetContext.GetById<Dataset>(datasetId);

            MapToDetailDto(ds, dto);

            return dto;
        }

        public BusinessIntelligenceHomeDto GetHomeDto()
        {
            return new BusinessIntelligenceHomeDto()
            {
                DatasetCount = _datasetContext.GetReportCount(),
                Categories = _datasetContext.Categories.Where(x => x.ObjectType == GlobalConstants.DataEntityCodes.REPORT).ToList(),
                CanManageReports = _userService.GetCurrentUser().CanManageReports
            };
        }

        public bool CreateAndSaveBusinessIntelligence(BusinessIntelligenceDto dto)
        {
            try
            {
                CreateDataset(dto);

                _datasetContext.SaveChanges();
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
                Dataset ds = _datasetContext.GetById<Dataset>(dto.DatasetId);

                UpdateDataset(dto, ds);
                UpdateDatasetFileConfig(dto, ds);

                _datasetContext.SaveChanges();
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
            _datasetContext.RemoveById<Dataset>(id);
            _datasetContext.SaveChanges();
        }

        public List<string> Validate(BusinessIntelligenceDto dto)
        {
            List<string> errors = new List<string>();

            if (dto.DatasetId == 0 && _datasetContext.Datasets.Where(w => w.DatasetName == dto.DatasetName &&
                                                                        w.DatasetCategories.Any(x => dto.DatasetCategoryIds.Contains(x.Id)) &&
                                                                        w.DatasetType == GlobalConstants.DataEntityCodes.REPORT)?.Count() > 0)
            {
                errors.Add("Dataset name already exists within category");
            }

            if (dto.FileTypeId == (int)ReportType.Excel)
            {
                try
                {
                    int pos = dto.Location.LastIndexOf('\\');
                    string directory = dto.Location.Substring(0, pos);
                    Directory.GetAccessControl(directory);
                    File.GetAccessControl(dto.Location);
                    var file = File.OpenRead(dto.Location);
                    file.Close();
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Attempted to perform an unauthorized operation.")
                    {
                        errors.Add("This file can’t be accessed by data.sentry.com.  DSCSupport@sentry.com has been notified.  You will be contacted when the exhibit can be created.");

                        _emailService.SendInvalidReportLocationEmail(dto, _userService.GetCurrentUser().DisplayName);
                    }
                    else
                    {
                        errors.Add($"An error occured finding the file. Please verify the file path is correct or contact DSCSupport@sentry.com for assistance.");
                    }
                }
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

        private void MapDatasetFileConig(BusinessIntelligenceDto dto, Dataset ds)
        {
            ds.DatasetFileConfigs.First().Name = dto.DatasetName;
            ds.DatasetFileConfigs.First().Description = dto.DatasetDesc;
            ds.DatasetFileConfigs.First().FileTypeId = dto.FileTypeId;
            ds.DatasetFileConfigs.First().ParentDataset = ds;
            ds.DatasetFileConfigs.First().DatasetScopeType = _datasetContext.DatasetScopeTypes.Where(w => w.Name == "Point-in-Time").FirstOrDefault();
            ds.DatasetFileConfigs.First().FileExtension = _datasetContext.FileExtensions.Where(w => w.Name == "ANY").FirstOrDefault();
        }

        private void CreateDataset(BusinessIntelligenceDto dto)
        {
            Dataset ds = MapDataset(dto, new Dataset());
            CreateDatasetFileConfig(dto, ds);
            _datasetContext.Add(ds);
        }

        private void UpdateDataset(BusinessIntelligenceDto dto, Dataset ds)
        {
            MapDataset(dto, ds);
        }

        private Dataset MapDataset(BusinessIntelligenceDto dto, Dataset ds)
        {
            ds.DatasetCategories = _datasetContext.Categories.Where(x => dto.DatasetCategoryIds.Contains(x.Id)).ToList();
            ds.BusinessUnits = _datasetContext.BusinessUnits.Where(x => dto.DatasetBusinessUnitIds.Contains(x.Id)).ToList();
            ds.DatasetFunctions = _datasetContext.DatasetFunctions.Where(x => dto.DatasetFunctionIds.Contains(x.Id)).ToList();
            ds.DatasetName = dto.DatasetName;
            ds.DatasetDesc = dto.DatasetDesc;
            ds.CreationUserName = dto.CreationUserName;
            ds.PrimaryOwnerId = dto.PrimaryOwnerId;
            ds.PrimaryContactId = dto.PrimaryContactId;
            ds.UploadUserName = dto.UploadUserName;
            ds.OriginationCode = Enum.GetName(typeof(DatasetOriginationCode), 1);  //All reports are internal
            ds.DatasetDtm = dto.DatasetDtm;
            ds.ChangedDtm = dto.ChangedDtm;
            ds.DatasetType = GlobalConstants.DataEntityCodes.REPORT;
            ds.CanDisplay = true;
            ds.Metadata = new DatasetMetadata()
            {
                ReportMetadata = new ReportMetadata()
                {
                    Location = dto.Location,
                    LocationType = dto.LocationType,
                    Frequency = dto.FrequencyId,
                    GetLatest = dto.GetLatest
                }
            };
            ds.Tags = _datasetContext.Tags.Where(x => dto.TagIds.Contains(x.TagId.ToString())).ToList();

            return ds;
        }

        //could probably be an extension.
        private void MapToDto(Dataset ds, BusinessIntelligenceDto dto)
        {
            IApplicationUser owner = _userService.GetByAssociateId(ds.PrimaryOwnerId);
            IApplicationUser contact = _userService.GetByAssociateId(ds.PrimaryContactId);

            dto.Security = _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());
            dto.PrimaryOwnerId = ds.PrimaryOwnerId;
            dto.PrimaryContactId = ds.PrimaryContactId;
            dto.IsSecured = ds.IsSecured;

            dto.DatasetId = ds.DatasetId;
            dto.DatasetCategoryIds = ds.DatasetCategories.Select(x => x.Id).ToList();
            dto.DatasetBusinessUnitIds = ds.BusinessUnits.Select(x => x.Id).ToList();
            dto.DatasetFunctionIds = ds.DatasetFunctions.Select(x => x.Id).ToList();
            dto.DatasetName = ds.DatasetName;
            dto.DatasetDesc = ds.DatasetDesc;
            dto.PrimaryOwnerName = (owner != null ? owner.DisplayName : "Not Available");
            dto.PrimaryContactName = (contact != null ? contact.DisplayName : "Not Available");
            dto.PrimaryContactEmail = (contact != null ? contact.EmailAddress : "");

            dto.CreationUserName = ds.CreationUserName;
            dto.UploadUserName = ds.UploadUserName;
            dto.DatasetDtm = ds.DatasetDtm;
            dto.ChangedDtm = ds.ChangedDtm;
            dto.S3Key = ds.S3Key;
            dto.DatasetType = ds.DatasetType;
            dto.Location = ds.Metadata.ReportMetadata.Location;
            dto.LocationType = ds.Metadata.ReportMetadata.LocationType;
            dto.FrequencyId = ds.Metadata.ReportMetadata.Frequency;
            dto.TagIds = ds.Tags.Select(x => x.TagId.ToString()).ToList();
            dto.FileTypeId = ds.DatasetFileConfigs.First().FileTypeId;
            dto.GetLatest = ds.Metadata.ReportMetadata.GetLatest;
            dto.CanDisplay = ds.CanDisplay;
            dto.MailtoLink = "mailto:?Subject=Business%20Intelligence%20Exhibit%20-%20" + ds.DatasetName + "&body=%0D%0A" + Configuration.Config.GetHostSetting("SentryDataBaseUrl") + "/BusinessIntelligence/Detail/" + ds.DatasetId;
            dto.ReportLink = (ds.DatasetFileConfigs.First().FileTypeId == (int)ReportType.BusinessObjects && ds.Metadata.ReportMetadata.GetLatest) ? ds.Metadata.ReportMetadata.Location + GlobalConstants.BusinessObjectExhibit.GET_LATEST_URL_PARAMETER : ds.Metadata.ReportMetadata.Location;
        }

        private void MapToDetailDto(Dataset ds, BusinessIntelligenceDetailDto dto)
        {
            MapToDto(ds, dto);

            IApplicationUser user = _userService.GetCurrentUser();
            dto.IsFavorite = ds.Favorities.Any(w => w.UserId == user.AssociateId);
            dto.ObjectType = dto.DatasetType;
            dto.IsSubscribed = _datasetContext.IsUserSubscribedToDataset(user.AssociateId, ds.DatasetId);
            dto.AmountOfSubscriptions = _datasetContext.GetAllUserSubscriptionsForDataset(user.AssociateId, ds.DatasetId).Count;
            dto.Views = _datasetContext.Events.Where(x => x.EventType.Description == GlobalConstants.EventType.VIEWED && x.Dataset == ds.DatasetId).Count();
            dto.FrequencyDescription = Enum.GetName(typeof(ReportFrequency), ds.Metadata.ReportMetadata.Frequency) ?? "Not Specified";
            dto.TagNames = ds.Tags.Select(x => x.Name).ToList();
            dto.CategoryColor = ds.DatasetCategories.Count == 1 ? ds.DatasetCategories.First().Color : "darkgray";
            dto.CategoryNames = ds.DatasetCategories.Select(x => x.Name).ToList();
        }

        #endregion

    }
}
