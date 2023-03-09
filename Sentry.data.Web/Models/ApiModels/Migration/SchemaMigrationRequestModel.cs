using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models.ApiModels.Migration
{
    public class SchemaMigrationRequestModel : BaseMigrationRequestModel
    {
        public int SourceSchemaId { get; set; }
        public string TargetDataFlowNamedEnviornment { get; set; }
    }
}