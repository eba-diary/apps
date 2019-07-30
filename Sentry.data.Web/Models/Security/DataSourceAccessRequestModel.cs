﻿using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class DataSourceAccessRequestModel : AccessRequestModel
    {

        [Required]
        [DisplayName("AD Group")]
        public string AdGroupName { get; set; }

        public List<SelectListItem> AllAdGroups { get; set; }

    }
}