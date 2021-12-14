using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    [Serializable]
    public abstract class BaseField
    {
        private BaseField _parentField;
        private IList<BaseField> _childFields = new List<BaseField>();


        public virtual int FieldId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual BaseField ParentField
        {
            get
            {
                return _parentField;
            }
            set
            {
                //If the current parent is set, remove the child (me) from it
                if (_parentField != null)
                {
                    _parentField._childFields.Remove(this);
                }
                _parentField = value;
                if (_parentField != null)
                {
                    _parentField._childFields.Add(this);
                }

            }
        }
        public virtual bool IsArray { get; set; }
        public virtual int OrdinalPosition { get; set; }
        public virtual int StartPosition { get; set; }
        public virtual int EndPosition { get; set; }
        public virtual DateTime CreateDTM { get; set; }
        public virtual DateTime LastUpdateDTM { get; set; }
        public virtual bool NullableIndicator { get; set; }
        public virtual Guid FieldGuid { get; set; }
        public virtual IList<BaseField> ChildFields
        {
            get
            {
                return _childFields;
            }
        }
        public virtual SchemaRevision ParentSchemaRevision { get; set; }
        public virtual int FieldLength { get; set; }

        //abstract properties
        public abstract SchemaDatatypes FieldType { get; set; }

        public virtual JObject ToJsonPropertyDefinition()
        {
            JObject definition = new JObject();

            if (IsArray)
            {
                definition.Add("type", "array");
                definition.Add("items", new JArray() { GetJsonTypeDefinition() });
            }
            else
            {
                definition = GetJsonTypeDefinition();
            }

            //determine how to handle Child Fields
            if (ChildFields?.Any() == true)
            {
                //this code below is the same code in SchemaRevision.ToJsonStructure, is there a way to consolidate?
                //consolidating might mean going to a static translator, the method would take a list of BaseFields 
                JObject childProperties = new JObject();

                foreach (BaseField field in ChildFields.OrderBy(x => x.OrdinalPosition).ToList())
                {
                    childProperties.Add(field.Name, field.ToJsonPropertyDefinition());
                }

                definition.Add("properties", childProperties);
            }

            if (!string.IsNullOrEmpty(Description))
            {
                definition.Add("description", Description);
            }

            return definition;
        }

        protected abstract JObject GetJsonTypeDefinition();
    }
}
