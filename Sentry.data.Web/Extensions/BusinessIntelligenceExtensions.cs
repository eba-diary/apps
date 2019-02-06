using System;
using System.Collections.Generic;
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
                DatasetBusinessUnitIds = model.DatasetBusinessUnitIds,
                DatasetFunctionIds = model.DatasetFunctionIds,
                DatasetName = model.DatasetName,
                DatasetDesc = model.DatasetDesc,
                SentryOwnerId = model.SentryOwnerId,
                CreationUserName = model.CreationUserName,
                UploadUserName = model.UploadUserName,
                DatasetDtm = CreateTime,
                ChangedDtm = CreateTime,
                DatasetType = Core.GlobalConstants.DataEntityTypes.REPORT,
                ObjectType = Core.GlobalConstants.DataEntityTypes.REPORT, //whats the difference here?
                Location = model.Location,
                LocationType = new Uri(model.Location)?.Scheme,
                FrequencyId = model.FrequencyId.Value,
                FileTypeId = model.FileTypeId,
                GetLatest = model.GetLatest,
                TagIds = (model.TagIds == null) ? new List<string>() : model.TagIds?.Split(',').ToList()
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