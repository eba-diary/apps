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

        public int CreateAndSaveSchema(SchemaDto schemaDto)
        {
            int newSchemaId = 0;
            try
            {
                FileSchemaDto dto = schemaDto as FileSchemaDto;
                if (dto != null)
                {
                    newSchemaId = CreateSchema(dto);
                }

                _datasetContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Error("schemaservice-createandsaveschema", ex);
                return 0;
            }

            return newSchemaId;            
        }

        public bool UpdateAndSaveSchema(SchemaDto schemaDto)
        {
            try
            {
                FileSchemaDto dto = schemaDto as FileSchemaDto;
                if (dto != null)
                {
                    FileSchema schema = _datasetContext.GetById<FileSchema>(dto.SchemaId);
                    UpdateAndSaveSchema(dto, schema);
                    _datasetContext.SaveChanges();
                    return true;
                }
                Logger.Info("schemaservice-updateandsaveschema typenotfound");
                return false;
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
            if (schema.Extension.Id != dto.FileExtensionId)
            {
                schema.Extension = _datasetContext.GetById<FileExtension>(dto.FileExtensionId);
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

        private int CreateSchema(FileSchemaDto dto)
        {
            FileSchema schema = new FileSchema()
            {
                Name = dto.Name,
                CreatedBy = _userService.GetCurrentUser().AssociateId,
                SchemaEntity_NME = dto.SchemaEntity_NME,
                Extension = _datasetContext.GetById<FileExtension>(dto.FileExtensionId),
                Delimiter = dto.Delimiter,
                HasHeader = dto.HasHeader,
                IsInSAS = dto.IsInSAS,
                SasLibrary = CommonExtensions.GenerateSASLibaryName(_datasetContext.GetById<Dataset>(dto.ParentDatasetId))
            };
            _datasetContext.Add(schema);
            return schema.SchemaId;
        }
    }
}
