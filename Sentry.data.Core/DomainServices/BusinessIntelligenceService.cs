using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
        private readonly IS3ServiceProvider _s3ServiceProvider;

        public BusinessIntelligenceService(IDatasetContext datasetContext, 
            IEmailService emailService, ISecurityService securityService, 
            UserService userService, IS3ServiceProvider s3ServiceProvider)
        {
            _datasetContext = datasetContext;
            _emailService = emailService;
            _userService = userService;
            _securityService = securityService;
            _s3ServiceProvider = s3ServiceProvider;
        }

        #region "Public Functions"


        public UserSecurity GetUserSecurityById(int datasetId)
        {
            Dataset ds = _datasetContext.Datasets.Where(x => x.DatasetId == datasetId && x.CanDisplay).FetchSecurityTree(_datasetContext).FirstOrDefault();
            
            return _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());
        }

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
                //CreateImages(dto);
                _datasetContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("Error saving business intelligence - ", ex);
                return false;
            }
            return true;
        }

        private void CreateImages(BusinessIntelligenceDto dto)
        {
            foreach (var img in dto.Images)
            {
                UploadImage(img);
            }
        }

        private void UploadImage(ImageDto dto)
        {
            using (MemoryStream stream = new MemoryStream(dto.Data))
            {
            dto.StorageETag = _s3ServiceProvider.UploadDataFile(stream, dto.StorageKey);
            }       
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

        private void UpdateImages(BusinessIntelligenceDto dto, Dataset ds)
        {
            if (dto.Images.Any())
            {
                foreach(ImageDto img in dto.Images)
                {
                    Image curImage = ds.Images.FirstOrDefault(w => w.Sort == img.sortOrder);
                    if(curImage != null)
                    {
                        curImage.UploadDate = DateTime.Now;
                        curImage.FileExtension = img.FileExtension;
                        curImage.FileName = img.FileName;
                        curImage.ContentType = img.ContentType;

                        //Reuse current image location
                        img.StorageBucketName = curImage.StorageBucketName;
                        img.StorageKey = curImage.StorageKey;
                        img.StoragePrefix = curImage.StoragePrefix;
                        UploadImage(img);
                    }
                    else
                    {
                        CreateImage(ds, img);
                    }
                }
            }
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
                /*Excel files can reside on network shares or sharepoint
                    - If Uri has file scheme, we need to ensure DSC service account has permissions to download file
                        and is valid location.
                    - All other Uri schemes (at this point would be http, for sharepoint) we do not need to perform any checks
                        as we are assuming the external system will generate error if user does not have permissions.
                */ 
                Uri fileUri = new Uri(dto.Location);
                if (fileUri.Scheme == Uri.UriSchemeFile)
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
            }

            return errors;
        }

        public List<KeyValuePair<string,string>> GetAllTagGroups()
        {
            return _datasetContext.TagGroups.Select(s => new KeyValuePair<string,string>(s.TagGroupId.ToString(), s.Name)).OrderBy(o => o.Value).ToList();
        }

        public byte[] GetImageData(string url, int? t)
        {
            MemoryStream target = new MemoryStream();
            using (Stream s = _s3ServiceProvider.GetObject(url, null))
            {
                s.CopyTo(target);
            }
            if (t == null)
            {
                return target.ToArray();
            }
            else
            {
                return getThumbNail(target.ToArray(), (int)t);
            }
        }

        public bool SaveTemporaryPreviewImage(ImageDto dto)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(dto.Data))
                {
                    dto.StorageETag = _s3ServiceProvider.UploadDataFile(stream, dto.StorageKey);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exhibit Temp image upload failure", ex);
                return false;
            }

            return true;            
        }

        #endregion

        #region "Private Functions"

        private byte[] getThumbNail(byte[] data, int multi = 1)
        {
            using (var file = new MemoryStream(data))
            {
                int width = 200 * multi;
                using (var image = System.Drawing.Image.FromStream(file, true, true)) /* Creates Image from specified data stream */
                {
                    int X = image.Width;
                    int Y = image.Height;
                    int height = (int)((width * Y) / X);
                    using (var thumb = image.GetThumbnailImage(width, height, () => false, IntPtr.Zero))
                    {
                        var jpgInfo = ImageCodecInfo.GetImageEncoders()
                                       .Where(codecInfo => codecInfo.MimeType == "image/png").First();
                        using (var encParams = new EncoderParameters(1))
                        {
                            using (var samllfile = new MemoryStream())
                            {
                                long quality = 100;
                                encParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                                thumb.Save(samllfile, jpgInfo, encParams);
                                return samllfile.ToArray();
                            }
                        };
                    };
                };
            };
        }

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
            CreateImages(dto, ds);

            _datasetContext.Add(ds);
        }

        private void CreateImages(BusinessIntelligenceDto dto, Dataset ds)
        {
            List<Image> ImageList = new List<Image>();
            ds.Images = ImageList;
            if (dto.Images.Count > 0)
            {
                foreach (var image in dto.Images)
                {
                    CreateImage(ds, image);
                }
            }
        }

        private void CreateImage(Dataset ds, ImageDto image)
        {
            UploadImage(image);
            Image img = MapImage(image, ds);
            _datasetContext.Add(img);

            ds.Images.Add(img);
        }

        private void UpdateDataset(BusinessIntelligenceDto dto, Dataset ds)
        {
            MapDataset(dto, ds);
            UpdateImages(dto, ds);
        }

        private Dataset MapDataset(BusinessIntelligenceDto dto, Dataset ds)
        {
            ds.DatasetCategories = _datasetContext.Categories.Where(x => dto.DatasetCategoryIds.Contains(x.Id)).ToList();
            ds.BusinessUnits = _datasetContext.BusinessUnits.Where(x => dto.DatasetBusinessUnitIds.Contains(x.Id)).ToList();
            ds.DatasetFunctions = _datasetContext.DatasetFunctions.Where(x => dto.DatasetFunctionIds.Contains(x.Id)).ToList();
            ds.DatasetName = dto.DatasetName;
            ds.DatasetDesc = dto.DatasetDesc;
            ds.CreationUserName = dto.CreationUserId;
            ds.PrimaryOwnerId = dto.PrimaryOwnerId ?? "000000";
            ds.PrimaryContactId = dto.PrimaryContactId ?? "000000";
            ds.UploadUserName = dto.UploadUserId;
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
                    GetLatest = dto.GetLatest,
                    Contacts = dto.ContactIds
                }
            };
            ds.Tags = _datasetContext.Tags.Where(x => dto.TagIds.Contains(x.TagId.ToString())).ToList();            
                
            return ds;
        }

        private Image MapImage(ImageDto dto, Dataset ds)
        {
            Image img = new Image()
            {
                FileName = dto.FileName,
                FileExtension = dto.FileExtension,
                ParentDataset = ds,
                ContentType = dto.ContentType,
                UploadDate = dto.UploadDate,
                StorageBucketName = dto.StorageBucketName,
                StoragePrefix = dto.StoragePrefix,
                StorageKey = dto.StorageKey,
                Sort = dto.sortOrder
            };

            return img;
        }

        //could probably be an extension.
        private void MapToDto(Dataset ds, BusinessIntelligenceDto dto)
        {
            IApplicationUser owner = (ds.PrimaryOwnerId == "000000")? null : _userService.GetByAssociateId(ds.PrimaryOwnerId);
            IApplicationUser contact = (ds.PrimaryContactId == "000000")? null : _userService.GetByAssociateId(ds.PrimaryContactId);
            IApplicationUser uploaded = _userService.GetByAssociateId(ds.UploadUserName);

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

            dto.CreationUserId = ds.CreationUserName;
            dto.CreationUserName = ds.CreationUserName;
            dto.UploadUserId = ds.UploadUserName;
            dto.UploadUserName = (uploaded != null ? uploaded?.DisplayName : "Not Available");

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
            dto.ContactIds = (ds.Metadata.ReportMetadata.Contacts != null)? ds.Metadata.ReportMetadata.Contacts.ToList() : new List<string>();
            dto.Images = MapToDto(ds.Images);
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
            dto.FunctionNames = ds.DatasetFunctions.Select(x => x.Name).ToList();
            dto.BusinessUnitNames = ds.BusinessUnits.Select(x => x.Name).ToList();
            dto.CategoryColor = ds.DatasetCategories.Count == 1 ? ds.DatasetCategories.First().Color : "darkgray";
            dto.CategoryNames = ds.DatasetCategories.Select(x => x.Name).ToList();
            dto.Images = ds.Images.Select(x => x.StorageKey).ToList();
        }

        private void MapToDto(List<TagGroup> tagGroups, List<TagGroupDto> dto)
        {
            foreach(TagGroup group in tagGroups)
            {
                TagGroupDto groupDto = new TagGroupDto();
                MapToDto(group, groupDto);
                dto.Add(groupDto);
            }
        }

        private void MapToDto(TagGroup group, TagGroupDto dto)
        {
            dto.Name = group.Name;
            dto.Description = group.Description;
            dto.TagGroupId = group.TagGroupId;
        }

        private List<ContactInfoDto> MapContactsToDto(IList<string> contacts)
        {
            List<ContactInfoDto> contactList = new List<ContactInfoDto>();
            foreach (string contact in contacts)
            {
                IApplicationUser user = _userService.GetByAssociateId(contact);
                contactList.Add(new ContactInfoDto() {
                    Id = user.AssociateId,
                    Name = user.DisplayName,
                    Email = user.EmailAddress
                });                
            }
            return contactList;
        }
        
        private List<ImageDto> MapToDto(IList<Image> images)
        {
            List<ImageDto> dtoList = new List<ImageDto>();
            if (images != null)
            {
                foreach(Image img in images)
                {
                    dtoList.Add(new ImageDto()
                    {
                        ImageId = img.ImageId,
                        ContentType = img.ContentType,
                        DatasetId = img.ParentDataset.DatasetId,
                        FileExtension = img.FileExtension,
                        FileName = img.FileName,
                        sortOrder = img.Sort,
                        StorageBucketName = img.StorageBucketName,
                        StorageKey = img.StorageKey,
                        StoragePrefix = img.StoragePrefix,
                        UploadDate = img.UploadDate
                    });
                };                                 
            };
            return dtoList;
        }
        
        #endregion

    }
}
