using Sentry.data.Core;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Web
{
    public class ViewAssetDetailsModel : BaseAssetModel
    {
        public ViewAssetDetailsModel(Asset asset, UserService userService) : base(asset)
        {
            this.Categories = asset.Categories.Select(((c) => new BaseCategoryModel(c))).ToList();
        }

        public List<BaseCategoryModel> Categories { get; set; }

    }
}
