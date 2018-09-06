using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;
using System.ComponentModel;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class CreateExtensionMapModel
    {
        public CreateExtensionMapModel()
        {
            ExtensionMappings = new List<MediaTypeExtension>();
        }

        [DisplayName("Extension Mapping")]
        public virtual List<MediaTypeExtension> ExtensionMappings { get; set; }
    }
}