﻿using System;
using System.Collections.Generic;

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
        public virtual Schema ParentSchema { get; set; }
        public virtual string SchemaRevision_Name { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual DateTime CreatedDTM { get; set; }
        public virtual DateTime LastUpdatedDTM { get; set; }
        public virtual int SchemaStruct_Id { get; set; }
        public virtual IList<BaseField> Fields { get; set; }
    }
}
