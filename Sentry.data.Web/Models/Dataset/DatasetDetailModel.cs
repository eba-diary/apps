using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sentry.data.Core;
using Sentry.data.Infrastructure;

namespace Sentry.data.Web
{
    public class DatasetDetailModel : BaseDatasetModel
    {
        public DatasetDetailModel(Dataset ds, IAssociateInfoProvider associateService, IDatasetContext datasetContext) : base(ds, associateService, datasetContext)
        {

        }

        public string ArtifactLink { get; set; }
        public string LocationType { get; set; }
        public string MailtoLink { get; set; }
    }
}