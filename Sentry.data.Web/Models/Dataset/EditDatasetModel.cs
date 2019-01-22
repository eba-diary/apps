using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.data.Core;
using Sentry.data.Infrastructure;

namespace Sentry.data.Web
{
    public class EditDatasetModel : BaseDatasetModel
    {
        public EditDatasetModel()
        {
        }

        public EditDatasetModel(Dataset ds, IAssociateInfoProvider associateService) : base(ds, associateService)
        {
             
        }

        [Required]
        [DisplayName("Sentry Owner")]
        public string OwnerID { get; set; }

        public string TagString { get; set; }

        [Required]
        [DisplayName("Data Classification")]
        public int? DataClassification { get; set; }
    }
}
