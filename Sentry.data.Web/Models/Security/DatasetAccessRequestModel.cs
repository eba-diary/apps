using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class DatasetAccessRequestModel : AccessRequestModel
    {

        [Required]
        [DisplayName("AD Group")]
        public string AdGroupName { get; set; }

        public List<SelectListItem> AllAdGroups { get; set; }

        public override string SecurableObjectLabel
        {
            get
            {
                return Core.GlobalConstants.SecurableEntityName.DATASET;
            }
        }
    }
}