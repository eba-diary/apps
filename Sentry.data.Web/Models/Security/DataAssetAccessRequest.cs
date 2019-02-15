using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class DataAssetAccessRequest : AccessRequestModel
    {

        public string PermssionForUserId { get; set; }

        public override string SecurableObjectLabel
        {
            get
            {
                return Core.GlobalConstants.SecurableEntityName.DATA_ASSET;
            }
        }

    }
}