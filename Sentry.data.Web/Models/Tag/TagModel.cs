using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sentry.Core;

namespace Sentry.data.Web
{
    public class TagModel
    {
        public int TagId { get; set; }
        [DisplayName("Tag Name")]
        public string TagName { get; set; }
        [DisplayName("Description")]
        public string Description { get; set; }
        public string CreationUserId { get; set; }

        [DisplayName("Group")]
        public string SelectedTagGroup { get; set; }


        public List<SelectListItem> AllTagGroups { get; set; }

    }
}