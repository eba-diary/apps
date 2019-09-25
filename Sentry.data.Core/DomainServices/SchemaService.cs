using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.DomainServices
{
    public class SchemaService : ISchemaService
    {
        public IDatasetContext _datasetContext;

        public SchemaService(IDatasetContext dsContext)
        {
            _datasetContext = dsContext;
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

            return revision.Fields.ToList().ToDto();
        }
    }
}
