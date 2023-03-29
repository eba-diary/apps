using Sentry.data.Core.GlobalEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Sentry.data.Web
{
    public class MigrationHistoryModel
    {
        public int MigrationHistoryId { get; set; }
        public DateTime CreateDateTime { get; set; }

        public string SourceNamedEnvironment { get; set; }
        public string TargetNamedEnvironment { get; set; }

        public int? SourceDatasetId { get; set; }
        public int? TargetDatasetId { get; set; }

        public IList<MigrationHistoryDetailModel> MigrationHistoryDetailModels { get; set; }

        public string SourceDatasetName { get; set; }
    }
}