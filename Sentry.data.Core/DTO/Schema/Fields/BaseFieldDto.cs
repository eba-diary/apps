using NJsonSchema;
using Sentry.Core;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Sentry.data.Core
{
    public abstract class BaseFieldDto
    {
        #region Constructors
        protected BaseFieldDto()
        {
            CreateDtm = DateTime.Now;
            LastUpdatedDtm = DateTime.Now;
            ChildFields = new List<BaseFieldDto>();
        }

        protected BaseFieldDto(BaseField field)
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
            DotNamePath = field.DotNamePath;
            StructurePosition = field.StructurePosition;
        }

        protected BaseFieldDto(KeyValuePair<string, JsonSchemaProperty> prop, int position, bool array)
        {
            FieldId = 0;
            FieldGuid = Guid.Empty;
            Name = prop.Key;
            CreateDtm = DateTime.Now;
            LastUpdatedDtm = DateTime.Now;
            IsArray = array;
            ChildFields = new List<BaseFieldDto>();
            Description = prop.Value.Description;
            OrdinalPosition = position;
        }

        protected BaseFieldDto(SchemaRow row)
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
            Length = int.TryParse(row.Length, out int x) ? x : GlobalConstants.Datatypes.Defaults.LENGTH_DEFAULT;
            DeleteInd = row.DeleteInd;
            DotNamePath = row.DotNamePath;
        }
        #endregion

        #region Properties
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
        public string DotNamePath { get; set; }
        public int Length { get; set; }
        public string StructurePosition { get; set; }
        #endregion

        #region Abstract
        public abstract string FieldType { get; }
        public abstract int Precision { get; set; }
        public abstract int Scale { get; set; }
        public abstract string SourceFormat { get; set; }
        public abstract bool Nullable { get; set; }
        public abstract int OrdinalPosition { get; set; }

        public abstract BaseField ToEntity(BaseField parentField, SchemaRevision parentRevision);
        public abstract bool CompareToEntity(BaseField field);
        public abstract void Clean(string extension);
        #endregion

        #region Methods
        public virtual ValidationResults Validate(string extension)
        {
            ValidationResults results = new ValidationResults();

            //Field name cannot be blank
            if (string.IsNullOrWhiteSpace(Name))
            {
                results.Add(OrdinalPosition.ToString(), $"Field name cannot be empty string");
            }
            else
            {
                //Field name must start with letter or underscore
                string startsWith = "^[A-Za-z_]";
                if (!Regex.IsMatch(Name, startsWith))
                {
                    results.Add(OrdinalPosition.ToString(), $"Field name ({Name}) must start with a letter or underscore");
                }

                //Field name can only contain letters, underscores, digits, and dollar signs
                string body = "^[A-Za-z0-9$_]+$";
                if (!Regex.IsMatch(Name, body))
                {
                    results.Add(OrdinalPosition.ToString(), $"Field name ({Name}) can only contain letters, underscores, digits (0-9), and dollar signs (\"$\")");
                }
            }

            if (extension == GlobalConstants.ExtensionNames.FIXEDWIDTH && Length == 0)
            {
                results.Add(OrdinalPosition.ToString(), $"({Name}) Length ({Length}) needs to be greater than zero for FIXEDWIDTH schema");
            }

            return results;
        }

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

        protected void DefaultNonFixedWidthLength(string extension)
        {
            if (extension != GlobalConstants.ExtensionNames.FIXEDWIDTH)
            {
                Length = GlobalConstants.Datatypes.Defaults.LENGTH_DEFAULT;
            }
        }
        #endregion
    }
}
