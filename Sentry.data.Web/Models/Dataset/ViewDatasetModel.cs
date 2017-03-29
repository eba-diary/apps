using Sentry.data.Core;
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

        public ViewDatasetModel(Dataset ds, UserService userService) : base(ds)
        {
        }

        public List<BaseCategoryModel> Folders { get; set; }
    }
}
