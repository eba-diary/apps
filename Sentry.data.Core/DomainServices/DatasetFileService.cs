using Hangfire;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Helpers.Paginate;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;

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

        /*
         *  Schedules the hangfire delayed job for reprocessing
         *  Logic for the time delay will be placed in this method eventually
         */
        public bool ScheduleReprocessing(int stepId, int[] datasetFileIds)
        {
            bool successfullySubmitted = true;

            try
            {
                foreach (int id in datasetFileIds)
                {
                    BackgroundJob.Schedule<DatasetFileService>((x) =>  x.ReprocessDatasetFile(stepId, id), System.TimeSpan.FromDays(1)); 
                }
            } catch (System.Exception ex)
            {
                successfullySubmitted = false;
            }

            return successfullySubmitted;
        }


        /* 
         * Implementation of reprocessing
         * @param int stepid
         * @param int[] datasetFileIds
        */
        private bool ReprocessDatasetFile(int stepId, int datasetFileId)
        {
            string triggerFileLocation = GetTriggerFileLocation(stepId, datasetFileId);

            string content = GetSourceBucketAndSourceKey(datasetFileId);

            return UploadTriggerFile(triggerFileLocation, content);
        }
        
        internal string GetTriggerFileLocation(int stepId, int datasetFileId)
        {
            DatasetFile datasetFile = _datasetContext.DatasetFileStatusActive.Where(w => w.DatasetFileId == datasetFileId).FirstOrDefault();

            // extractng the necessary data from stepId and datasetFileIds to get the location of trigger file
            string flowExecutionGuid = datasetFile.FlowExecutionGuid;
            string originalFileName = datasetFile.OriginalFileName;
            string TriggerKey = _datasetContext.DataFlowStep.Where(w => w.Id == stepId).FirstOrDefault().TriggerKey;
            // trigger file location
            string triggerFileLocation = TriggerKey + flowExecutionGuid + "/" + originalFileName + ".trg";

            return triggerFileLocation;
        }

        internal string GetSourceBucketAndSourceKey(int datasetFileId) 
        {
            DatasetFile datasetFile = _datasetContext.DatasetFileStatusActive.Where(w => w.DatasetFileId == datasetFileId).FirstOrDefault();

            string flowExecutionGuid = datasetFile.FlowExecutionGuid;
            string originalFileName = datasetFile.OriginalFileName;

            // gets the file key
            string filekey = datasetFile.FileKey;

            // gets the source bucket
            string sourceBucket = datasetFile.FileBucket;

            // 1) remove the file name
            String[] splitString = filekey.Split('/');
            Array.Resize(ref splitString, splitString.Length - 1);
            string newStr = String.Join("/", splitString) + "/";

            // 2) adjust root prefix to be raw
            newStr = newStr.Replace("rawquery", "raw");

            // 3) Append flowexecution guid from the dataset file
            string temp = newStr + flowExecutionGuid + "/";

            // 4) Add OriginalFileName from datasetfile
            string result = temp + originalFileName;

            // creating the ndjson object for the trigger file content
            JObject jobject = new JObject();
            jobject.Add("SourceBucket", sourceBucket);
            jobject.Add("SourceKey", result);
            string content = jobject.ToString();
            string singleLineContent = content.Replace("\r\n", " ");

            return singleLineContent;
        }

        internal bool UploadTriggerFile(string triggerFileLocation, string triggerFileContent)
        {
            _s3ServiceProvider.UploadDataFile(triggerFileLocation, triggerFileContent);
            return true;
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
