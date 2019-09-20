using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core
{
    public static class SchemaExtensions
    {
        public static SchemaDto MapToSchemaDto(this Schema scm)
        {
            return new SchemaDto()
            {
                SchemaId = scm.SchemaId,
                Name = scm.Name,
                SchemaEntity_NME = scm.SchemaEntity_NME
            };
        }

        public static SchemaRevisionDto ToDto(this SchemaRevision revision)
        {
            return new SchemaRevisionDto()
            {
                RevisionId = revision.SchemaRevision_Id,
                RevisionNumber = revision.Revision_NBR,
                SchemaRevisionName = revision.SchemaRevision_Name,
                CreatedBy = revision.CreatedBy,
                CreatedDTM = revision.CreatedDTM,
                LastUpdatedDTM = revision.LastUpdatedDTM
            };
        }
    }
}
