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


       

        public string Delete(int datasetId, int schemaId, DeleteFilesParamDto dto)
        {
            string error = ValidateDeleteDataFilesParams(datasetId, schemaId, dto);
            if(error != null)
            {
                return error;
            }

            List<DatasetFile> dbList = GetDatasetFileList(datasetId, schemaId, dto);
            if (dbList != null && dbList.Count == 0)    //VALIDATE: ANYTHING TO DELETE
            {
                return "Nothing found to delete.";
            }

            DeleteByDatasetFileList(datasetId, schemaId, dbList);
            return error;
        }


        private string ValidateDeleteDataFilesParams(int datasetId, int schemaId, DeleteFilesParamDto dto)
        {
            //VALIDATIONS:  datasetId/schemaId
            if (datasetId < 1 || schemaId < 1)
            {
                return nameof(datasetId) + " AND " + nameof(schemaId) + " must be greater than 0";
            }

            //VALIDATIONS:  deleteFilesModel NOT NULL
            if (dto == null)
            {
                return " DeleteFilesParam format is wrong, please see definition for format.";
            }

            //VALIDATIONS:    CANNOT PASS BOTH LISTS
            bool userFileNameListPassed = (dto.UserFileNameList != null && dto.UserFileNameList.Length > 0);
            bool userIdListPassed = (dto.UserFileIdList != null && dto.UserFileIdList.Length > 0);
            if (userFileNameListPassed && userIdListPassed)
            {
                return "Cannot pass " + nameof(dto.UserFileNameList) + " AND " + nameof(dto.UserFileIdList) + " at the same time.  Please include only " + nameof(dto.UserFileNameList) + " OR " + nameof(dto.UserFileIdList);
            }

            //VALIDATIONS: MUST PASS ATLEAST ONE LIST
            if ((dto.UserFileNameList == null && dto.UserFileIdList == null))
            {
                return "Must pass either " + nameof(dto.UserFileNameList) + " OR " + nameof(dto.UserFileIdList);
            }

            return null;
        }

        private List<DatasetFile> GetDatasetFileList(int datasetId, int schemaId, DeleteFilesParamDto dto)
        {
            List<DatasetFile> dbList = new List<DatasetFile>();
            const int maxChunk = 2000;      //NOTE:  CHUNK SIZE SET HERE, LINQ TURNS "Contains" below into SQL SERVER PARAMS for each element in buffer, limit is 2100 params in SQL SERVER.  The loops here keep us under that threshold
            
            if(dto.UserFileNameList != null)
            {
                string[] buffer;
                for (int i = 0; i < dto.UserFileNameList.Length; i += maxChunk)
                {
                    int chunk = (dto.UserFileNameList.Length - i < maxChunk) ? dto.UserFileNameList.Length - i : maxChunk;          //ENSURE LAST CHUNK ONLY HAS WHAT REMAINS
                    buffer = new string[chunk];                                                                                     //CREATE BUFFER BASED ON CHUNK SIZE
                    Array.Copy(dto.UserFileNameList, i, buffer, 0, chunk);                                                          //COPY FROM list INTO buffer
                    dbList.AddRange(_datasetContext.DatasetFileStatusAll.Where(w => w.Dataset.DatasetId == datasetId                //USE LINQ to grab anything from DB that matches whats in buffer
                                                                           && w.Schema.SchemaId == schemaId
                                                                           && buffer.Contains(w.OriginalFileName)
                                                                           ).ToList());
                }
            }
            else
            {
                int[] buffer;
                for (int i = 0; i < dto.UserFileIdList.Length; i += maxChunk)
                {
                    int chunk = (dto.UserFileIdList.Length - i < maxChunk) ? dto.UserFileIdList.Length - i : maxChunk;          //ENSURE LAST CHUNK ONLY HAS WHAT REMAINS
                    buffer = new int[chunk];
                    Array.Copy(dto.UserFileIdList, i, buffer, 0, chunk);
                    dbList.AddRange(_datasetContext.DatasetFileStatusAll.Where(w => w.Dataset.DatasetId == datasetId
                                                                           && w.Schema.SchemaId == schemaId
                                                                           && buffer.Contains(w.DatasetFileId)
                                                                           ).ToList());
                }
            }

            return dbList;
        }

        private void DeleteByDatasetFileList(int datasetId, int schemaId,List<DatasetFile> dbList)
        {
            DeleteS3(datasetId,schemaId,dbList);
            UpdateObjectStatus(dbList, Core.GlobalEnums.ObjectStatusEnum.Pending_Delete);
        }

       
        private void DeleteS3(int datasetId, int schemaId, List<DatasetFile> dbList)
        {
            //CONVERT LIST TO GENERIC ARRAY IN PREP FOR PublishDSCEvent and ERROR HANDLING
            int[] idList = dbList.Select(s => s.DatasetFileId).ToArray();

            try
            {
                //CHUNK INTO 10 id's PER MESSAGE
                int[] buffer;
                for (int i = 0; i < idList.Length; i += 10)
                {
                    int chunk = (idList.Length - i < 10) ? idList.Length - i : 10;          //ENSURE LAST CHUNK ONLY HAS WHAT REMAINS
                    buffer = new int[chunk];
                    Array.Copy(idList, i, buffer, 0, chunk );

                    DeleteFilesRequestModel model = CreateDeleteFilesRequestModel(datasetId,schemaId,buffer);
                    
                    //PUBLISH DSC DELETE EVENT
                    _messagePublisher.PublishDSCEvent(schemaId.ToString(), JsonConvert.SerializeObject(model));
                }
            }
            catch (System.Exception ex)
            {
                string errorMsg = "Error trying to call _messagePublisher.PublishDSCEvent: " + 
                            JsonConvert.SerializeObject(CreateDeleteFilesRequestModel(datasetId, schemaId, idList));
                
                Logger.Error(errorMsg, ex);
                throw;
            }
        }


        public void UpdateObjectStatus(List<DatasetFile> dbList, GlobalEnums.ObjectStatusEnum status)
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

        public void UpdateObjectStatus(int datasetFileId, GlobalEnums.ObjectStatusEnum status)
        {
            try
            {
                DatasetFile datasetFile = _datasetContext.DatasetFileStatusAll.FirstOrDefault(w => w.DatasetFileId == datasetFileId);
                if (datasetFile != null)
                {
                    datasetFile.ObjectStatus = status;
                    _datasetContext.SaveChanges();
                }
            }
            catch (System.Exception ex)
            {
                string msg = "Error marking DatasetFile row as " + status.GetDescription();
                Logger.Error(msg, ex);
                throw;
            }
        }




        private DeleteFilesRequestModel CreateDeleteFilesRequestModel(int datasetId, int schemaId, int[] datasetFileIdList)
        {
            DeleteFilesRequestModel model = new DeleteFilesRequestModel()
            {
                DatasetID = datasetId,
                SchemaID = schemaId,
                RequestGUID = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"),
                DatasetFileIdList = datasetFileIdList
            };

            return model;
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
