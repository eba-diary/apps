using System;

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
                DatasetName = model.DatasetName,
                DatasetDesc = model.DatasetDesc,
                DatasetInformation = model.DatasetInformation,
                SentryOwnerName = model.SentryOwnerName,
                DatasetDtm = CreateTime,
                ChangedDtm = CreateTime,
                S3Key = "Blank S3 Key", //why do this vs having a null value?
                IsSensitive = false,
                CanDisplay = true,
                DatasetType = Core.GlobalConstants.DataEntityTypes.REPORT,
                Location = model.Location,
                LocationType = new Uri(model.Location).Scheme,
                FrequencyId = model.FrequencyId.Value,
                FileTypeId = model.FileTypeId,
                TagIds = model.TagIds
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

    }
}