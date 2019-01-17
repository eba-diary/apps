using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sentry.Core;

namespace Sentry.data.Core
{
    public class BusinessIntelligenceService : IBusinessIntelligenceService
    {

        private readonly IReportContext _reportContext;
        private readonly UserService _userService;

        public BusinessIntelligenceService(IReportContext reportContext, UserService userService)
        {
            _reportContext = reportContext;
            _userService = userService;
        }

        #region "Public Functions"


        public BusinessIntelligenceDto GetBusinessIntelligenceDto(int datasetId)
        {
            return MapToDto(_reportContext.GetById<Dataset>(datasetId));
        }

        public BusinessIntelligenceHomeDto GetHomeDto()
        {
            return new BusinessIntelligenceHomeDto()
            {
                DatasetCount = _reportContext.GetReportCount(),
                Categories = _reportContext.Categories.ToList(),
                CanManageReports = _userService.GetCurrentUser().CanManageReports
            };
        }

        public bool CreateAndSaveBusinessIntelligenceDataset(BusinessIntelligenceDto dto)
        {
            try
            {
                Dataset ds = CreatAndSaveDataset(dto);

                DatasetFileConfig dfc = CreateDatasetFileConig(dto, ds);
            }
            catch (Exception ex)
            {
                //probably add some logging.
                return false;
            }
            return true;
        }
        

        public bool IsDatasetNameDuplicate(string datasetName)
        {
            return _reportContext.Datasets.Where(w => w.DatasetName == datasetName).Count() > 0;
        }


        public List<string> Validate(BusinessIntelligenceDto dto)
        {
            List<string> errors = new List<string>();

            if (IsDatasetNameDuplicate(dto.DatasetName))
            {
                errors.Add("Dataset name already exists within category");
            }

            try
            {
                //Determine schema of incoming file location (i.e. http, file, etc)
                Uri incomingPath = new Uri(dto.Location);
            }
            catch (Exception)
            {
                errors.Add("Invalid location value");
            }

            switch (dto.FileTypeId)
            {
                case (int)ReportType.Tableau:
                    if (!Regex.IsMatch(dto.Location.ToLower(), "^https://tableau.sentry.com"))
                    {
                        errors.Add("Tableau exhibits should begin with https://Tableau.sentry.com");
                    }
                    break;
                case (int)ReportType.Excel:
                    if (!Regex.IsMatch(dto.Location.ToLower(), "^\\\\\\\\(sentry.com\\\\share\\\\|sentry.com\\\\appfs)"))
                    {
                        errors.Add("Excel exhibits should begin with \\\\Sentry.com\\Share or \\\\Sentry.com\\appfs");
                    }
                    break;
            }

            return errors;
        }

        #endregion

        #region "Private Functions"

        private DatasetFileConfig CreateDatasetFileConig( BusinessIntelligenceDto dto, Dataset ds)
        {
            DatasetFileConfig dfc = new DatasetFileConfig()
            {
                ConfigId = dto.DatasetFileConfigIds.First(), //BIs only ever have one.
                Name = dto.DatasetName,
                Description = dto.DatasetDesc,
                FileTypeId = dto.FileTypeId,
                ParentDataset = ds,
                DatasetScopeType = _reportContext.DatasetScopeTypes.Where(w => w.Name == "Point-in-Time").FirstOrDefault(),
                FileExtension = _reportContext.FileExtensions.Where(w => w.Name == "ANY").FirstOrDefault()
            };

            DatasetFileConfig dfcToReturn = _reportContext.Merge(dfc);
            _reportContext.SaveChanges();

            return dfcToReturn;
        }

        private Dataset CreatAndSaveDataset(BusinessIntelligenceDto dto)
        {

            IApplicationUser user = _userService.GetCurrentUser();

            Dataset ds = new Dataset()
            {
                DatasetId = dto.DatasetId,
                DatasetCategories = _reportContext.Categories.Where(x => dto.DatasetCategoryIds.Contains(x.Id)).ToList(),
                DatasetName = dto.DatasetName,
                DatasetDesc = dto.DatasetDesc,
                DatasetInformation = dto.DatasetInformation,
                CreationUserName = user.DisplayName,
                SentryOwnerName = dto.SentryOwnerName,
                UploadUserName = user.AssociateId,
                OriginationCode = Enum.GetName(typeof(DatasetOriginationCode), 1),  //All reports are internal
                DatasetDtm = dto.DatasetDtm,
                ChangedDtm = dto.ChangedDtm,
                S3Key = "Blank S3 Key",
                IsSensitive =dto.IsSensitive,
                CanDisplay = dto.CanDisplay,
                DatasetType = dto.DatasetType,
                Metadata = new DatasetMetadata()
                {
                    ReportMetadata = new ReportMetadata()
                    {
                        Location = dto.Location,
                        LocationType = dto.LocationType,
                        Frequency = dto.FrequencyId
                    }
                },
                Tags = _reportContext.Tags.Where(x=> dto.TagIds.Contains(x.TagId)).ToList()
            };

            Dataset dsToreturn = _reportContext.Merge(ds);
            _reportContext.SaveChanges();

            return dsToreturn;
        }

        private BusinessIntelligenceDto MapToDto(Dataset ds)
        {
            if (ds is null) { return new BusinessIntelligenceDto(); }
            return new BusinessIntelligenceDto()
            {
                DatasetId = ds.DatasetId,
                DatasetCategoryIds = ds.DatasetCategories.Select(x=> x.Id).ToList(),
                DatasetName = ds.DatasetName,
                DatasetDesc = ds.DatasetDesc,
                DatasetInformation = ds.DatasetInformation,
                SentryOwnerName = ds.SentryOwnerName,
                DatasetDtm = ds.DatasetDtm,
                ChangedDtm = ds.ChangedDtm,
                S3Key = ds.S3Key,
                IsSensitive = ds.IsSensitive,
                CanDisplay = ds.CanDisplay,
                DatasetType = ds.DatasetType,
                Location = ds.Metadata.ReportMetadata.Location,
                LocationType = ds.Metadata.ReportMetadata.LocationType,
                FrequencyId = ds.Metadata.ReportMetadata.Frequency,
                TagIds = ds.Tags.Select(x=> x.TagId).ToList(),
                FileTypeId = ds.DatasetFileConfigs.First().FileTypeId
            };
        }

        #endregion

    }
}
