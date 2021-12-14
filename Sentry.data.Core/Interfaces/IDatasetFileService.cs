using Sentry.data.Core.Helpers;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public interface IDatasetFileService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaId"></param>
        /// <returns></returns>
        IEnumerable<DatasetFileDto> GetAllDatasetFilesBySchema(int schemaId);

        /// <summary>
        /// Get all data files associated with schema.  PageParameters included to 
        /// limit number of records returned.
        /// </summary>
        /// <param name="schemaId"></param>
        /// <param name="pageParameters"></param>
        /// <returns></returns>
        PagedList<DatasetFileDto> GetAllDatasetFileDtoBySchema(int schemaId, PageParameters pageParameters);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaId"></param>
        /// <returns></returns>
        IEnumerable<DatasetFileDto> GetAllDatasetFilesBySchema(int schemaId, Func<DatasetFile, bool> where);

        /// <summary>
        /// Update data file record and save
        /// </summary>
        /// <param name="dto"></param>
        void UpdateAndSave(DatasetFileDto dto);
    }
}
