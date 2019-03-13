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
            return new Core.BusinessIntelligenceDto()
            {
                DatasetId = model.DatasetId,
                DatasetCategoryIds = model.DatasetCategoryIds,
                DatasetBusinessUnitIds = (model.DatasetBusinessUnitIds == null) ? new List<int>() : model.DatasetBusinessUnitIds,
                DatasetFunctionIds = (model.DatasetFunctionIds == null) ? new List<int>() : model.DatasetFunctionIds,
                DatasetName = model.DatasetName,
                DatasetDesc = model.DatasetDesc,
                PrimaryOwnerId = model.PrimaryOwnerId,
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
                Images = model.file.ToDto()
            };
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

        public static List<Core.ImageDto> ToDto(this List<System.Web.HttpPostedFileBase> imageList)
        {
            List<Core.ImageDto> imageArray = new List<Core.ImageDto>();
            foreach (var image in imageList)
            {
                Core.ImageDto dto = image.ToDto();
                dto.GenerateStorageInfo();
                imageArray.Add(dto);                
            }

            return imageArray;
        }

        public static Core.ImageDto ToDto(this System.Web.HttpPostedFileBase imageFile)
        {
            MemoryStream target = new MemoryStream();
            imageFile.InputStream.CopyTo(target);
            return new Core.ImageDto()
            {
                Data = target.ToArray(),
                FileName = imageFile.FileName,
                FileExtension = Path.GetExtension(imageFile.FileName),
                ContentType = imageFile.ContentType,
                UploadDate = DateTime.Now,
                FileLength = imageFile.ContentLength
            };
        }

    }
}