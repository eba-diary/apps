using Sentry.data.Core.Exceptions;
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

        public DatasetFileService(IDatasetContext datasetContext, ISecurityService securityService,
                                    IUserService userService)
        {
            _datasetContext = datasetContext;
            _securityService = securityService;
            _userService = userService;
        }

        public IEnumerable<DatasetFileDto> GetAllDatasetFilesBySchema(int schemaId)
        {
            IEnumerable<DatasetFile> files = _datasetContext.DatasetFile.Where(x => x.Schema.SchemaId == schemaId).AsEnumerable();

            IEnumerable<DatasetFileDto> fileDtoList = files.ToDto();

            return fileDtoList;
        }

        public IEnumerable<DatasetFileDto> GetAllDatasetFilesBySchema(int schemaId, PageParameters pageParameters)
        {
            int pageNumber = pageParameters.PageNumber ?? default;
            int pageSize = pageParameters.PageSize ?? default;

            IEnumerable<DatasetFile> files = _datasetContext.DatasetFile
                                                .Where(x => x.Schema.SchemaId == schemaId)
                                                .OrderBy(o => o.DatasetFileId)
                                                .Skip((pageNumber - 1) * pageSize)
                                                .Take(pageSize)
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

        public void UpdateAndSave(DatasetFileDto dto)
        {
            IApplicationUser user = _userService.GetCurrentUser();
            if (!user.IsAdmin)
            {
                throw new DataFileUnauthorizedException();
            }

            Dataset ds = _datasetContext.GetById<Dataset>(dto.Dataset);
            if (ds == null)
            {
                throw new DatasetNotFoundException();
            }

            DatasetFile dataFile = _datasetContext.GetById<DatasetFile>(dto.DatasetFileId);

            UpdateDataFile(dto, dataFile);

            _datasetContext.SaveChanges();

        }

        #region PrivateMethods
        internal void UpdateDataFile(DatasetFileDto dto, DatasetFile dataFile)
        {
            dataFile.FileLocation = dto.FileLocation;
        }
        #endregion
    }
}
