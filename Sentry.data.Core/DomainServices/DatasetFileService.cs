using Newtonsoft.Json;
using Sentry.Common.Logging;
using Sentry.data.Core.Entities.DataProcessing;
using Hangfire;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Helpers.Paginate;
using System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using Sentry.data.Core.Interfaces;

namespace Sentry.data.Core
{

    public class DatasetFileService : IDatasetFileService
    {
        private readonly IDatasetContext _datasetContext;
        private readonly ISecurityService _securityService;
        private readonly IUserService _userService;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IS3ServiceProvider _s3ServiceProvider;
        private readonly IJobScheduler _jobScheduler;

        public DatasetFileService(IDatasetContext datasetContext, ISecurityService securityService, IUserService userService, IMessagePublisher messagePublisher, IS3ServiceProvider s3ServiceProvider, IJobScheduler jobScheduler)
        {
            _datasetContext = datasetContext;
            _securityService = securityService;
            _userService = userService;
            _messagePublisher = messagePublisher;
            _s3ServiceProvider = s3ServiceProvider;
            _jobScheduler = jobScheduler;
        }

        public PagedList<DatasetFileDto> GetAllDatasetFileDtoBySchema(int schemaId, PageParameters pageParameters)
        {
            DatasetFileConfig config = _datasetContext.DatasetFileConfigs.FirstOrDefault(w => w.Schema.SchemaId == schemaId); // gets the specific schema

            if (config == null) // the case where the schema was not found
            {
                throw new SchemaNotFoundException();
            }

            // security measures taken to make sure that the user has the permission to see the schema/dataset data
            UserSecurity us = _securityService.GetUserSecurity(config.ParentDataset, _userService.GetCurrentUser());
            if (!us.CanViewFullDataset)
            {
                throw new DatasetUnauthorizedAccessException(); // if the user does not have permission then this exception is thrown
            }

            IQueryable<DatasetFile> datasetFileQueryable = _datasetContext.DatasetFileStatusActive.Where(x => x.Schema == config.Schema);
            datasetFileQueryable = pageParameters.SortDesc ? datasetFileQueryable.OrderByDescending(o => o.DatasetFileId) : datasetFileQueryable.OrderBy(o => o.DatasetFileId); // ordering datasetfiles by ascending or descending
            PagedList<DatasetFile> files = PagedList<DatasetFile>.ToPagedList(datasetFileQueryable, pageParameters.PageNumber, pageParameters.PageSize); // passing pageNumber and pageSize to the ToPagedList method

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
                //log list of Ids by exception
                string msg = "Error marking DatasetFile rows as Deleted";
                Logger.Error(msg, ex);
                throw;
            }
        }

        public UserSecurity GetUserSecurityForDatasetFile(int datasetId)
        {
            Dataset ds = _datasetContext.Datasets.Where(x => x.DatasetId == datasetId && x.CanDisplay).FetchSecurityTree(_datasetContext).FirstOrDefault();
            return _securityService.GetUserSecurity(ds, _userService.GetCurrentUser());
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

        /*
         *  Schedules the hangfire delayed job for reprocessing
         *  Logic for the time delay will be placed in this method eventually
         */
        public bool ScheduleReprocessing(int stepId, List<int> datasetFileIds)
        {
            bool successfullySubmitted = true;

            try
            {
                int batchSize = 100;
                int counter = 1;
                List<int> batch = datasetFileIds.Take(batchSize).ToList();

                while (batch.Any())
                {
                    _jobScheduler.Schedule<DatasetFileService>((d) => d.ReprocessDatasetFile(stepId, batch), TimeSpan.FromSeconds(30 * counter)); // this is returning null
                    counter++;
                    batch = batch.Skip(batchSize*counter).Take(batchSize).ToList();
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                successfullySubmitted = false;
            }

            return successfullySubmitted;
        }

        #region PrivateMethods
        internal void UpdateDataFile(DatasetFileDto dto, DatasetFile dataFile)
        {
            dataFile.FileLocation = dto.FileLocation;
            dataFile.VersionId = dto.VersionId;
            dataFile.FileKey = dto.FileKey;
            dataFile.FileBucket = dto.FileBucket;
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

            if (dto.UserFileNameList != null)
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

        private void DeleteByDatasetFileList(int datasetId, int schemaId, List<DatasetFile> dbList)
        {
            DeleteS3(datasetId, schemaId, dbList);
            UpdateObjectStatus(dbList, Core.GlobalEnums.ObjectStatusEnum.Pending_Delete);
        }

        private void DeleteS3(int datasetId, int schemaId, List<DatasetFile> dbList)
        {
            //CONVERT LIST TO GENERIC ARRAY IN PREP FOR PublishDSCEvent and ERROR HANDLING
            string[] idList = dbList.Select(s => s.DatasetFileId.ToString()).ToArray();

            try
            {
                //CHUNK INTO 10 id's PER MESSAGE
                string[] buffer;
                for (int i = 0; i < idList.Length; i += 10)
                {
                    int chunk = (idList.Length - i < 10) ? idList.Length - i : 10;          //ENSURE LAST CHUNK ONLY HAS WHAT REMAINS
                    buffer = new string[chunk];
                    Array.Copy(idList, i, buffer, 0, chunk);

                    S3DeleteFilesModel model = CreateS3DeleteFilesModel(datasetId, schemaId, buffer);

                    //PUBLISH DSC DELETE EVENT
                    _messagePublisher.PublishDSCEvent(schemaId.ToString(), JsonConvert.SerializeObject(model));
                }
            }
            catch (System.Exception ex)
            {
                string errorMsg = "Error trying to call _messagePublisher.PublishDSCEvent: " +
                            JsonConvert.SerializeObject(CreateS3DeleteFilesModel(datasetId, schemaId, idList));

                Logger.Error(errorMsg, ex);
                throw;
            }
        }

        private S3DeleteFilesModel CreateS3DeleteFilesModel(int datasetId, int schemaId, string[] datasetFileIdList)
        {
            S3DeleteFilesModel model = new S3DeleteFilesModel()
            {
                DatasetID = datasetId,
                SchemaID = schemaId,
                RequestGUID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                DatasetFileIdList = datasetFileIdList
            };

            return model;
        }

        /* 
         * Implementation of reprocessing
         * @param int stepid
         * @param int[] datasetFileIds
        */
        private bool ReprocessDatasetFile(int stepId, List<int> datasetFileIds)
        {
            try
            {
                //List<DatasetFile> datasetFiles = _datasetContext.DatasetFileStatusActive.Where(w => w.DatasetFileId == datasetFileIds).ToList();
                DataFlowStep dataFlowStep = _datasetContext.DataFlowStep.Where(w => w.Id == stepId).FirstOrDefault();
                string triggerFileLocation, triggerFileContent;
                foreach (int id in datasetFileIds)
                {
                    DatasetFile datasetFile = _datasetContext.DatasetFileStatusActive.Where(w => w.DatasetFileId == id).FirstOrDefault();
                    triggerFileLocation = GetTriggerFileLocation(dataFlowStep, datasetFile);
                    if (triggerFileLocation != null)
                    {
                        triggerFileContent = GetSourceBucketAndSourceKey(datasetFile);
                        if (triggerFileContent != null)
                        {
                            _s3ServiceProvider.UploadDataFile(triggerFileLocation, triggerFileContent);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            } catch (Exception ex)
            {
                // the case when reprocessing fails
                Console.WriteLine(ex.Message);
                return false;
            }
            
            return true;
        }

        /*
         * Helper function for ReprocessingDatasetFile which gets the trigger file location
         */
        private string GetTriggerFileLocation(DataFlowStep dataFlowStep, DatasetFile datasetFile)
        {
            // trigger file location
            return dataFlowStep.TriggerKey + datasetFile.FlowExecutionGuid + "/" + datasetFile.OriginalFileName + ".trg";
        }

        /*
         * Helper function for ReprocessingDatasetFile wich gets the content of the trigger file
         */
        private string GetSourceBucketAndSourceKey(DatasetFile datasetFile)
        {

            List<string> splitString = datasetFile.FileKey.Split('/').ToList();
            splitString.RemoveAt(splitString.Count - 1);
            string newStr = String.Join("/", splitString) + "/";

            newStr = newStr.Replace("rawquery", "raw");

            string result = newStr + datasetFile.FlowExecutionGuid + "/" + datasetFile.OriginalFileName;            

            // creating the ndjson object for the trigger file content
            JObject jobject = new JObject();
            jobject.Add("SourceBucket", datasetFile.FileBucket);
            jobject.Add("SourceKey", result);
            return jobject.ToString(Formatting.None);
        }
        #endregion

    }

    
}
