using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Helpers.Paginate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public class DatasetFileService : IDatasetFileService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly ISecurityService _securityService;
        private readonly IUserService _userService;
        private readonly IMessagePublisher _messagePublisher;

        public DatasetFileService(IDatasetContext datasetContext, ISecurityService securityService,
                                    IUserService userService, IMessagePublisher messagePublisher)
        {
            _datasetContext = datasetContext;
            _securityService = securityService;
            _userService = userService;
            _messagePublisher = messagePublisher;
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

        
        public List<DatasetFile> GetDatasetFileList(int datasetId, int schemaId, string[] fileNameList)
        {
            List<DatasetFile> dbList = _datasetContext.DatasetFile.Where(w =>   w.Dataset.DatasetId == datasetId 
                                                                            &&  w.Schema.SchemaId == schemaId 
                                                                            &&  fileNameList.Contains(w.OriginalFileName)).ToList();
            return dbList;
        }

        public List<DatasetFile> GetDatasetFileList(int datasetId, int schemaId, int[] datasetFileIdList)
        {
            List<DatasetFile> dbList = _datasetContext.DatasetFile.Where(w =>   w.Dataset.DatasetId == datasetId
                                                                            &&  w.Schema.SchemaId == schemaId
                                                                            &&  datasetFileIdList.Contains(w.DatasetFileId)).ToList();
            return dbList;
        }


        public void UpdateMetadata(List<DatasetFile> dbList, GlobalEnums.ObjectStatusEnum status)
        {
            try
            {
                //UPDATE OBJECTSTATUS
                dbList.ForEach(f => f.ObjectStatus = status);
                _datasetContext.SaveChanges();
            }
            catch (System.Exception ex)
            {
                string msg = "Error marking DatasetFile rows as Deleted";
                Logger.Error(msg, ex);
                throw;
            }
        }

        public void DeleteS3(int datasetId, int schemaId, List<DatasetFile> dbList)
        {
            try
            {
                //CONVERT LIST TO GENERIC ARRAY IN PREP FOR PublishDSCEvent
                string[] idList = dbList.Select(s => s.DatasetFileId.ToString()).ToArray();

                //CHUNK INTO 10 id's PER MESSAGE
                string[] buffer;
                for (int i = 0; i < idList.Length; i += 10)
                {
                    int chunk = (idList.Length - i < 10) ? idList.Length - i : 10;          //ENSURE LAST CHUNK ONLY HAS WHAT REMAINS
                    buffer = new string[chunk];
                    Array.Copy(idList, i, buffer, 0, chunk );

                    S3DeleteFilesModel model = new S3DeleteFilesModel()
                    {
                        DatasetID = datasetId,
                        SchemaID = schemaId,
                        RequestGUID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        DatasetFileIdList = buffer
                    };

                    //PUBLISH DSC DELETE EVENT
                    _messagePublisher.PublishDSCEvent(schemaId.ToString(), JsonConvert.SerializeObject(model));
                }
            }
            catch (System.Exception ex)
            {
                string msg = "Error trying to call _messagePublisher.PublishDSCEvent";
                Logger.Error(msg, ex);
                throw;
            }
        }



        public UserSecurity GetUserSecurityForDatasetFile(int datasetId)
        {
            Dataset ds = _datasetContext.Datasets.Where(x => x.DatasetId == datasetId && x.CanDisplay).FetchSecurityTree(_datasetContext).FirstOrDefault();

            return _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());
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
