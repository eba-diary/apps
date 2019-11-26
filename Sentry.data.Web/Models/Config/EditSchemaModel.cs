using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class EditSchemaModel
    {

        public EditSchemaModel()
        {


        }

        [Required]
        [DisplayName("Schema Name")]
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual Boolean IsForceMatch { get; set; }
        public virtual Boolean IsPrimary { get; set; }

        public virtual int Schema_Id { get; set; }
        public virtual string FileType { get; set; }
        public virtual int FileTypeId { get; set; }
        public virtual string Delimiter { get; set; }
        public virtual Boolean HasHeader { get; set; }

        public virtual int DatasetId { get; set; }
    }
}