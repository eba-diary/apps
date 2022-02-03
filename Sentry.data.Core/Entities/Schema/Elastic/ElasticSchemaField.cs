using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Core.Entities.Schema.Elastic
{
    public class ElasticSchemaField
    {
        [PropertyName("FieldGuid")]
        string FieldGuid { get; set; }
        [PropertyName("Name")]
        public string Name { get; set; }
        [PropertyName("CreateDTM")]
        string CreateDTM { get; set; }
        [PropertyName("LastUpdatedDTM")]
        string LastUpdatedDTM { get; set; }
        [PropertyName("FieldType")]
        string FieldType { get; set; }
        [PropertyName("Precision")]
        int Precision { get; set; }
        [PropertyName("Scale")]
        int Scale { get; set; }
        [PropertyName("SourceFormat")]
        string SourceFormat { get; set; }
        [PropertyName("IsArray")]
        bool IsArray { get; set; }
        [PropertyName("Nullable")]
        bool Nullable { get; set; }
        [PropertyName("OrdinalPosition")]
        int OrdinalPosition { get; set; }
        [PropertyName("Description")]
        public string Description { get; set; }
        [PropertyName("Length")]
        int Length { get; set; }
        [PropertyName("DotNamePath")]
        public string DotNamePath { get; set; }
        [PropertyName("DatasetId")]
        int DatasetId { get; set; }
        [PropertyName("SchemaId")]
        int SchemaId { get; set; }
    }
}

