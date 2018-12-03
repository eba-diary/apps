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
    public class CreateBusinessIntelligenceModel : BusinessIntelligenceModel
    {
        public CreateBusinessIntelligenceModel()
        {
        }

        [Required]
        [DisplayName("Categories")]
        public int CategoryIDs { get; set; }
    }
}
