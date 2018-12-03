using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Sentry.data.Core;
using Sentry.data.Infrastructure;

namespace Sentry.data.Web
{
    public class EditBusinessIntelligenceModel : BusinessIntelligenceModel
    {
        public EditBusinessIntelligenceModel()
        {

        }

        public EditBusinessIntelligenceModel(Dataset ds, IAssociateInfoProvider associateInfoProvider) : base(ds, associateInfoProvider)
        {
            CategoryIDs = ds.DatasetCategory.Id;
        }

        [Required]
        [DisplayName("Category")]
        public int CategoryIDs { get; set; }
    }
}