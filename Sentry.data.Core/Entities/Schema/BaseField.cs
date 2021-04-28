﻿using System;
using System.Collections.Generic;

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
    }
}