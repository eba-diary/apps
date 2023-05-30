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

        public string ConsumeDatasetGroupName { get; set; }
        public string ProducerDatasetGroupName { get; set; }
        public string ConsumeAssetGroupName { get; set; }
        public string ProducerAssetGroupName { get; set; }

        public bool InheritanceStatus { get; set; }
    }
}