using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace Sentry.data.Web
{
    public static class BusinessIntelligenceExtensions
    {


        public static Core.BusinessIntelligenceDto ToDto(this BusinessIntelligenceModel model)
        {
            DateTime CreateTime = DateTime.Now;
            Core.BusinessIntelligenceDto dto = new Core.BusinessIntelligenceDto()
            {
                DatasetId = model.DatasetId,
                DatasetCategoryIds = model.DatasetCategoryIds,
                DatasetBusinessUnitIds = (model.DatasetBusinessUnitIds == null) ? new List<int>() : model.DatasetBusinessUnitIds,
                DatasetFunctionIds = (model.DatasetFunctionIds == null) ? new List<int>() : model.DatasetFunctionIds,
                DatasetName = model.DatasetName,
                DatasetDesc = model.DatasetDesc,
                PrimaryContactId = model.PrimaryContactId,
                CreationUserId = model.CreationUserId,
                UploadUserId = model.UploadUserId,
                DatasetDtm = CreateTime,
                ChangedDtm = CreateTime,
                DatasetType = Core.GlobalConstants.DataEntityCodes.REPORT,
                ObjectType = Core.GlobalConstants.DataEntityCodes.REPORT, //whats the difference here?
                Location = model.Location,
                LocationType = new Uri(model.Location)?.Scheme,
                FrequencyId = model.FrequencyId.Value,
                FileTypeId = model.FileTypeId,
                GetLatest = model.GetLatest,
                TagIds = (model.TagIds == null) ? new List<string>() : model.TagIds?.Split(',').ToList(),
                ContactIds = model.ContactIds.Where(w => !String.IsNullOrWhiteSpace(w)).ToList(),
                AlternateContactEmail = model.AlternateContactEmail
            };

            ImagesToDto(model, dto);

            return dto;
        }


        public static BusinessIntelligenceHomeModel ToModel(this Core.BusinessIntelligenceHomeDto dto)
        {
            if (dto == null) { return new BusinessIntelligenceHomeModel(); }
            return new BusinessIntelligenceHomeModel()
            {
                CanEditDataset = dto.CanEditDataset,
                CanManageReports = dto.CanManageReports,
                CanUpload = dto.CanUpload,
                Categories = dto.Categories.ToModel(),
                DatasetCount = dto.DatasetCount,
            };
        }

        public static TagGroupModel ToModel(this Core.TagGroupDto dto)
        {
            return new TagGroupModel()
            {
                Name = dto.Name,
                Description = dto.Description,
                TagGroupId = dto.TagGroupId
            };
        }

        public static List<TagGroupModel> ToModel(this List<Core.TagGroupDto> dtoList)
        {
            List<TagGroupModel> modelList = new List<TagGroupModel>();
            foreach(Core.TagGroupDto dto in dtoList)
            {
                modelList.Add(dto.ToModel());
            }
            return modelList;
        }

        public static Core.TagDto ToDto(this TagModel model)
        {
            DateTime created = DateTime.Now;
            return new Core.TagDto()
            {
                TagId = model.TagId,
                TagName = model.TagName,
                Description = model.Description,
                TagGroupId = Int32.Parse(model.SelectedTagGroup),
                Created = created,
                CreatedBy = model.CreationUserId
            };
        }

        public static List<ContactInfoModel> ToModel(this List<Core.ContactInfoDto> dtoList)
        {
            List<ContactInfoModel> modelList = new List<ContactInfoModel>();
            if (dtoList != null)
            {
                foreach (Core.ContactInfoDto dto in dtoList)
                {
                    modelList.Add(dto.ToModel());
                }
            }
            return modelList;
        }

        public static ContactInfoModel ToModel(this Core.ContactInfoDto dto)
        {
            return new ContactInfoModel()
            {
                Id = dto.Id,
                Name = dto.Name,
                Email = dto.Email
            };
        }

        public static void ImagesToDto(BusinessIntelligenceModel model, Core.BusinessIntelligenceDto dto)
        {
            List<Core.ImageDto> imageArray = new List<Core.ImageDto>();

            for (int i = 0; i < model.Images.Count; i++)
            {
                imageArray.Add(ToDto(model.Images[i], model.Images[i].ImageFileData, i));
            }

            //foreach (ImageModel item in model.Images)
            //{
            //    imageArray.Add(ToDto(item, item.ImageFileData, item.sortOrder));
            //}

            //if (model.ImageFile_1 != null || model.Images.Any(s => s.sortOrder == 1))
            //{
            //    imageArray.Add(ToDto(model.Images.FirstOrDefault(w => w.sortOrder == 1), model.ImageFile_1, 1));
            //}
            //if (model.ImageFile_2 != null || model.Images.Any(s => s.sortOrder == 2))
            //{
            //    imageArray.Add(ToDto(model.Images.FirstOrDefault(w => w.sortOrder == 2), model.ImageFile_2, 2));
            //}
            //if (model.ImageFile_3 != null || model.Images.Any(s => s.sortOrder == 3))
            //{
            //    imageArray.Add(ToDto(model.Images.FirstOrDefault(w => w.sortOrder == 3), model.ImageFile_3, 3));
            //}

            dto.Images = imageArray;
        }

        public static Core.ImageDto ToDto(ImageModel model, System.Web.HttpPostedFileBase imageFile, int sort, bool tempfile = false)
        {
            Core.ImageDto dto = ToDto(imageFile, model);
            if (tempfile) { dto.GenerateStorageInfo(); }
            dto.sortOrder = sort;
            return dto;
        }

        public static Core.ImageDto ToDto(System.Web.HttpPostedFileBase imageFile, ImageModel model)
        {
            Core.ImageDto dto = new Core.ImageDto();

            dto.ImageId = model.ImageId;
            dto.sortOrder = model.sortOrder;
            dto.DeleteImage = model.deleteImage;
            dto.UploadDate = DateTime.Now;

            if (imageFile != null)
            {
                MemoryStream target = new MemoryStream();
                imageFile.InputStream.CopyTo(target);
                dto.Data = target.ToArray();
                dto.FileName = imageFile.FileName;
                dto.FileExtension = Path.GetExtension(imageFile.FileName);
                dto.ContentType = imageFile.ContentType;
                dto.FileLength = imageFile.ContentLength;
                dto.UploadDate = DateTime.Now;
            }
            else
            {
                dto.FileName = model.FileName;
                dto.FileExtension = model.FileExtension;
                dto.ContentType = model.ContentType;
                dto.StorageBucketName = model.StorageBucketName;
                dto.StorageKey = model.StorageKey;
                dto.StoragePrefix = model.StoragePrefix;
            }

            return dto;
        }

    }
}