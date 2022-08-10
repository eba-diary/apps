using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class SupportLinkModel
    {
        public int SupportLinkId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string Url { get; set; }
    }
}