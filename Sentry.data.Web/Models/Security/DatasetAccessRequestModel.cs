using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class DatasetAccessRequestModel : AccessRequestModel
    {

        [DisplayName("AD Group")]
        public string AdGroupName { get; set; }

        public List<SelectListItem> AllAdGroups { get; set; }
        [DisplayName("Amazon Resource Name")]
        public string AwsArn { get; set; }
    }
}