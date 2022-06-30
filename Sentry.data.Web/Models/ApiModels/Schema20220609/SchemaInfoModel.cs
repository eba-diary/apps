using Sentry.data.Core;
using System;
using System.Collections.Generic;

namespace Sentry.data.Web.Models.ApiModels.Schema20220609
{
    public class SchemaInfoModel : Schema.SchemaInfoModelBase
    {
        public IList<SchemaConsumptionModel> ConsumptionDetails { get; set; }
    }


}