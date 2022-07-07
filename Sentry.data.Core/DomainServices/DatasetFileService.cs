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

        public DatasetFileService(IDatasetContext datasetContext, ISecurityService securityService,
                                    IUserService userService)
        {
            _datasetContext = datasetContext;
            _securityService = securityService;
            _userService = userService;
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

            // try and catch block to see if the hangfire job is created without error
            // done on each datasetFileId
            try
            {
                foreach (int id in datasetFileIds)
                {
                    BackgroundJob.Schedule<DatasetFileService>((x) =>  x.ReprocessDatasetFiles(stepId, new int[] {id}), System.TimeSpan.FromDays(1)); 
                }
            } catch (System.Exception ex)
            {
                successfullySubmitted = false;
            }

            return successfullySubmitted;
        }

        /* Implementation of reprocessing
             * 
             * SourceKey
             *      start with FileKey from datasetfile
             *      1) remove file name
             *      2) Adjust root prefix to be raw
             *      3) Append flowexecutionguid from dataset file
             *      4) Add OriginalFileName from datasetfile
        */
        private bool ReprocessDatasetFiles(int stepId, int[] datasetFileIds)
        {

            // extractng the necessary data from stepId and datasetFileIds to get the location of trigger file
            string dataFlowStep_TriggerKey = _datasetContext.DataFlowStep.Where(w => w.Id == stepId).FirstOrDefault().TriggerKey;
            string datasetFile_FlowExecutionGuid = _datasetContext.DatasetFileStatusActive.Where(w => w.DatasetFileId == datasetFileIds[0]).FirstOrDefault().FlowExecutionGuid;
            string datasetFile_OriginalFileName = _datasetContext.DatasetFileStatusActive.Where(w => w.DatasetFileId == datasetFileIds[0]).FirstOrDefault().OriginalFileName;
            string datasetFile_FileBucket = _datasetContext.DatasetFileStatusActive.Where(w => w.DatasetFileId == datasetFileIds[0]).FirstOrDefault().FileBucket;

            // trigger file location
            string triggerFileLocation = dataFlowStep_TriggerKey + "/" + datasetFile_FlowExecutionGuid + "/" + datasetFile_OriginalFileName + ".trg";

            // gets the file key
            string filekey = _datasetContext.DatasetFileStatusActive.Where(w => w.DatasetFileId == datasetFileIds[0]).FirstOrDefault().FileKey;

            // 1) remove the file name
            String[] splitString = filekey.Split('/');
            Array.Resize(ref splitString, splitString.Length - 1);
            string newStr = String.Join("/", splitString) + "/";

            // 2) adjust root prefix to be raw
            newStr = newStr.Replace("rawquery", "raw");

            // 3) Append flowexecution guid from the dataset file
            string temp = newStr + datasetFile_FlowExecutionGuid + "/";

            // 4) Add OriginalFileName from datasetfile
            string result = temp + datasetFile_OriginalFileName;

            // creating the ndjson object for the trigger file content
            JObject jobject = new JObject();
            jobject.Add("SourceBucket", datasetFile_FileBucket);
            jobject.Add("SourceKey", result);
            string content = jobject.ToString();
            string singleLineContent = content.Replace("\r\n", " ");

            // Writing content into the trigger file at the trigger file location
            File.Create(triggerFileLocation);
            TextWriter tw = new StreamWriter(triggerFileLocation);
            tw.WriteLine(singleLineContent);
            tw.Close();

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
