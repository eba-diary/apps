using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Web.Models.ApiModels.Schema;

namespace Sentry.data.Web
{
    public static class SchemaExtensions
    {
        public static SchemaInfoModel ToModel(this Core.SchemaDto dto)
        {
            return new SchemaInfoModel()
            {
                SchemaId = dto.SchemaId,
                SchemaEntity_NME = dto.SchemaEntity_NME,
                Name = dto.Name
            };
        }

        public static SchemaRevisionModel ToModel(this Core.SchemaRevisionDto dto)
        {
            return new SchemaRevisionModel()
            {
                RevisionId = dto.RevisionId,
                RevisionNumber = dto.RevisionNumber,
                SchemaRevisionName = dto.SchemaRevisionName,
                CreatedBy = dto.CreatedBy,
                CreatedByName = dto.CreatedByName,
                CreatedDTM = dto.CreatedDTM,
                LastUpdatedDTM = dto.LastUpdatedDTM
            };
        }

        public static List<SchemaRevisionModel> ToModel(this List<Core.SchemaRevisionDto> dtoList)
        {
            List<SchemaRevisionModel> modelList = new List<SchemaRevisionModel>();
            foreach(Core.SchemaRevisionDto dto in dtoList)
            {
                modelList.Add(dto.ToModel());
            }
            return modelList;
        }

        public static SchemaRevisionDetailModel ToSchemaDetailModel(this Core.SchemaRevisionDto dto)
        {
            return new SchemaRevisionDetailModel()
            {
                Revision = dto.ToModel()
            };
        }
    }
}