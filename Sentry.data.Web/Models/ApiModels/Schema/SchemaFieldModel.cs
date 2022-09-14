using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.ApiModels.Schema
{
    public class SchemaFieldModel
    {
        public Guid FieldGuid { get; set; }
        public string Name { get; set; }
        public string CreateDTM { get; set; }
        public string LastUpdatedDTM { get; set; }
        public string FieldType { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public List<SchemaFieldModel> Fields { get; set; }
        public string SourceFormat { get; set; }
        public bool IsArray { get; set; }
        public bool Nullable { get; set; }
        public int OrdinalPosition { get; set; }
        public string Description { get; set; }
        public int Length { get; set; }
        public string DotNamePath { get; set; }
        public string StructurePosition { get; set; }
    }
}