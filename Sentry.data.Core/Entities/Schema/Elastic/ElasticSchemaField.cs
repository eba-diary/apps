using Nest;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Core.Entities.Schema.Elastic
{
    public class ElasticSchemaField : SearchHighlightable
    {
        public ElasticSchemaField()
        {

        }

        public ElasticSchemaField(BaseField field, int schemaId, int datasetId)
        {
            FieldGuid = field.FieldGuid.ToString();
            Name = field.Name;
            FieldType = field.FieldType.ToString();
            IsArray = field.IsArray;
            Nullable = field.NullableIndicator;
            OrdinalPosition = field.OrdinalPosition;
            Description = field.Description;
            Length = field.FieldLength;
            DotNamePath = field.DotNamePath;
            SchemaId = schemaId;
            DatasetId = datasetId;
        }


        [PropertyName("FieldGuid")]
        string FieldGuid { get; set; }
        [PropertyName("Name")]
        [GlobalSearchField(SearchDisplayNames.SchemaField.COLUMNNAME)]
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
        [PropertyName("FlattenedDescription")]
        public string FlattenedDescription => !string.IsNullOrEmpty(Description) ? Description.ToLower() : "";
        [PropertyName("Length")]
        int Length { get; set; }
        [PropertyName("DotNamePath")]
        public string DotNamePath { get; set; }
        [PropertyName("DatasetId")]
        public int DatasetId { get; set; }
        [PropertyName("SchemaId")]
        public int SchemaId { get; set; }

        public override bool Equals(object obj)
        {
            ElasticSchemaField field = obj as ElasticSchemaField;
            return field != null && FieldGuid.Equals(field.FieldGuid); 
        }

        public override int GetHashCode()
        {
            return this.FieldGuid.GetHashCode();
        }
    }
}
