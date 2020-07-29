using NJsonSchema;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public abstract class BaseFieldDto
    {
        public BaseFieldDto(BaseField field)
        {
            FieldId = field.FieldId;
            FieldGuid = field.FieldGuid;
            Name = field.Name;
            CreateDtm = field.CreateDTM;
            LastUpdatedDtm = field.LastUpdateDTM;
            IsArray = field.IsArray;
            Description = field.Description;
            ChildFields = new List<BaseFieldDto>();
            Length = field.FieldLength;
            OrdinalPosition = field.OrdinalPosition;
        }

        public BaseFieldDto(KeyValuePair<string, JsonSchemaProperty> prop, bool array)
        {
            FieldId = 0;
            FieldGuid = Guid.Empty;
            Name = prop.Key;
            CreateDtm = DateTime.Now;
            LastUpdatedDtm = DateTime.Now;
            IsArray = array;
            ChildFields = new List<BaseFieldDto>();
            Description = prop.Value.Description;
        }

        public BaseFieldDto(SchemaRow row)
        {
            FieldId = row.DataObjectField_ID;
            FieldGuid = row.FieldGuid;
            Name = row.Name;
            CreateDtm = DateTime.Now;
            LastUpdatedDtm = DateTime.Now;
            IsArray = row.IsArray;
            Description = row.Description;
            ChildFields = new List<BaseFieldDto>();
            OrdinalPosition = row.Position;
            Length = (Int32.TryParse(row.Length, out int x) ? x : 0);
        }

        public int FieldId { get; set; }
        public Guid FieldGuid { get; set; }
        public string Name { get; set; }
        public DateTime CreateDtm { get; set; }
        public DateTime LastUpdatedDtm { get; set; }
        public string Description { get; set; }
        public bool DeleteInd { get; set; }
        public List<BaseFieldDto> ChildFields { get; set; }
        public bool IsArray { get; set; }
        public bool HasChildren { get; set; }


        public abstract string FieldType { get; }
        public abstract int Precision { get; set; }
        public abstract int Scale { get; set; }
        public abstract string SourceFormat { get; set; }
        public abstract bool Nullable { get; set; }
        public abstract int OrdinalPosition { get; set; }
        public abstract int Length { get; set; }
        public abstract BaseField ToEntity(BaseField parentField, SchemaRevision parentRevision);
        public abstract bool CompareToEntity(BaseField field);

        protected void ToEntity(BaseField field, BaseField parentField, SchemaRevision parentRevision)
        {
            field.FieldId = FieldId;
            field.Name = Name;
            field.CreateDTM = CreateDtm;
            field.LastUpdateDTM = LastUpdatedDtm;
            field.Description = Description;
            field.IsArray = IsArray;
            field.ParentField = parentField;
            field.ParentSchemaRevision = parentRevision;
            field.FieldLength = Length;
            field.OrdinalPosition = OrdinalPosition;
            if (FieldGuid == Guid.Empty)
            {
                Guid g = Guid.NewGuid();
                field.FieldGuid = g;
            }
            else
            {
                field.FieldGuid = FieldGuid;
            }
        }
    }
}
