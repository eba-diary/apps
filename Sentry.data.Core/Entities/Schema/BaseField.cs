﻿using System;
using System.Collections.Generic;

namespace Sentry.data.Core
{
    public abstract class BaseField
    {
        private BaseField _parentField;
        private IList<BaseField> _childFields = new List<BaseField>();


        public virtual Guid FieldId { get; set; }
        public virtual string Name { get; set; }
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
        public virtual IEnumerable<BaseField> ChildFields
        {
            get
            {
                return _childFields;
            }
        }

        //abstract properties
        public abstract SchemaDatatypes Type { get; set; }
    }
}
