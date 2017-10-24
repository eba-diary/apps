using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;
using System.Web.Mvc;
using System.ComponentModel;
using Sentry.data.Infrastructure;

namespace Sentry.data.Web
{
    public class CreateAssetNotificationModel : BaseAssetNotificationModel
    {
        public CreateAssetNotificationModel() { }
        public CreateAssetNotificationModel(AssetNotifications an, IAssociateInfoProvider associateInfoService) : base(an, associateInfoService) { }

        public IEnumerable<SelectListItem> AllDataAssets { get; set; }
        public IEnumerable<SelectListItem> AllSeverities { get; set; }

        [DisplayName("Severity")]
        public int SeverityID { get; set; }

        [DisplayName("Asset")]
        public int DataAssetID { get; set; }
    }
}