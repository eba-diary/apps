using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.ApiModels.Migration
{
    public class MigrationRequestModel
    {
        public int TargetDatasetId { get; set; }
        public string TargetDatasetNamedEnvironment { get; set; }
    }
}