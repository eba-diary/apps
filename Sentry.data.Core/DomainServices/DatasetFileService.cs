using Sentry.Common.Logging;
using Sentry.data.Core.Entities.DataProcessing;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Helpers.Paginate;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public class DatasetFileService : IDatasetFileService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly ISecurityService _securityService;
        private readonly IUserService _userService;
        private readonly IS3ServiceProvider _s3ServiceProvider;

        public DatasetFileService(IDatasetContext datasetContext, ISecurityService securityService,
                                    IUserService userService, IS3ServiceProvider s3ServiceProvider)
        {
            _datasetContext = datasetContext;
            _securityService = securityService;
            _userService = userService;
            _s3ServiceProvider = s3ServiceProvider;
        }

        public PagedList<DatasetFileDto> GetAllDatasetFileDtoBySchema(int schemaId, PageParameters pageParameters)
        {
            DatasetFileConfig config = _datasetContext.DatasetFileConfigs.FirstOrDefault(w => w.Schema.SchemaId == schemaId);

            if (config == null)
            {
                throw new SchemaNotFoundException();
            }

            UserSecurity us = _securityService.GetUserSecurity(config.ParentDataset, _userService.GetCurrentUser());
            if (!us.CanViewFullDataset)
            {
                throw new DatasetUnauthorizedAccessException();
            }

            PagedList<DatasetFile> files = PagedList<DatasetFile>.ToPagedList(_datasetContext.DatasetFileStatusActive
                                                .Where(x => x.Schema == config.Schema)
                                                .OrderBy(o => o.DatasetFileId),
                                                pageParameters.PageNumber, pageParameters.PageSize);

            return new PagedList<DatasetFileDto>(files.ToDto().ToList(), files.TotalCount, files.CurrentPage, files.PageSize);
        }

        public void UpdateAndSave(DatasetFileDto dto)
        {
            IApplicationUser user = _userService.GetCurrentUser();
            if (!user.IsAdmin)
            {
                throw new DataFileUnauthorizedException();
            }

            DatasetFile dataFile = _datasetContext.GetById<DatasetFile>(dto.DatasetFileId);
            if (dataFile == null)
            {
                throw new DataFileNotFoundException();
            }
            if (dataFile.Dataset.DatasetId != dto.Dataset)
            {
                throw new DatasetNotFoundException("DataFile is not associated specified DatasetId");
            }
            if ((dataFile.Schema == null && dto.Schema != 0) ||
                dataFile.Schema.SchemaId != dto.Schema)
            {
                throw new SchemaNotFoundException("DataFile is not associated with specified SchemaId");
            }
            if ((dataFile.SchemaRevision == null && dto.SchemaRevision != 0) ||
                (dataFile.SchemaRevision != null && dataFile.SchemaRevision.SchemaRevision_Id != dto.SchemaRevision))
            {
                throw new SchemaRevisionNotFoundException("DataFile is not associated with specified SchemaRevision");
            }

            UpdateDataFile(dto, dataFile);

            _datasetContext.SaveChanges();

        }

        
        public List<DatasetFile> GetDatasetFileList(string[] fileNameList)
        {
            List<DatasetFile> dbList = _datasetContext.DatasetFile.Where(w => fileNameList.Contains(w.OriginalFileName)).ToList();
            return dbList;
        }

        public List<DatasetFile> GetDatasetFileList(int[] datasetFileIdList)
        {
            List<DatasetFile> dbList = _datasetContext.DatasetFile.Where(w => datasetFileIdList.Contains(w.DatasetFileId)).ToList();
            return dbList;
        }

        public void UpdateDatasetFileObjectStatus(List<DatasetFile> files)
        {
            //UPDATE ObjectStatus
        }

        public void UploadDatasetFileToS3(UploadDatasetFileDto uploadDatasetFileDto)
        {
            DatasetFileConfig datasetFileConfig = _datasetContext.GetById<DatasetFileConfig>(uploadDatasetFileDto.ConfigId);

            if (datasetFileConfig != null)
            {
                DataFlow dataFlow = _datasetContext.DataFlow.FirstOrDefault(x => x.DatasetId == uploadDatasetFileDto.DatasetId && x.SchemaId == datasetFileConfig.Schema.SchemaId);
                DataFlowStep dropStep = dataFlow?.Steps.FirstOrDefault(x => x.DataAction_Type_Id == DataActionType.ProducerS3Drop);
                if (dropStep != null)
                {
                    _s3ServiceProvider.UploadDataFile(uploadDatasetFileDto.FileInputStream, dropStep.TriggerBucket, dropStep.TriggerKey + uploadDatasetFileDto.FileName);
                }
                else
                {
                    Logger.Info($"Data Flow for dataset: {uploadDatasetFileDto.DatasetId} and schema: {datasetFileConfig.Schema.SchemaId} not found while attempting to upload file to S3");
                }
            }
            else
            {
                Logger.Info($"Dataset File Config with Id: {uploadDatasetFileDto.ConfigId} not found while attempting to upload file to S3");
            }
        }

        #region PrivateMethods
        internal void UpdateDataFile(DatasetFileDto dto, DatasetFile dataFile)
        {
            dataFile.FileLocation = dto.FileLocation;
            dataFile.VersionId = dto.VersionId;
            dataFile.FileKey = dto.FileKey;
            dataFile.FileBucket = dto.FileBucket;
        }
        #endregion
    }
}
