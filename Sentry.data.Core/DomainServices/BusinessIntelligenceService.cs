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
                //UpdateDatasetFileConfig(dto, ds);

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

            Image curImage;

            //https://stackoverflow.com/questions/1582285/how-to-remove-elements-from-a-generic-list-while-iterating-over-it
            for (int i = dto.Images.Count - 1; i >= 0; i--)
            {
                ImageDto img = dto.Images[i];
                bool updatesDetected = false;

                // Image marked for delete
                if (img.DeleteImage == true)
                {
                    DeleteImage(img);
                    if(ds.Images.Any(w => w.ImageId == img.ImageId))
                    {
                        Image removeimg = ds.Images.First(w => w.ImageId == img.ImageId);
                        ds.Images.Remove(removeimg);
                        //dto.Images.Remove(img);
                    }
                }
                //New Image
                else if (img.ImageId == 0)
                {
                    CreateImage(ds, img);
                }
                //Existing image
                else
                {
                    curImage = ds.Images.FirstOrDefault(w => w.ImageId == img.ImageId);

                    if (curImage.Sort != img.sortOrder) { curImage.Sort = img.sortOrder; updatesDetected = true; }

                    //Updated image data
                    if (img.Data != null)
                    {
                        curImage.FileName = img.FileName;
                        curImage.FileExtension = img.FileExtension;
                        curImage.ContentType = img.ContentType;

                        //Reuse current image location
                        img.StorageBucketName = curImage.StorageBucketName;
                        img.StorageKey = curImage.StorageKey;
                        img.StoragePrefix = curImage.StoragePrefix;
                        UploadImage(img);

                        updatesDetected = true;
                    }

                    if (updatesDetected) { curImage.UploadDate = DateTime.Now; };
                }
            }
        }


        public void Delete(int id)
        {
            Dataset exhibit = _datasetContext.GetById<Dataset>(id);

            foreach (Favorite fav in exhibit.Favorities)
            {
                _datasetContext.RemoveById<Favorite>(fav.FavoriteId);
            }

            RemoveAllImages(id);
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
                        else if (ex.Message.Contains("because it is being used by another process"))
                        {
                            Logger.Error("Exhibit validation OpenRead test could be executed, file in use", ex);
                        }
                        else
                        {
                            Logger.Error($"Exhibit Validation Exception - Creator:{dto.CreationUserId} ExhibitName:{dto.DatasetName}", ex);
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


        public List<FavoriteDto> GetDatasetFavoritesDto(int id)
        {
            return _datasetContext.Favorites.Where(w => w.DatasetId == id).Select(s => MapToDto(s)).ToList();
        }
        #endregion

        #region "Private Functions"

        /// <summary>
        /// Deletes an image file from storage.
        /// </summary>
        /// <param name="img"></param>
        private void DeleteImage(ImageDto img)
        {
            ObjectKeyVersion version = null;

            Logger.Info($"Image Delete Issued - Id:{img.ImageId} Key:{img.StorageKey}");
            try
            {
                version = _s3ServiceProvider.MarkDeleted(img.StorageKey);
                Logger.Info($"Image Delete Successful - Id:{img.ImageId} Key:{version.key} DeleteMarker:{version.versionId}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Image Delete Failed - Id:{img.ImageId} Key:{img.StorageKey}", ex);
            }
        }

        /// <summary>
        /// Deletes an image file from storage.
        /// </summary>
        /// <param name="img"></param>
        private void DeleteImage(Image img)
        {
            ObjectKeyVersion version = null;

            Logger.Info($"Image Delete Issued - Id:{img.ImageId} Key:{img.StorageKey}");
            try
            {
                version = _s3ServiceProvider.MarkDeleted(img.StorageKey);
                Logger.Info($"Image Delete Successful - Id:{img.ImageId} Key:{version.key} DeleteMarker:{version.versionId}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Image Delete Failed - Id:{img.ImageId} Key:{img.StorageKey}", ex);
            }
        }

        /// <summary>
        /// Deletes associated images, from storage and metadata, assossicated with dataset.
        /// </summary>
        /// <param name="id">Dataset Id</param>
        private void RemoveAllImages(int id)
        {
            Dataset ds = _datasetContext.GetById<Dataset>(id);
            foreach (Image img in ds.Images)
            {
                DeleteImage(img);
                _datasetContext.RemoveById<Image>(img.ImageId);
            }
        }


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

        private DatasetFileConfig CreateDatasetFileConfig(BusinessIntelligenceDto dto)
        {
            return new DatasetFileConfig()
            {
                Name = dto.DatasetName,
                Description = dto.DatasetDesc,
                FileTypeId = dto.FileTypeId,
                ParentDataset = _datasetContext.GetById<Dataset>(dto.DatasetId),
                DatasetScopeType = _datasetContext.DatasetScopeTypes.Where(w => w.Name == "Point-in-Time").FirstOrDefault(),
                FileExtension = _datasetContext.FileExtensions.Where(w => w.Name == GlobalConstants.ExtensionNames.ANY).FirstOrDefault(),
                ObjectStatus = GlobalEnums.ObjectStatusEnum.Active,
                DeleteIssuer = null,
                DeleteIssueDTM = DateTime.MaxValue
            };
        }

        private void CreateDataset(BusinessIntelligenceDto dto)
        {
            Dataset ds = MapDataset(dto, new Dataset());
            _datasetContext.Add(ds);
            dto.DatasetId = ds.DatasetId;

            var config = CreateDatasetFileConfig(dto);
            _datasetContext.Add(config);
            ds.DatasetFileConfigs = new List<DatasetFileConfig>() { config };

            CreateImages(dto, ds);
        }

        private void CreateImages(BusinessIntelligenceDto dto, Dataset ds)
        {
            List<Image> ImageList = new List<Image>();
            ds.Images = ImageList;
            if (dto.Images.Count > 0)
            {
                //Exclude any images marked for delete
                foreach (var image in dto.Images.Where(w => !w.DeleteImage))
                {
                    CreateImage(ds, image);
                }
            }
        }

        private void CreateImage(Dataset ds, ImageDto image)
        {
            //UploadImage(image);
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
            ds.DeleteInd = false;
            ds.DeleteIssueDTM = DateTime.MaxValue;
            ds.Asset = _datasetContext.Assets.FirstOrDefault(da => da.SaidKeyCode == "DATA");

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
                UploadDate = DateTime.Now,
                StorageBucketName = dto.StorageBucketName,
                StoragePrefix = dto.StoragePrefix,
                StorageKey = dto.StorageKey,
                Sort = dto.sortOrder,
            };

            return img;
        }

        //could probably be an extension.
        private void MapToDto(Dataset ds, BusinessIntelligenceDto dto)
        {
            IApplicationUser contact = (ds.PrimaryContactId == "000000")? null : _userService.GetByAssociateId(ds.PrimaryContactId);
            IApplicationUser uploaded = _userService.GetByAssociateId(ds.UploadUserName);

            dto.Security = _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());
            dto.PrimaryContactId = ds.PrimaryContactId;
            dto.IsSecured = ds.IsSecured;

            dto.DatasetId = ds.DatasetId;
            dto.DatasetCategoryIds = ds.DatasetCategories.Select(x => x.Id).ToList();
            dto.DatasetBusinessUnitIds = ds.BusinessUnits.Select(x => x.Id).ToList();
            dto.DatasetFunctionIds = ds.DatasetFunctions.Select(x => x.Id).ToList();
            dto.DatasetName = ds.DatasetName;
            dto.DatasetDesc = ds.DatasetDesc;
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
            dto.MailtoLink = "mailto:?Subject=Business%20Intelligence%20Exhibit%20-%20" + Uri.EscapeDataString(ds.DatasetName) + "&body=%0D%0A" + Configuration.Config.GetHostSetting("SentryDataBaseUrl") + "/BusinessIntelligence/Detail/" + ds.DatasetId;
            dto.ReportLink = (ds.DatasetFileConfigs.First().FileTypeId == (int)ReportType.BusinessObjects && ds.Metadata.ReportMetadata.GetLatest) ? ds.Metadata.ReportMetadata.Location + GlobalConstants.BusinessObjectExhibit.GET_LATEST_URL_PARAMETER : ds.Metadata.ReportMetadata.Location;
            dto.ContactIds = (ds.Metadata.ReportMetadata.Contacts != null)? ds.Metadata.ReportMetadata.Contacts.ToList() : new List<string>();
            dto.ContactDetails = (ds.Metadata.ReportMetadata.Contacts != null)? MapContactsToDto(ds.Metadata.ReportMetadata.Contacts) : new List<ContactInfoDto>();
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

        private FavoriteDto MapToDto(Favorite fav)
        {
            IApplicationUser user = _userService.GetByAssociateId(fav.UserId);
            return new FavoriteDto()
            {
                UserId = fav.UserId,
                UserEmail = user.EmailAddress,
                UserDisplayName = user.DisplayName
            };
        }

        private List<ImageDto> MapToDto(IList<Image> images)
        {
            List<ImageDto> dtoList = new List<ImageDto>();
            if (images != null)
            {
                foreach (Image img in images)
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
