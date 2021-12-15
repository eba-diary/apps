using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Helpers;
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

        public IEnumerable<DatasetFileDto> GetAllDatasetFileDtoBySchema(int schemaId)
        {
            IEnumerable<DatasetFile> files = _datasetContext.DatasetFile.Where(x => x.Schema.SchemaId == schemaId).AsEnumerable();

            UserSecurity us = _securityService.GetUserSecurity(files.First().Dataset, _userService.GetCurrentUser());
            if (!us.CanViewFullDataset)
            {
                throw new DatasetUnauthorizedAccessException();
            }

            IEnumerable<DatasetFileDto> fileDtoList = files.ToDto();

            return fileDtoList;
        }

        public PagedList<DatasetFileDto> GetAllDatasetFileDtoBySchema(int schemaId, PageParameters pageParameters)
        {
            PagedList<DatasetFile> files = PagedList<DatasetFile>.ToPagedList(_datasetContext.DatasetFile
                                                .Where(x => x.Schema.SchemaId == schemaId)
                                                .OrderBy(o => o.DatasetFileId),
                                                pageParameters.PageNumber, pageParameters.PageSize);

            UserSecurity us = _securityService.GetUserSecurity(files.First().Dataset, _userService.GetCurrentUser());
            if (!us.CanViewFullDataset)
            {
                throw new DatasetUnauthorizedAccessException();
            }

            return new PagedList<DatasetFileDto>(files.ToDto().ToList(), files.TotalCount, files.CurrentPage, files.PageSize);
        }

        public IEnumerable<DatasetFileDto> GetAllDatasetFileDtoBySchema(int schemaId, Func<DatasetFile, bool> where)
        {
            IEnumerable<DatasetFile> files = _datasetContext.DatasetFile.Where(x => x.Schema.SchemaId == schemaId).Where(where).AsEnumerable();

            UserSecurity us = _securityService.GetUserSecurity(files.First().Dataset, _userService.GetCurrentUser());
            if (!us.CanViewFullDataset)
            {
                throw new DatasetUnauthorizedAccessException();
            }

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
                dataFile.SchemaRevision.SchemaRevision_Id != dto.SchemaRevision)
            {
                throw new SchemaRevisionNotFoundException("DataFile is not associated with specified SchemaRevision");
            }

            UpdateDataFile(dto, dataFile);

            _datasetContext.SaveChanges();

        }

        #region PrivateMethods
        internal void UpdateDataFile(DatasetFileDto dto, DatasetFile dataFile)
        {
            dataFile.FileLocation = dto.FileLocation;
            dataFile.VersionId = dto.VersionId;
        }
        #endregion
    }
}
