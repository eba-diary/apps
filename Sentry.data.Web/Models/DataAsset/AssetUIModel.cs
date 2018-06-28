using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sentry.data.Web.Models
{
    public class AssetUIModel
    {
        public AssetUIModel(DataAsset da)
        {
            Id = da.Id;
            Name = da.Name;
            DisplayName = da.DisplayName;
        }

        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string DisplayName { get; set; }
    }
}