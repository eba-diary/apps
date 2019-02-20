
using System.Collections.Generic;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class NotificationAccessRequestModel : AccessRequestModel
    {

        public string PermssionForUserId { get; set; }

        public List<SelectListItem> AllSecurableObjects { get; set; }


    }
}