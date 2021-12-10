using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public class DatasetFileService : IDatasetFileService
    {
        private readonly IDatasetContext _datasetContext;

        public DatasetFileService(IDatasetContext datasetContext)
        {
            _datasetContext = datasetContext;
        }

        public IEnumerable<DatasetFileDto> GetAllDatasetFilesBySchema(int schemaId)
        {
            IEnumerable<DatasetFile> files = _datasetContext.DatasetFile.Where(x => x.Schema.SchemaId == schemaId).AsEnumerable();

            IEnumerable<DatasetFileDto> fileDtoList = files.ToDto();

            return fileDtoList;
        }

        public IEnumerable<DatasetFileDto> GetAllDatasetFilesBySchema(int schemaId, PageParameters pageParameters)
        {
            IEnumerable<DatasetFile> files = _datasetContext.DatasetFile
                                                .Where(x => x.Schema.SchemaId == schemaId)
                                                .OrderBy(o => o.DatasetFileId)
                                                .Skip((pageParameters.PageNumber - 1) * pageParameters.PageSize)
                                                .Take(pageParameters.PageSize)
                                                .AsEnumerable();

            IEnumerable<DatasetFileDto> fileDtoList = files.ToDto();

            return fileDtoList;
        }

        public IEnumerable<DatasetFileDto> GetAllDatasetFilesBySchema(int schemaId, Func<DatasetFile, bool> where)
        {
            IEnumerable<DatasetFile> files = _datasetContext.DatasetFile.Where(x => x.Schema.SchemaId == schemaId).Where(where).AsEnumerable();

            IEnumerable<DatasetFileDto> fileDtoList = files.ToDto();

            return fileDtoList;
        }
    }
}
