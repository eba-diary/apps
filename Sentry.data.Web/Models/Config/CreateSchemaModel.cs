﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models
{
    public class CreateSchemaModel
    {
        public CreateSchemaModel()
        {


        }

        [Required]
        [DisplayName("Schema Name")]
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual Boolean IsForceMatch { get; set; }
        public virtual Boolean IsPrimary { get; set; }
        public virtual int DataObject_ID { get; set; }

        public virtual int DatasetId { get; set; }
    }
}