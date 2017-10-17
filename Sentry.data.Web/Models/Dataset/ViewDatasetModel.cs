using Sentry.data.Core;
using Sentry.data.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Sentry.data.Web
{
    public class ViewDatasetModel : BaseDatasetModel
    {
        public ViewDatasetModel()
        {
        }

        public ViewDatasetModel(Dataset ds, IAssociateInfoProvider associateService) : base(ds, associateService)
        {
        }

        public List<BaseCategoryModel> Folders { get; set; }
    }
}
