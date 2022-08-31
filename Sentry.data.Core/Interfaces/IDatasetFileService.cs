﻿using Sentry.data.Core.Helpers.Paginate;
using System.Collections.Generic;


namespace Sentry.data.Core
{
    public interface IDatasetFileService
    {
        /// <summary>
        /// Get all data files associated with schema.  PageParameters included to 
        /// limit number of records returned.
        /// </summary>
        /// <param name="schemaId"></param>
        /// <param name="pageParameters"></param>
        /// <returns></returns>
        PagedList<DatasetFileDto> GetNonDeletedDatasetFileDtoBySchema(int schemaId, PageParameters pageParameters);

        /// <summary>
        /// Get all active data files associated with schema.  PageParameters included to 
        /// limit number of records returned.
        /// </summary>
        /// <param name="schemaId"></param>
        /// <param name="pageParameters"></param>
        /// <returns></returns>
        PagedList<DatasetFileDto> GetActiveDatasetFileDtoBySchema(int schemaId, PageParameters pageParameters);

        /// <summary>
        /// Get all data files associated with schema.  PageParameters included to 
        /// limit number of records returned.
        /// </summary>
        /// <param name="schemaId"></param>
        /// <param name="pageParameters"></param>
        /// <returns></returns>
        PagedList<DatasetFileDto> GetAllDatasetFileDtoBySchema(int schemaId, PageParameters pageParameters);

        /// <summary>
        /// Update data file record and save
        /// </summary>
        /// <param name="dto"></param>
        void UpdateAndSave(DatasetFileDto dto);
        string Delete(int datasetId, int schemaId, DeleteFilesParamDto dto);
        UserSecurity GetUserSecurityForDatasetFile(int datasetId);
        void UploadDatasetFileToS3(UploadDatasetFileDto uploadDatasetFileDto);

        void UpdateObjectStatus(List<DatasetFile> dbList, GlobalEnums.ObjectStatusEnum status);
        void UpdateObjectStatus(int[] idList, GlobalEnums.ObjectStatusEnum status);
        bool ScheduleReprocessing(int stepId, List<int> datasetFileIds);
    }
}
