using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public class SchemaRevision
    {
        public SchemaRevision()
        {
            CreatedDTM = DateTime.Now;
            LastUpdatedDTM = DateTime.Now;
            Fields = new List<BaseField>();
        }
        public virtual int SchemaRevision_Id { get; set; }
        public virtual int Revision_NBR { get; set; }
        public virtual FileSchema ParentSchema { get; set; }
        public virtual string SchemaRevision_Name { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual DateTime CreatedDTM { get; set; }
        public virtual DateTime LastUpdatedDTM { get; set; }
        public virtual int SchemaStruct_Id { get; set; }
        public virtual IList<BaseField> Fields { get; set; }
        public virtual string JsonSchemaObject { get; set; }

        public virtual JObject ToJsonStructure()
        {
            JObject structure = new JObject()
            {
                { "$schema", "http://json-schema.org/draft-04/schema#" },
                { "title", SchemaRevision_Name },
                { "type", "object" }
            };

            structure.AddJsonStructureProperties(Fields.Where(x => x.ParentField == null));

            return structure;
        }

        public virtual List<BaseFieldDto> ToFieldStructure()
        {
            List<BaseFieldDto> fieldDtoList = new List<BaseFieldDto>();

            foreach (var field in Fields)
            {
                fieldDtoList.Add(field.ToDto());
            }

            return fieldDtoList;
        }
    }
}
