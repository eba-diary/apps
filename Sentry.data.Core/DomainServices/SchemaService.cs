using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sentry.Common.Logging;

namespace Sentry.data.Core
{
    public class SchemaService : ISchemaService
    {
        public IDatasetContext _datasetContext;
        public IUserService _userService;

        public SchemaService(IDatasetContext dsContext, IUserService userService)
        {
            _datasetContext = dsContext;
            _userService = userService;
        }

        public int CreateAndSaveSchema(FileSchemaDto schemaDto)
        {
            int newSchemaId = 0;
            try
            {
                newSchemaId = CreateSchema(schemaDto);

                _datasetContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("schemaservice-createandsaveschema", ex);
                return 0;
            }

            return newSchemaId;            
        }

        public bool UpdateAndSaveSchema(FileSchemaDto schemaDto)
        {
            try
            {
                FileSchema schema = _datasetContext.GetById<FileSchema>(schemaDto.SchemaId);
                UpdateAndSaveSchema(schemaDto, schema);
                _datasetContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("schemaservice-updateandsaveschema", ex);
                return false;
            }
        }

        private void UpdateAndSaveSchema(FileSchemaDto dto, FileSchema schema)
        {
            bool chgDetected = false;
            if (schema.Name != dto.Name)
            {
                schema.Name = dto.Name;
                chgDetected = true;
            };
            if (schema.Delimiter != dto.Delimiter)
            {
                schema.Description = dto.Description;
                chgDetected = true;
            }
            if (schema.CreateCurrentView != dto.CreateCurrentView)
            {
                schema.CreateCurrentView = dto.CreateCurrentView;
                chgDetected = true;
            }
            if (schema.Description != dto.Description)
            {
                schema.Description = dto.Description;
                chgDetected = true;
            }
            if (schema.Extension.Id != dto.FileExtensionId)
            {
                schema.Extension = _datasetContext.GetById<FileExtension>(dto.FileExtensionId);
                chgDetected = true;
            }
            if (schema.HasHeader != dto.HasHeader)
            {
                schema.HasHeader = dto.HasHeader;
                chgDetected = true;
            }
            if (schema.IsInSAS != dto.IsInSAS)
            {
                schema.IsInSAS = dto.IsInSAS;
                chgDetected = true;
            }

            if (chgDetected)
            {
                schema.LastUpdatedDTM = DateTime.Now;
                schema.UpdatedBy = _userService.GetCurrentUser().AssociateId;
            } 
            
        }

        public FileSchemaDto GetFileSchemaDto(int id)
        {
            FileSchema scm = _datasetContext.FileSchema.Where(w => w.SchemaId == id).FirstOrDefault();
            return scm.MapToDto();
        }

        public SchemaRevisionDto GetSchemaRevisionDto(int id)
        {
            SchemaRevision revision = _datasetContext.GetById<SchemaRevision>(id);
            SchemaRevisionDto dto = revision.ToDto();
            return dto;
        }

        public List<SchemaRevisionDto> GetSchemaRevisionDtoBySchema(int id)
        {
            List<SchemaRevisionDto> dtoList = new List<SchemaRevisionDto>();
            foreach (SchemaRevision revision in _datasetContext.SchemaRevision.Where(w => w.ParentSchema.SchemaId == id).ToList())
            {
                dtoList.Add(revision.ToDto());
            }
            return dtoList;
        }

        public List<BaseFieldDto> GetBaseFieldDtoBySchemaRevision(int revisionId)
        {
            SchemaRevision revision = _datasetContext.SchemaRevision.FirstOrDefault(w => w.SchemaRevision_Id == revisionId);

            return revision.Fields.Where(w => w.ParentField == null).ToList().ToDto();
        }

        public SchemaRevisionDto GetLatestSchemaRevisionDtoBySchema(int schemaId)
        {
            SchemaRevision revision = _datasetContext.SchemaRevision.Where(w => w.ParentSchema.SchemaId == schemaId).OrderByDescending(o => o.Revision_NBR).Take(1).FirstOrDefault();

            return revision.ToDto();
        }

        public List<DatasetFile> GetDatasetFilesBySchema(int schemaId)
        {
            List<DatasetFile> fileList = _datasetContext.DatasetFile.Where(w => w.Schema.SchemaId == schemaId).ToList();
            return fileList;
        }

        public DatasetFile GetLatestDatasetFileBySchema(int schemaId)
        {
            DatasetFile file = _datasetContext.DatasetFile.OrderBy(x => x.CreateDTM).FirstOrDefault(w => w.Schema.SchemaId == schemaId);
            return file;
        }

        private int CreateSchema(FileSchemaDto dto)
        {
            string storageCode = _datasetContext.GetNextStorageCDE().ToString();
            Dataset parentDataset = _datasetContext.GetById<Dataset>(dto.ParentDatasetId);
            FileSchema schema = new FileSchema()
            {
                Name = dto.Name,
                CreatedBy = _userService.GetCurrentUser().AssociateId,
                SchemaEntity_NME = dto.SchemaEntity_NME,
                Extension = (dto.FileExtensionId != 0) ? _datasetContext.GetById<FileExtension>(dto.FileExtensionId) : (dto.FileExtenstionName != null) ? _datasetContext.FileExtensions.Where(w => w.Name == dto.FileExtenstionName).FirstOrDefault() : null,
                Delimiter = dto.Delimiter,
                HasHeader = dto.HasHeader,
                IsInSAS = dto.IsInSAS,
                SasLibrary = CommonExtensions.GenerateSASLibaryName(_datasetContext.GetById<Dataset>(dto.ParentDatasetId)),
                Description = dto.Description,
                StorageCode = storageCode,
                HiveDatabase = "dsc_" + parentDataset.DatasetCategories.First().Name.ToLower(),
                HiveTable = parentDataset.DatasetName.Replace(" ", "").Replace("_", "").ToUpper() + "_" + dto.Name.Replace(" ", "").ToUpper(),
                HiveTableStatus = HiveTableStatusEnum.NameReserved.ToString(),
                HiveLocation = Configuration.Config.GetHostSetting("AWSRootBucket") + "/" + GlobalConstants.ConvertedFileStoragePrefix.PARQUET_STORAGE_PREFIX + "/" + Configuration.Config.GetHostSetting("S3DataPrefix") + storageCode,
                CreatedDTM = DateTime.Now,
                LastUpdatedDTM = DateTime.Now,
                DeleteIssueDTM = DateTime.MaxValue
            };
            _datasetContext.Add(schema);
            return schema.SchemaId;
        }
    }
}
