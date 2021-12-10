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

        IEnumerable<DatasetFileDto> GetAllDatasetFilesBySchema(int schemaId, PageParameters pageParameters);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaId"></param>
        /// <returns></returns>
        IEnumerable<DatasetFileDto> GetAllDatasetFilesBySchema(int schemaId, Func<DatasetFile, bool> where);
    }
}
